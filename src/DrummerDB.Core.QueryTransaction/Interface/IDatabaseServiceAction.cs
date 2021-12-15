using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface IDatabaseServiceAction
    {
        Guid Id { get; }

        bool Execute(TransactionRequest transaction, TransactionMode transactionMode, out string errorMessage);
    }
}
