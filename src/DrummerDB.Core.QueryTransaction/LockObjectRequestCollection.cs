using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class LockObjectRequestCollection : ICollection<LockObjectRequest>
    {
        #region Private Fields
        private List<LockObjectRequest> _requests;
        #endregion

        #region Public Properties
        public int Count => _requests.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public LockObjectRequestCollection()
        {
            _requests = new List<LockObjectRequest>();
        }

        public LockObjectRequestCollection(int length)
        {
            _requests = new List<LockObjectRequest>(length);
        }
        #endregion

        #region Public Methods
        public LockObjectRequest this[int index]
        {
            get { return _requests[index]; }
            set { _requests[index] = value; }
        }

        public LockObjectRequest GetLockObjectRequest(Guid id)
        {
            foreach (var request in _requests)
            {
                if (id == request.Id)
                {
                    return request;
                }
            }

            return new LockObjectRequest();
        }

        public void Add(LockObjectRequest item)
        {
            if (!Contains(item))
            {
                _requests.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                    $"There is already an expression with id {item.Id.ToString()}");
            }
        }

        public void Clear()
        {
            _requests.Clear();
        }

        public bool Contains(Guid id)
        {
            foreach (var request in _requests)
            {
                if (request.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(Guid lockObjectId, string objectName)
        {
            foreach (var request in _requests)
            {
                if (request.ObjectId == lockObjectId &&
                    string.Equals(request.ObjectName, objectName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(LockObjectRequest item)
        {
            foreach (var expresion in _requests)
            {
                if (expresion.Id == item.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(LockObjectRequest[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _requests.Count; i++)
            {
                array[i + arrayIndex] = _requests[i];
            }
        }

        public IEnumerator<LockObjectRequest> GetEnumerator()
        {
            return new LockObjectRequestEnumerator(this);
        }

        public bool Remove(LockObjectRequest item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _requests.Count; i++)
            {
                LockObjectRequest curEx = _requests[i];

                if (item.Id == curEx.Id)
                {
                    _requests.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LockObjectRequestEnumerator(this);
        }


        #endregion

        #region Private Methods
        #endregion
    }
}
