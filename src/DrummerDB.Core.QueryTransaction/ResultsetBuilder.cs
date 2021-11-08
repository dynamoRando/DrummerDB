using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class ResultsetBuilder
    {
        #region Private Fields
        private ResultsetLayout _layout;
        private IDbManager _db;
        private LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public ResultsetBuilder(ResultsetLayout layout, IDbManager db, LogService log)
        {
            _layout = layout;
            _db = db;
            _log = log;
        }
        #endregion

        #region Public Methods
        public Resultset Build(List<ValueAddress> addresses, TransactionRequest transaction, ref Resultset resultSet)
        {
            List<ResultsetValue[]> resultRows = null;
            List<ColumnSchemaStruct> resultColumns = null;

            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
                _log.Info($"{LogService.GetCurrentMethod()} - Total Addresses: {addresses.Count.ToString()}");
            }

            if (_layout is not null)
            {
                _layout.Columns.OrderBy(column => column.Order);
                resultColumns = new List<ColumnSchemaStruct>(_layout.Columns.Count);
                
                // build out the columns first
                foreach (var column in _layout.Columns)
                {
                    if (column is ResultsetSourceTable)
                    {
                        var item = column as ResultsetSourceTable;
                        var table = _db.GetTable(item.Table);
                        var colStruct = table.GetColumnStruct(item.ColumnId);
                        resultColumns.Add(colStruct);
                    }
                }

                // now build out the rows
                var tables = GetTables(addresses);

                if (tables.Count == 1)
                {
                    resultRows = GetRowsForSingleTable(addresses, transaction, tables);
                }
                else if (tables.Count > 1)
                {
                    // need to design later how to handle multiple columns from different tables
                    throw new NotImplementedException();
                }
            }

            if (resultColumns is not null)
            {
                resultSet.Columns = resultColumns.ToArray();
            }

            if (resultRows is not null)
            {
                resultSet.Rows = resultRows;
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return resultSet;
        }

        public Resultset Build(List<ValueAddress> addresses, QueryExpressionCollection collection)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private List<ResultsetValue[]> GetRowsForSingleTable(List<ValueAddress> addresses, TransactionRequest transaction, List<TreeAddress> tables)
        {
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            List<ResultsetValue[]> resultRows;
            var tableAddress = tables.First();
            List<int> rows = GetRowsForTable(tableAddress, addresses);
            resultRows = new List<ResultsetValue[]>(rows.Count);

            foreach (var row in rows)
            {
                ResultsetValue[] rsRow = GetValueForRow(addresses, transaction, tableAddress, row);

                resultRows.Add(rsRow);
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return resultRows;
        }

        private ResultsetValue[] GetValueForRow(List<ValueAddress> addresses, TransactionRequest transaction, TreeAddress tableAddress, int row)
        {
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            var rsRow = new ResultsetValue[_layout.Columns.Count];
            int rsi = 0;

            foreach (var column in _layout.Columns)
            {
                if (column is ResultsetSourceTable)
                {
                    var rsColumn = column as ResultsetSourceTable;
                    Table table = _db.GetTable(rsColumn.Table);

                    List<ValueAddress> rowValues = GetValuesForRow(tableAddress, row, addresses);

                    foreach (var value in rowValues)
                    {
                        if (value.ColumnId == value.ColumnId)
                        {
                            // this may not be correct
                            rsRow[rsi] = table.GetValueAtAddress(value, transaction);
                            rsi++;
                        }
                    }

                    rsi = 0;
                    break;
                }
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return rsRow;
        }

        /// <summary>
        /// Filters an unfiltered list of values for the specified table and row and returns the values for that row
        /// </summary>
        /// <param name="table">The table to filter down  to</param>
        /// <param name="rowId">The row id to filter down to</param>
        /// <param name="values">An unfiltered list of values</param>
        /// <returns>The values for the specified table and row</returns>
        private List<ValueAddress> GetValuesForRow(TreeAddress table, int rowId, List<ValueAddress> values)
        {
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            var result = new List<ValueAddress>();

            var tableValues = GetAddressesForTable(table, values);
            foreach (var value in tableValues)
            {
                if (value.RowId == rowId)
                {
                    if (!result.Contains(value))
                    {
                        result.Add(value);
                    }

                }
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return result;
        }

        /// <summary>
        /// Takes a list of value addresses and returns only the values for a specified Table
        /// </summary>
        /// <param name="table">The address of the table</param>
        /// <param name="addresses">The unfiltered list of all addresses</param>
        /// <returns>A list of values filtered down for the specified table</returns>
        private List<ValueAddress> GetAddressesForTable(TreeAddress table, List<ValueAddress> addresses)
        {
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            var result = new List<ValueAddress>();

            /*
            var result = new ConcurrentBag<ValueAddress>();

            Parallel.ForEach(addresses, address => 
            {
                if (IsAddressOfTable(table, address))
                {
                    result.Add(address);
                }
            });
            */

            foreach (var address in addresses)
            {
                if (address.DatabaseId == table.DatabaseId && address.TableId == table.TableId)
                {
                    result.Add(address);
                }
            }
           
            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            //var x = result.OrderBy(r => r.ColumnId).ToList();
            //return x;
            return result;
        }

        /// <summary>
        /// Takes a list of values and a table and returns a list of distinct rows for the specified table
        /// </summary>
        /// <param name="table">The table to filter on</param>
        /// <param name="addresses">An unfiltered list of addresses</param>
        /// <returns>A distinct list of row ids for the specified table</returns>
        private List<int> GetRowsForTable(TreeAddress table, List<ValueAddress> addresses)
        {
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            var result = new List<int>();

            var tableAddresses = GetAddressesForTable(table, addresses);
            foreach (var add in tableAddresses)
            {
                if (!result.Contains(add.RowId))
                {
                    result.Add(add.RowId);
                }
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return result;
        }

        /// <summary>
        /// Takes a list of value addresses and returns a distinct list of table (tree addresses)
        /// </summary>
        /// <param name="addreses">The addresses to sort</param>
        /// <returns>A list of distinct tables (tree addresses)</returns>
        private List<TreeAddress> GetTables(List<ValueAddress> addresses)
        {

            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            var result = new List<TreeAddress>();

            foreach (var address in addresses)
            {
                var table = new TreeAddress(address.DatabaseId, address.TableId, address.SchemaId);
                if (!result.Contains(table))
                {
                    result.Add(table);
                }
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return result;
        }

        private bool IsAddressOfTable(TreeAddress table, ValueAddress address)
        {
            if (address.DatabaseId == table.DatabaseId && address.TableId == table.TableId)
            {
                return true;
            }
            return false;
        }
        #endregion

    }
}
