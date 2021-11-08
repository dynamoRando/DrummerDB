using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class SQLLockCollection : IEnumerable<SQLLock>
    {
        #region Private Fields
        private List<SQLLock> _locks;
        #endregion

        #region Public Properties
        public int Count => _locks.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public SQLLockCollection()
        {
            _locks = new List<SQLLock>();
        }

        public SQLLockCollection(int length)
        {
            _locks = new List<SQLLock>(length);
        }
        #endregion

        #region Public Methods
        public SQLLock this[int index]
        {
            get { return _locks[index]; }
            set { _locks[index] = value; }
        }

        public SQLLock GetSQLLock(Guid id)
        {
            foreach (var sqlLock in _locks)
            {
                if (sqlLock.Id == id)
                {
                    return sqlLock;
                }
            }

            return null;
        }

        public SQLLock GetCopyOfLock(string objectName, Guid objectId)
        {
            foreach (var sqlLock in _locks)
            {
                if (sqlLock.Id == objectId &&
                    string.Equals(sqlLock.ObjectName, objectName, StringComparison.OrdinalIgnoreCase))
                {
                    return new SQLLock
                        (
                        sqlLock.Id,
                        sqlLock.LockStartTimeUTC,
                        sqlLock.UserName,
                        sqlLock.TransactionBatchId,
                        sqlLock.TransactionId,
                        sqlLock.LockType,
                        sqlLock.LockAddress,
                        sqlLock.ObjectName,
                        sqlLock.ObjectId,
                        sqlLock.ObjectType
                        );
                }
            }

            return null;
        }

        public List<SQLLock> GetCopyOfLocksForTransactionBatch(Guid transactionBatchId)
        {
            var result = new List<SQLLock>();

            foreach (var sqlLock in _locks)
            {
                if (sqlLock.TransactionBatchId == transactionBatchId)
                {
                    result.Add(GetCopyOfLock(sqlLock.ObjectName, sqlLock.ObjectId));
                }
            }

            return result;
        }

        public LockType GetLockTypeForObject(string objectName, Guid objectId)
        {
            foreach (var sqlLock in _locks)
            {
                if (string.Equals(sqlLock.ObjectName, objectName, StringComparison.OrdinalIgnoreCase)
                    && sqlLock.ObjectId == objectId)
                {
                    return sqlLock.LockType;
                }
            }

            return LockType.Unknown;
        }

        public ObjectType GetObjectTypeForObject(string objectName, Guid objectId)
        {
            foreach (var sqlLock in _locks)
            {
                if (string.Equals(sqlLock.ObjectName, objectName, StringComparison.OrdinalIgnoreCase)
                    && sqlLock.ObjectId == objectId)
                {
                    return sqlLock.ObjectType;
                }
            }

            return ObjectType.Unknown;
        }

        public void Add(SQLLock item)
        {
            if (!Contains(item))
            {
                _locks.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a lock with id {item.Id}");
            }
        }

        public void Clear()
        {
            _locks.Clear();
        }

        public bool Contains(SQLLock item)
        {
            foreach (var sqlLock in _locks)
            {
                if (sqlLock.Id == item.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(string objectName, Guid objectId)
        {
            foreach (var sqlLock in _locks)
            {
                if (string.Equals(sqlLock.ObjectName, objectName, StringComparison.OrdinalIgnoreCase)
                    && sqlLock.ObjectId == objectId)
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(SQLLock[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _locks.Count; i++)
            {
                array[i + arrayIndex] = _locks[i];
            }
        }

        public IEnumerator<SQLLock> GetEnumerator()
        {
            return new SQLLockEnumerator(this);
        }

        public bool Remove(SQLLock item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _locks.Count; i++)
            {
                SQLLock curLock = _locks[i];

                if (curLock.Id == item.Id)
                {
                    _locks.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(string objectName, Guid objectId)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _locks.Count; i++)
            {
                SQLLock curLock = _locks[i];

                if (string.Equals(curLock.ObjectName, objectName, StringComparison.OrdinalIgnoreCase)
                    && curLock.ObjectId == objectId)
                {
                    _locks.RemoveAt(i);
                    result = true;
                    break;
                }
            }

            return result;
        }

        public bool RemoveLocksForTransactionBatch(Guid transactionBatchId)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _locks.Count; i++)
            {
                SQLLock curLock = _locks[i];

                if (curLock.TransactionBatchId == transactionBatchId)
                {
                    _locks.RemoveAt(i);
                    result = true;
                }
            }

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SQLLockEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
