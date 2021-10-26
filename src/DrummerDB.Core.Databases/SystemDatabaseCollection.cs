using System.Collections;
using System.Collections.Generic;
using System;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class SystemDatabaseCollection : IEnumerable<SystemDatabase>
    {
        #region Private Fields
        private List<SystemDatabase> _SystemDatabases;
        #endregion

        #region Public Properties
        public int Count => _SystemDatabases.Count;
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public SystemDatabaseCollection()
        {
            _SystemDatabases = new List<SystemDatabase>();
        }

        public SystemDatabaseCollection(int length)
        {
            _SystemDatabases = new List<SystemDatabase>(length);
        }
        #endregion

        #region Public Methods
        public string[] Names()
        {
            string[] names = new string[Count];

            for (int i = 0; i <= Count; i++)
            {
                names[i] = _SystemDatabases[i].Name;
            }

            return names;
        }

        public SystemDatabase this[int index]
        {
            get { return _SystemDatabases[index]; }
            set { _SystemDatabases[index] = value; }
        }

        public SystemDatabase GetSystemDatabase(string dbName)
        {
            foreach (var db in _SystemDatabases)
            {
                if (string.Equals(db.Name, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    return db;
                }
            }

            return null;
        }

        public void Add(SystemDatabase item)
        {
            if (!Contains(item))
            {
                _SystemDatabases.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a database named {item.Name}");
            }
        }

        public void Clear()
        {
            _SystemDatabases.Clear();
        }

        public bool Contains(SystemDatabase item)
        {
            foreach (var db in _SystemDatabases)
            {
                if (string.Equals(db.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string dbName)
        {
            foreach (var db in _SystemDatabases)
            {
                if (string.Equals(db.Name, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(SystemDatabase[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _SystemDatabases.Count; i++)
            {
                array[i + arrayIndex] = _SystemDatabases[i];
            }
        }

        public IEnumerator<SystemDatabase> GetEnumerator()
        {
            return new SystemDatabaseEnumerator(this);
        }

        public bool Remove(SystemDatabase item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _SystemDatabases.Count; i++)
            {
                SystemDatabase curDb = (SystemDatabase)_SystemDatabases[i];

                if (string.Equals(curDb.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _SystemDatabases.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(string dbName)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _SystemDatabases.Count; i++)
            {
                SystemDatabase curDb = (SystemDatabase)_SystemDatabases[i];

                if (string.Equals(curDb.Name, dbName, StringComparison.OrdinalIgnoreCase))
                {
                    _SystemDatabases.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SystemDatabaseEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
