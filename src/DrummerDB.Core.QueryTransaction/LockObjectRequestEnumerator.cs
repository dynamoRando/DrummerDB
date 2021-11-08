using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class LockObjectRequestEnumerator : IEnumerator<LockObjectRequest>
    {
        private LockObjectRequestCollection _databases;
        private int _index;
        private LockObjectRequest _current;

        public LockObjectRequest Current => _current;

        object IEnumerator.Current => _current;

        public LockObjectRequestEnumerator(LockObjectRequestCollection collection)
        {
            _databases = collection;
            _index = -1;
            _current = default(LockObjectRequest);
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
