using Drummersoft.DrummerDB.Core.Storage.Abstract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Storage
{
    internal class UserDbFileHandlerCollection : ICollection<UserDbFileHandler>
    {
        #region Private Fields
        private List<UserDbFileHandler> _dbs;
        #endregion

        #region Public Properties
        public int Count => _dbs.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public UserDbFileHandlerCollection()
        {
            _dbs = new List<UserDbFileHandler>();
        }

        public UserDbFileHandlerCollection(int length)
        {
            _dbs = new List<UserDbFileHandler>(length);
        }
        #endregion

        #region Public Methods
        public UserDbFileHandler this[int index]
        {
            get { return _dbs[index]; }
            set { _dbs[index] = value; }
        }

        public void Add(UserDbFileHandler item)
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

        public UserDbFileHandler Get(Guid dbId)
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

        public UserDbFileHandler Get(string name)
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

        public bool Contains(UserDbFileHandler item)
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

        public void CopyTo(UserDbFileHandler[] array, int arrayIndex)
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

        public IEnumerator<UserDbFileHandler> GetEnumerator()
        {
            return new UserDbFileHandlerEnumerator(this);
        }

        public bool Remove(string dbName)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _dbs.Count; i++)
            {
                UserDbFileHandler curDb = (UserDbFileHandler)_dbs[i];

                if (string.Equals(curDb.DatabaseName, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    _dbs.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(UserDbFileHandler item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _dbs.Count; i++)
            {
                UserDbFileHandler curDb = (UserDbFileHandler)_dbs[i];

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
            return new UserDbFileHandlerEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
