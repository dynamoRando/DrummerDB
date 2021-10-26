using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
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
    internal class TableReadBooleanFilter : ITableReadFilter
    {
        #region Private Fields
        #endregion

        #region Public Properties
        public int Order { get; set; }
        public BooleanComparisonOperator ComparisonOperator { get; set; }
        public ITableReadFilter LeftFilter { get; set; }
        public ITableReadFilter RightFilter { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public List<RowAddress> GetRows(IDbManager db, TransactionRequest transaction, TransactionMode transactionMode)
        {
            var result = new List<RowAddress>();

            var leftValues = GetRows(LeftFilter, db, transaction, transactionMode);
            var rightValues = GetRows(RightFilter, db, transaction, transactionMode);

            if (ComparisonOperator == BooleanComparisonOperator.And)
            {
                foreach (var lValue in leftValues)
                {
                    foreach (var rValue in rightValues)
                    {
                        if (lValue.RowId == rValue.RowId)
                        {
                            result.Add(rValue);
                        }
                    }
                }
            }

            if (ComparisonOperator == BooleanComparisonOperator.Or)
            {
                result.AddRange(leftValues);
                result.AddRange(rightValues);
                return result;
            }

            return result;
        }
        #endregion

        #region Private Methods
        private List<RowAddress> GetRows(ITableReadFilter filter, IDbManager db, TransactionRequest transaction, TransactionMode transactionMode)
        {
            if (filter is TableReadFilter)
            {
                var tableFilter = filter as TableReadFilter;

                if (tableFilter.ComparisonOperator == ValueComparisonOperator.Equals)
                {
                    var table = db.GetTable(new TreeAddress(tableFilter.TableRowValue.DatabaseId, tableFilter.TableRowValue.TableId, tableFilter.TableRowValue.SchemaId));
                    return table.FindRowAddressesWithValue(tableFilter.TableRowValue.RowValue, transaction, transactionMode);
                }
                else
                {
                    var table = db.GetTable(new TreeAddress(tableFilter.TableRowValue.DatabaseId, tableFilter.TableRowValue.TableId, tableFilter.TableRowValue.SchemaId));
                    return table.FindRowAddressesWithValue(tableFilter.TableRowValue.RowValue, tableFilter.ComparisonOperator, transaction, transactionMode);
                }
            }
            else
            {
                // recursively get the rows needed
                return filter.GetRows(db, transaction, transactionMode);
            }

            return null;
        }
        #endregion
    }
}
