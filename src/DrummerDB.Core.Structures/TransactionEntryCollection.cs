using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Provides functions for managing a collection of <see cref="TransactionEntry"/>s.
    /// </summary>
    /// <remarks>Usually only used in <see cref="TransactionEntryManager"/></remarks>
    class TransactionEntryCollection : IEnumerable<TransactionEntry>
    {
        #region Private Fields
        private List<TransactionEntry> _transactions;
        #endregion

        #region Public Properties
        public int Count => _transactions.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public TransactionEntryCollection()
        {
            _transactions = new List<TransactionEntry>();
        }

        public TransactionEntryCollection(int length)
        {
            _transactions = new List<TransactionEntry>(length);
        }
        #endregion

        #region Public Methods
        public TransactionEntry Get(TransactionEntryKey key)
        {
            foreach (var xact in _transactions)
            {
                if (xact.Key == key)
                {
                    return xact;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches the collection for a <see cref="InsertTransaction"/> with the specified row id, or <c>NULL</c>
        /// </summary>
        /// <param name="rowId">The row Id to find</param>
        /// <returns>The <see cref="InsertTransaction"/> with the specified row, or <c>NULL</c></returns>
        public TransactionEntry FindInsertTransactionForRowId(int rowId, Guid databaseId, int tableId)
        {
            foreach (var xact in _transactions)
            {
                if (xact.Action is InsertTransaction)
                {
                    var iAction = xact.Action as InsertTransaction;
                    if (iAction.Address.RowId == rowId && iAction.Address.TableId == tableId && iAction.Address.DatabaseId == databaseId)
                    {
                        return xact;
                    }
                }
            }

            return null;
        }

        public TransactionEntry FindUpdateTransactionForRowId(int rowId)
        {
            foreach (var xact in _transactions)
            {
                if (xact.Action is UpdateTransaction)
                {
                    var iAction = xact.Action as UpdateTransaction;
                    if (iAction.Address.RowId == rowId)
                    {
                        return xact;
                    }
                }
            }

            return null;
        }

        public TransactionEntry FindDeleteTransactionForRowId(int rowId)
        {
            foreach (var xact in _transactions)
            {
                if (xact.Action is DeleteTransaction)
                {
                    var iAction = xact.Action as DeleteTransaction;
                    if (iAction.Address.RowId == rowId)
                    {
                        return xact;
                    }
                }
            }

            return null;
        }


        public TransactionEntry this[int index]
        {
            get { return _transactions[index]; }
            set { _transactions[index] = value; }
        }

        public void Add(TransactionEntry item)
        {
            if (!Contains(item))
            {
                _transactions.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a transaction for batch {item.TransactionBatchId.ToString()}");
            }
        }

        public void Clear()
        {
            _transactions.Clear();
        }

        public bool Contains(TransactionEntry item)
        {
            foreach (var xact in _transactions)
            {
                if (xact.Key == item.Key)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsBatch(Guid transactionBatchId)
        {
            foreach (var xact in _transactions)
            {
                if (xact.TransactionBatchId == transactionBatchId)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Conains(TransactionEntryKey key)
        {
            foreach (var xact in _transactions)
            {
                if (xact.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(TransactionEntry[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _transactions.Count; i++)
            {
                array[i + arrayIndex] = _transactions[i];
            }
        }

        public IEnumerator<TransactionEntry> GetEnumerator()
        {
            return new TransactionEntryEnumerator(this);
        }

        public bool Remove(TransactionEntry item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _transactions.Count; i++)
            {
                TransactionEntry curXact = (TransactionEntry)_transactions[i];

                if (curXact.Key == item.Key)
                {
                    _transactions.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public int TransactionBatchCount(Guid transactionBatchId)
        {
            int count = 0;
            foreach (var transaction in _transactions)
            {
                if (transaction.TransactionBatchId == transactionBatchId)
                {
                    count++;
                }
            }

            return count;
        }

        public List<TransactionEntry> GetTransactionsForBatch(Guid transactionBatchId)
        {
            List<TransactionEntry> list = new List<TransactionEntry>();
            if (ContainsBatch(transactionBatchId))
            {
                list = new List<TransactionEntry>(TransactionBatchCount(transactionBatchId));
                foreach (var xact in _transactions)
                {
                    if (xact.TransactionBatchId == transactionBatchId)
                    {
                        list.Add(xact);
                    }
                }
            }

            return list;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TransactionEntryEnumerator(this);
        }

        public int GetMaxSequenceForBatch(Guid transactionBatchId)
        {
            int max = 0;
            var xacts = GetTransactionsForBatch(transactionBatchId);
            foreach (var xact in xacts)
            {
                if (xact.Key.SequenceNumber > max)
                {
                    max = xact.Key.SequenceNumber;
                }
            }

            return max;
        }
        #endregion


        #region Private Methods
        #endregion
    }
}
