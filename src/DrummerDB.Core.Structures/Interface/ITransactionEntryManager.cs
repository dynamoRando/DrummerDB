using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    interface ITransactionEntryManager
    {
        void AddEntry(TransactionEntry entry);
        List<TransactionEntry> GetBatch(Guid transactionBatchId);
        void RemoveEntry(TransactionEntry entry);
        TransactionEntry FindInsertTransactionForRowId(uint rowId, Guid databaseId, uint tableId);
        TransactionEntry FindUpdateTransactionForRowId(uint rowId);
        TransactionEntry FindDeleteTransactionForRowId(uint rowId);
        int GetNextSequenceNumberForBatchId(Guid transactionBatchId);
        TransactionEntry Get(TransactionEntryKey key);
    }
}
