using Drummersoft.DrummerDB.Core.Storage.Abstract;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Storage
{
    class UserDbFileHandlerEnumerator : IEnumerator<UserDbFileHandler>
    {
        private UserDbFileHandlerCollection _databases;
        private int _index;
        private UserDbFileHandler _current;

        public UserDbFileHandler Current => _current;

        object IEnumerator.Current => Current;

        public UserDbFileHandlerEnumerator(UserDbFileHandlerCollection collection)
        {
            _databases = collection;
            _index = -1;
            _current = default(UserDbFileHandler);
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
