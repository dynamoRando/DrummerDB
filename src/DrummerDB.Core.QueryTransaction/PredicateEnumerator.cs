using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class PredicateEnumerator : IEnumerator<IPredicate>
    {
        private PredicateCollection _databases;
        private int _index;
        private IPredicate _current;

        public IPredicate Current => _current;
        object IEnumerator.Current => Current;

        public PredicateEnumerator(PredicateCollection collection)
        {
            _databases = collection;
            _index = -1;
            _current = default(IPredicate);
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
