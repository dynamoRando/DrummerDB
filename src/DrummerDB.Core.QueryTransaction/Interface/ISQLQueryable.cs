using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface ISQLQueryable
    {
        ValueAddressCollection Result { get; }
        int Order { get; set; }
        List<ValueAddress> Execute(TransactionRequest transaction, TransactionMode transactionMode);
    }
}
