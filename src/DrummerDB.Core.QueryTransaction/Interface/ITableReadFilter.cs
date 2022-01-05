using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface ITableReadFilter
    {
        uint Order { get; set; }
        public List<RowAddress> GetRows(IDbManager db, TransactionRequest transaction, TransactionMode transactionMode);
    }
}
