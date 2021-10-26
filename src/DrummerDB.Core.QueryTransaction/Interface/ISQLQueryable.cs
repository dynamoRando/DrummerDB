using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface ISQLQueryable
    {
        ValueAddressCollection Result { get; }
        int Order { get; set; }
        List<ValueAddress> Execute(TransactionRequest transaction, TransactionMode transactionMode);
    }
}
