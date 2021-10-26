using Drummersoft.DrummerDB.Core.Databases.Abstract;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class UserDatabaseEnumerator : IEnumerator<UserDatabase>
    {
        private UserDatabaseCollection _databases;
        private int _index;
        private UserDatabase _current;

        public UserDatabase Current => _current;

        object IEnumerator.Current => Current;

        public UserDatabaseEnumerator(UserDatabaseCollection collection)
        {
            _databases = collection;
            _index = -1;
            _current = default(UserDatabase);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _databases.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _databases[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
