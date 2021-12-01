using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class PredicateCollection : IEnumerable<IPredicate>
    {
        #region Private Fields
        private List<IPredicate> _parts;
        #endregion

        #region Public Properties
        public int Count => _parts.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public PredicateCollection()
        {
            _parts = new List<IPredicate>();
        }

        public PredicateCollection(int length)
        {
            _parts = new List<IPredicate>(length);
        }
        #endregion

        #region Public Methods
        public IPredicate Get(Interval interval)
        {
            foreach (var part in _parts)
            {
                if (part.Interval == interval)
                {
                    return part;
                }
            }

            return null;
        }

        public IPredicate this[int index]
        {
            get { return _parts[index]; }
            set { _parts[index] = value; }
        }

        public void Add(IPredicate item)
        {
            if (!Contains(item))
            {
                _parts.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a part with id {item.Id}");
            }
        }

        public void Clear()
        {
            _parts.Clear();
        }

        public bool Contains(Interval interval)
        {
            foreach (var part in _parts)
            {
                if (part.Interval == interval)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(IPredicate item)
        {
            foreach (var part in _parts)
            {
                if (part.Id == item.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(IPredicate[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _parts.Count; i++)
            {
                array[i + arrayIndex] = _parts[i];
            }
        }

        public IEnumerator<IPredicate> GetEnumerator()
        {
            return new PredicateEnumerator(this);
        }

        public bool Remove(Predicate item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _parts.Count; i++)
            {
                Predicate curPart = (Predicate)_parts[i];

                if (curPart.Id == item.Id)
                {
                    _parts.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(string fullText)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _parts.Count; i++)
            {
                Predicate curPart = (Predicate)_parts[i];

                if (string.Equals(curPart.FullText, fullText, StringComparison.OrdinalIgnoreCase))
                {
                    _parts.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PredicateEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
