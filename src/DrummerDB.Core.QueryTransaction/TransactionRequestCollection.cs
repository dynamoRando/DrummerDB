using Drummersoft.DrummerDB.Core.Structures;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Drummersoft.DrummerDB.Core.QueryTransaction 
{
    class TransactionRequestCollection : IEnumerable<TransactionRequest>
    {
        #region Private Fields
        private List<TransactionRequest> _transactions;
        #endregion

        #region Public Properties
        public int Count => _transactions.Count;
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public TransactionRequestCollection()
        {
            _transactions = new List<TransactionRequest>();
        }

        public TransactionRequestCollection(int length)
        {
            _transactions = new List<TransactionRequest>(length);
        }
        #endregion

        #region Public Methods
        public TransactionRequest this[int index]
        {
            get { return _transactions[index]; }
            set { _transactions[index] = value; }
        }

        public TransactionRequest GetTransactionEntryRequest(Guid batchTransactionId)
        {
            foreach (var xact in _transactions)
            {
                if (xact.TransactionBatchId == batchTransactionId)
                {
                    return xact;
                }
            }

            return new TransactionRequest();
        }

        public void Add(TransactionRequest item)
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

        public bool Contains(TransactionRequest item)
        {
            foreach (var xact in _transactions)
            {
                if (xact.TransactionBatchId == item.TransactionBatchId)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(TransactionRequest[] array, int arrayIndex)
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

        public IEnumerator<TransactionRequest> GetEnumerator()
        {
            return new TransactionRequestEnumerator(this);
        }

        public bool Remove(TransactionRequest item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _transactions.Count; i++)
            {
                TransactionRequest curXact = (TransactionRequest)_transactions[i];

                if (curXact.TransactionBatchId == item.TransactionBatchId)
                {
                    _transactions.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TransactionRequestEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
