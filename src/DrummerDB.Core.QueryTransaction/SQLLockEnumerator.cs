using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class SQLLockEnumerator : IEnumerator<SQLLock>
    {
        private SQLLockCollection _databases;
        private int _index;
        private SQLLock _current;

        public SQLLock Current => _current;

        object IEnumerator.Current => Current;

        public SQLLockEnumerator(SQLLockCollection collection)
        {
            _databases = collection;
            _index = -1;
            _current = default(SQLLock);
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
