using Drummersoft.DrummerDB.Core.Structures;
using System;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface ITransactionManager
    {
        Guid GetPendingBatchTransactionId();
        TransactionRequest EnqueueBatchTransaction(Guid batchTransactionId, string userName, Guid planId);
        bool DequeueBatchTransaction(Guid batchTransactionId);
    }
}
