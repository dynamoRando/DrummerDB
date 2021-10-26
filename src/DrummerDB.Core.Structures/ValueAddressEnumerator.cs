using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class ValueAddressEnumerator : IEnumerator<ValueAddress>
    {
        private ValueAddressCollection _transactions;
        private int _index;
        private ValueAddress _current;

        public ValueAddress Current => _current;
        object IEnumerator.Current => Current;

        public ValueAddressEnumerator(ValueAddressCollection collection)
        {
            _transactions = collection;
            _index = -1;
            _current = default(ValueAddress);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _transactions.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _transactions[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
