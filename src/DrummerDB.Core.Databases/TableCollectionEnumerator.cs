using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class TableCollectionEnumerator : IEnumerator<Table>
    {
        private TableCollection _tables;
        private int _index;
        private Table _current;

        public Table Current => _current;

        object IEnumerator.Current => Current;

        public TableCollectionEnumerator(TableCollection collection)
        {
            _tables = collection;
            _index = -1;
            _current = default(Table);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _tables.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _tables[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
