using Drummersoft.DrummerDB.Core.Structures;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class TransactionRequestEnumerator : IEnumerator<TransactionRequest>
    {
        private TransactionRequestCollection _transactions;
        private int _index;
        private TransactionRequest _current;

        public TransactionRequest Current => _current;
        object IEnumerator.Current => Current;

        public TransactionRequestEnumerator(TransactionRequestCollection collection)
        {
            _transactions = collection;
            _index = -1;
            _current = default(TransactionRequest);
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
