using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        #endregion

        #region Public Properties
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public int Order { get; set; }
        public readonly TreeAddress Address;
        public ValueAddressCollection Result => _result;
        #endregion

        #region Constructors
        public TableReadOperator(IDbManager db, TreeAddress address, string[] columnNames)
        {
            _db = db;
            Address = address;
            _columnNames = columnNames;
            _result = new ValueAddressCollection();
        }

        public TableReadOperator(IDbManager db, TreeAddress address, string[] columnNames, ITableReadFilter filter) : this(db, address, columnNames)
        {
            _filter = filter;
        }

        public TableReadOperator(IDbManager db, TreeAddress address, string[] columnNames, List<ITableReadFilter> filters) : this(db, address, columnNames)
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
                _result.AddRange(ExecuteNoFilter(transaction, transactionMode));
                return Result.List();
            }

            if (_filter is not null && _filters is null)
            {
                _result.AddRange(ExecuteWithFilter());
                return Result.List();
            }

            if (_filters is not null && _filter is null)
            {
                _result.AddRange(ExecuteWithFilters(transaction, transactionMode));
                return Result.List();
            }

            return null;
        }

        #endregion

        #region Private Methods
        private List<ValueAddress> ExecuteWithFilter()
        {
            throw new NotImplementedException();
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

            int maxFilterId = _filters.Max(f => f.Order);
            ITableReadFilter maxFilter = _filters.Where(filter => filter.Order == maxFilterId).FirstOrDefault();

            if (maxFilter is not null)
            {
                List<RowAddress> rows = maxFilter.GetRows(_db, transaction, transactionMode);
                Table table = _db.GetTable(Address);

                foreach (var column in _columnNames)
                {
                    result.AddRange(table.GetValuesForColumnByRows(rows, column, transaction, transactionMode));
                }
            }

            var item = result.Distinct().ToList();
            return item;
        }

        private List<ValueAddress> ExecuteNoFilter(TransactionRequest transaction, TransactionMode transactionMode)
        {
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
                    result.AddRange(table.GetAllValuesForColumn(column, transaction, transactionMode));
                }
            }

            return result;
        }
        #endregion
    }
}
