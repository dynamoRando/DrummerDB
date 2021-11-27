
using System;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class ColumnSchemaEnumerator : IEnumerator<ColumnSchema>
    {
        private ColumnSchemaCollection _ColumnSchemas;
        private int _index;
        private ColumnSchema _current;

        public ColumnSchema Current => _current;

        object IEnumerator.Current => Current;

        public ColumnSchemaEnumerator(ColumnSchemaCollection collection)
        {
            _ColumnSchemas = collection;
            _index = -1;
            _current = default(ColumnSchema);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _ColumnSchemas.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _ColumnSchemas[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
