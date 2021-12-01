using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Manages a <see cref="TransactionEntryCollection"/>
    /// </summary>
    /// <remarks>Used to hold actual details of transactions for reporting purposes. Consumed by a TransactionManager</remarks>
    internal class TransactionEntryManager : ITransactionEntryManager
    {
        #region Private Fields
        private TransactionEntryCollection _transactions;
        #endregion

        #region Constructors
        public TransactionEntryManager()
        {
            _transactions = new TransactionEntryCollection();
        }
        #endregion

        #region Public Methods
        public void AddEntry(TransactionEntry entry)
        {
            _transactions.Add(entry);
        }

        public List<TransactionEntry> GetBatch(Guid transactionBatchId)
        {
            return _transactions.GetTransactionsForBatch(transactionBatchId);
        }

        public void RemoveEntry(TransactionEntry entry)
        {
            _transactions.Remove(entry);
        }

        public TransactionEntry FindInsertTransactionForRowId(int rowId, Guid databaseId, int tableId)
        {
            return _transactions.FindInsertTransactionForRowId(rowId, databaseId, tableId);
        }

        public TransactionEntry FindUpdateTransactionForRowId(int rowId)
        {
            return _transactions.FindUpdateTransactionForRowId(rowId);
        }

        public TransactionEntry FindDeleteTransactionForRowId(int rowId)
        {
            return _transactions.FindDeleteTransactionForRowId(rowId);
        }

        public int GetNextSequenceNumberForBatchId(Guid transactionBatchId)
        {
            return _transactions.GetMaxSequenceForBatch(transactionBatchId) + 1;
        }

        public TransactionEntry Get(TransactionEntryKey key)
        {
            return _transactions.Get(key);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
