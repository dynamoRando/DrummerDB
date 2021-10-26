using Drummersoft.DrummerDB.Core.Storage.Abstract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Storage
{
    class SystemDbFileHandlerCollection : ICollection<SystemDbFileHandler>
    {
        #region Private Fields
        private List<SystemDbFileHandler> _dbs;
        #endregion

        #region Public Properties
        public int Count => _dbs.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public SystemDbFileHandlerCollection()
        {
            _dbs = new List<SystemDbFileHandler>();
        }

        public SystemDbFileHandlerCollection(int length)
        {
            _dbs = new List<SystemDbFileHandler>(length);
        }
        #endregion

        #region Public Methods
        public SystemDbFileHandler this[int index]
        {
            get { return _dbs[index]; }
            set { _dbs[index] = value; }
        }

        public void Add(SystemDbFileHandler item)
        {
            if (!Contains(item))
            {
                _dbs.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a database with id {item.DbId.ToString()}");
            }
        }

        public void Clear()
        {
            _dbs.Clear();
        }

        public SystemDbFileHandler Get(Guid dbId)
        {
            foreach (var db in _dbs)
            {
                if (db.DbId == dbId)
                {
                    return db;
                }
            }

            return null;
        }

        public SystemDbFileHandler Get(string name)
        {
            foreach (var db in _dbs)
            {
                if (string.Equals(db.DatabaseName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return db;
                }
            }

            return null;
        }

        public bool Contains(SystemDbFileHandler item)
        {
            foreach (var db in _dbs)
            {
                if (db.DbId == item.DbId)
                { return true; }
            }
            return false;
        }

        public bool Contains(string item)
        {
            foreach (var db in _dbs)
            {
                if (string.Equals(db.DatabaseName, item, StringComparison.OrdinalIgnoreCase))
                { return true; }
            }
            return false;
        }

        public void CopyTo(SystemDbFileHandler[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _dbs.Count; i++)
            {
                array[i + arrayIndex] = _dbs[i];
            }
        }

        public IEnumerator<SystemDbFileHandler> GetEnumerator()
        {
            return new SystemDbFileHandlerEnumerator(this);
        }

        public bool Remove(SystemDbFileHandler item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _dbs.Count; i++)
            {
                SystemDbFileHandler curDb = (SystemDbFileHandler)_dbs[i];

                if (curDb.DbId == item.DbId)
                {
                    _dbs.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SystemDbFileHandlerEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
