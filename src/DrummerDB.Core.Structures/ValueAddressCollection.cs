using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class ValueAddressCollection : IEnumerable<ValueAddress>
    {
        #region Private Fields
        private List<ValueAddress> _addresses;
        #endregion

        #region Public Properties
        public int Count => _addresses.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public ValueAddressCollection()
        {
            _addresses = new List<ValueAddress>();
        }

        public ValueAddressCollection(int length)
        {
            _addresses = new List<ValueAddress>(length);
        }
        #endregion

        #region Public Methods
        public List<ValueAddress> List()
        {
            return _addresses;
        }
        public void AddRange(List<ValueAddress> addresses)
        {
            _addresses.AddRange(addresses);
        }

        public ValueAddress this[int index]
        {
            get { return _addresses[index]; }
            set { _addresses[index] = value; }
        }

        public void Add(ValueAddress item)
        {
            if (!Contains(item))
            {
                _addresses.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"Duplicate address added");
            }
        }

        public void Clear()
        {
            _addresses.Clear();
        }

        public bool Contains(ValueAddress item)
        {
            return _addresses.Contains(item);
        }

        public void CopyTo(ValueAddress[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _addresses.Count; i++)
            {
                array[i + arrayIndex] = _addresses[i];
            }
        }

        public IEnumerator<ValueAddress> GetEnumerator()
        {
            return new ValueAddressEnumerator(this);
        }

        public bool Remove(ValueAddress item)
        {
            return _addresses.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ValueAddressEnumerator(this);
        }

        public List<RowAddress> GetRowAddresses()
        {
            var rows = new List<RowAddress>();

            foreach (var address in _addresses)
            {
                rows.Add(address.ToRowAddress());
            }

            return rows;
        }

        public (List<TreeAddress>, List<RowAddress>) Rows()
        {
            var tables_ = new List<TreeAddress>();
            var rows_ = new List<RowAddress>();

            foreach (var address in _addresses)
            {
                tables_.Add(address.ToTreeAddress());
                rows_.Add(address.ToRowAddress());
            }

            var tables = tables_.Distinct().ToList();
            var rows = rows_.Distinct().ToList();

            return (tables, rows);
        }

        #endregion


        #region Private Methods
        #endregion
    }
}
