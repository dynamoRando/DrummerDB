using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class TableReadOperator : ISQLQueryable, IQueryPlanPartOperator
    {
        #region Private Fields
        private IDbManager _db;
        private string[] _columnNames;
        private ITableReadFilter _filter;
        private List<ITableReadFilter> _filters;
        private ValueAddressCollection _result;
        private LogService _log;
        #endregion

        #region Public Properties
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public int Order { get; set; }
        public readonly TreeAddress Address;
        public ValueAddressCollection Result => _result;
        #endregion

        #region Constructors
        public TableReadOperator(IDbManager db, TreeAddress address, string[] columnNames, LogService log)
        {
            _db = db;
            Address = address;
            _columnNames = columnNames;
            _result = new ValueAddressCollection();
            _log = log;
        }

        public TableReadOperator(IDbManager db, TreeAddress address, string[] columnNames, ITableReadFilter filter, LogService log) : this(db, address, columnNames, log)
        {
            _filter = filter;
        }

        public TableReadOperator(IDbManager db, TreeAddress address, string[] columnNames, List<ITableReadFilter> filters, LogService log) : this(db, address, columnNames, log)
        {
            _filters = filters;
        }
        #endregion

        #region Public Methods
        public void SetFilter(ITableReadFilter filter)
        {
            _filter = filter;
        }

        public void SetFilters(List<ITableReadFilter> filters)
        {
            _filters = filters;
        }

        public List<ValueAddress> Execute(TransactionRequest transaction, TransactionMode transactionMode)
        {
            if (_filter is null && _filters is null)
            {
                var result = ExecuteNoFilter(transaction, transactionMode);
                _result.AddRange(result);

                return Result.List();
            }

            if (_filter is not null && _filters is null)
            {
                var result = ExecuteWithFilter(transaction, transactionMode);
                _result.AddRange(result);

                return Result.List();
            }

            if (_filters is not null && _filter is null)
            {
                var result = ExecuteWithFilters(transaction, transactionMode);
                _result.AddRange(result);

                return Result.List();
            }

            return null;
        }

        #endregion

        #region Private Methods
        private List<ValueAddress> ExecuteWithFilter(TransactionRequest transaction, TransactionMode transactionMode)
        {
            var result = new List<ValueAddress>();
            List<RowAddress> rows = _filter.GetRows(_db, transaction, transactionMode);
            Table table = _db.GetTable(Address);

            foreach (var column in _columnNames)
            {
                var results = table.GetValuesForColumnByRows(rows, column, transaction, transactionMode);
                result.AddRange(results);
            }

            var item = result.Distinct().ToList();
            return item;
        }

        private List<ValueAddress> ExecuteWithFilters(TransactionRequest transaction, TransactionMode transactionMode)
        {
            var result = new List<ValueAddress>();

            if (_filters is not null)
            {
                _filters.OrderByDescending(f => f.Order);
            }
            else
            {
                throw new ArgumentNullException(nameof(_filters));
            }

            uint maxFilterId = _filters.Max(f => f.Order);
            ITableReadFilter maxFilter = _filters.Where(filter => filter.Order == maxFilterId).FirstOrDefault();

            if (maxFilter is not null)
            {
                List<RowAddress> rows = maxFilter.GetRows(_db, transaction, transactionMode);
                var localRows = GetLocalRows(rows);

                Table table = _db.GetTable(Address);

                foreach (var column in _columnNames)
                {
                    var results = table.GetValuesForColumnByRows(localRows, column, transaction, transactionMode);
                    result.AddRange(results);
                }

                var remoteRows = GetRemoteRows(rows);
                foreach (var row in remoteRows)
                {
                    foreach (var column in _columnNames)
                    {
                        uint colId = table.GetColumn(column).Id;
                        var address = new ValueAddress
                        {
                            ColumnId = colId,
                            ColumnName = column,
                            DatabaseId = Address.DatabaseId,
                            RowId = row.RowId,
                            TableId = Address.TableId,
                            PageId = row.PageId,
                            RowOffset = row.RowOffset,
                            RemotableId = row.RemotableId,
                            SchemaId = table.Schema().Schema.SchemaGUID,
                            ParseLength = 0,
                            ValueOffset = 0
                        };

                        result.Add(address);
                    }
                }
            }

            var item = result.Distinct().ToList();
            return item;
        }

        private List<ValueAddress> ExecuteNoFilter(TransactionRequest transaction, TransactionMode transactionMode)
        {
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            Table table = _db.GetTable(Address);
            var result = new List<ValueAddress>();

            foreach (var column in _columnNames)
            {
                if (!table.HasColumn(column))
                {
                    throw new InvalidOperationException($"Table {table.Name} does not contain column {column}");
                }
                else
                {
                    var results = table.XactGetAllValuesForColumn(column, transaction, transactionMode);
                    result.AddRange(results);
                }
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return result;
        }

        private List<RowAddress> GetRemoteRows(List<RowAddress> rows)
        {
            var result = new List<RowAddress>();

            foreach (var row in rows)
            {
                if (row.RemotableId != Guid.Empty)
                {
                    result.Add(row);
                }
            }

            return result;
        }

        private List<RowAddress> GetLocalRows(List<RowAddress> rows)
        {
            var result = new List<RowAddress>();

            foreach (var row in rows)
            {
                if (row.RemotableId == Guid.Empty)
                {
                    result.Add(row);
                }
            }

            return result;
        }
        #endregion
    }
}
