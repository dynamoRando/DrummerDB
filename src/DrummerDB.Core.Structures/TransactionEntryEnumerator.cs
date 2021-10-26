using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Drummersoft.DrummerDB.Core.Structures
{
    class TransactionEntryEnumerator : IEnumerator<TransactionEntry>
    {
        private TransactionEntryCollection _transactions;
        private int _index;
        private TransactionEntry _current;

        public TransactionEntry Current => _current;
        object IEnumerator.Current => Current;

        public TransactionEntryEnumerator(TransactionEntryCollection collection)
        {
            _transactions = collection;
            _index = -1;
            _current = default(TransactionEntry);
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
