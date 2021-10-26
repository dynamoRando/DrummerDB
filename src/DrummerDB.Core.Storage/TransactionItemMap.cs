using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// An in-memory map of the transaction log. Maintains transaction entry file offsets, completed status, etc.
    /// </summary>
    internal class TransactionItemMap
    {
        #region Private Fields
        private ConcurrentBag<TransactionItem> _items;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public TransactionItemMap()
        {
            _items = new ConcurrentBag<TransactionItem>();
        }
        #endregion

        #region Public Methods
        public int Count()
        {
            return _items.Count();
        }

        public long MaxOrder()
        {
            return _items.Max(item => item.Order);
        }

        public List<TransactionItem> GetOpenTransactions()
        {
            return _items.Where(item => item.IsCompleted == false).ToList();
        }

        public void CompleteTransaction(TransactionEntryKey key)
        {
            TransactionItem item = _items.Where(item => item.Key == key).FirstOrDefault();

            if (item is not null)
            {
                item.IsCompleted = true;
            }
        }

        /// <summary>
        /// Returns the file offset for the specified transaction id. If the transaction is not found, it will return 0.
        /// </summary>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>The file offset for the transaction, or 0 if not found.</returns>
        public long GetOffset(TransactionEntryKey key)
        {
            TransactionItem item = _items.Where(item => item.Key == key).FirstOrDefault();

            return item is null ? 0 : item.Offset;
        }

        public int GetSize(TransactionEntryKey key)
        {
            TransactionItem item = _items.Where(item => item.Key == key).FirstOrDefault();

            return item is null ? 0 : item.Size;
        }

        public void UpdateOffset(TransactionEntry entry, long offset)
        {
            TransactionItem item = _items.Where(item => item.Key == entry.Key).FirstOrDefault();

            if (item is not null)
            {
                item.Offset = offset;
            }
        }

        public void AddItem(TransactionItem item)
        {
            _items.Add(item);
        }

        public long GetOrder(TransactionEntryKey key)
        {
            TransactionItem item = _items.Where(item => item.Key == key).FirstOrDefault();
            return item is null ? 0 : item.Order;
        }

        public bool HasTransaction(TransactionEntryKey key)
        {
            return _items.Any(item => item.Key == key);
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
