using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class SystemDatabaseEnumerator : IEnumerator<SystemDatabase>
    {
        private SystemDatabaseCollection _databases;
        private int _index;
        private SystemDatabase _current;

        public SystemDatabase Current => _current;
        object IEnumerator.Current => Current;

        public SystemDatabaseEnumerator(SystemDatabaseCollection collection)
        {
            _databases = collection;
            _index = -1;
            _current = default(SystemDatabase);
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
