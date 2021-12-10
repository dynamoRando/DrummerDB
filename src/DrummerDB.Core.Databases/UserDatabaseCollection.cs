using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class UserDatabaseCollection : ICollection<UserDatabase>
    {
        #region Private Fields
        private List<UserDatabase> _userDatabases;
        #endregion

        #region Public Properties
        public int Count => _userDatabases.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public UserDatabaseCollection()
        {
            _userDatabases = new List<UserDatabase>();
        }

        public UserDatabaseCollection(int length)
        {
            _userDatabases = new List<UserDatabase>(length);
        }
        #endregion

        #region Public Methods
        public string[] Names()
        {
            string[] names = new string[Count];

            if (_userDatabases.Count > 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    names[i] = _userDatabases[i].Name;
                }
            }

            return names;
        }

        public UserDatabase this[int index]
        {
            get { return _userDatabases[index]; }
            set { _userDatabases[index] = value; }
        }

        public UserDatabase GetUserDatabase(string dbName, DatabaseType type)
        {
            foreach (var db in _userDatabases)
            {
                if (string.Equals(db.Name, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    if (db.DatabaseType == type)
                    {
                        return db;
                    }
                }
            }

            return null;
        }

        public UserDatabase GetUserDatabase(Guid dbId)
        {
            foreach (var db in _userDatabases)
            {
                if (db.Id == dbId)
                {
                    return db;
                }
            }

            return null;
        }

        public void Add(UserDatabase item)
        {
            if (!Contains(item))
            {
                _userDatabases.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                    $"There is already a database named {item.Name}");
            }
        }

        public void Clear()
        {
            _userDatabases.Clear();
        }

        public bool Contains(string dbName)
        {
            foreach (var db in _userDatabases)
            {
                if (string.Equals(db.Name, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(UserDatabase item)
        {
            foreach (var db in _userDatabases)
            {
                if (string.Equals(db.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string dbName, DatabaseType type)
        {
            foreach (var db in _userDatabases)
            {
                if (string.Equals(db.Name, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    if (db.DatabaseType == type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Contains(Guid dbId)
        {
            foreach (var db in _userDatabases)
            {
                if (db.Id == dbId)
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(UserDatabase[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _userDatabases.Count; i++)
            {
                array[i + arrayIndex] = _userDatabases[i];
            }
        }

        public IEnumerator<UserDatabase> GetEnumerator()
        {
            return new UserDatabaseEnumerator(this);
        }

        public bool Remove(string dbName)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _userDatabases.Count; i++)
            {
                UserDatabase curDb = _userDatabases[i];

                if (string.Equals(curDb.Name, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    _userDatabases.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(UserDatabase item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _userDatabases.Count; i++)
            {
                UserDatabase curDb = _userDatabases[i];

                if (string.Equals(curDb.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _userDatabases.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new UserDatabaseEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
