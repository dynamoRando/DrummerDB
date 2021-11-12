using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class TableSchemaCollectionEnumerator : IEnumerator<TableSchema>
    {
        private TableSchemaCollection _tables;
        private int _index;
        private TableSchema _current;

        public TableSchema Current => _current;

        object IEnumerator.Current => Current;

        public TableSchemaCollectionEnumerator(TableSchemaCollection collection)
        {
             _tables = collection;
            _index = -1;
            _current = default(TableSchema);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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
