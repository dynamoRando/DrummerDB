using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    interface ITransactionEntryManager
    {
        void AddEntry(TransactionEntry entry);
        List<TransactionEntry> GetBatch(Guid transactionBatchId);
        void RemoveEntry(TransactionEntry entry);
        TransactionEntry FindInsertTransactionForRowId(int rowId, Guid databaseId, int tableId);
        TransactionEntry FindUpdateTransactionForRowId(int rowId);
        TransactionEntry FindDeleteTransactionForRowId(int rowId);
        int GetNextSequenceNumberForBatchId(Guid transactionBatchId);
        TransactionEntry Get(TransactionEntryKey key);
    }
}
