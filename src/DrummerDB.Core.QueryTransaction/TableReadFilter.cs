﻿using Drummersoft.DrummerDB.Core.Databases.Interface;
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
    internal class TableReadFilter : ITableReadFilter
    {
        public TableRowValue TableRowValue { get; set; }
        public ValueComparisonOperator ComparisonOperator { get; set; }
        public int Order { get; set; }

        public TableReadFilter(TableRowValue value, ValueComparisonOperator operation, int order)
        {
            TableRowValue = value;
            ComparisonOperator = operation;
            Order = order;
        }

        public List<RowAddress> GetRows(IDbManager db, TransactionRequest transaction, TransactionMode transactionMode)
        {
            var table = db.GetTable(new TreeAddress(TableRowValue.DatabaseId, TableRowValue.TableId, TableRowValue.SchemaId));
            return table.FindRowAddressesWithValue(TableRowValue.RowValue, ComparisonOperator, transaction, transactionMode);
        }
    }
}
