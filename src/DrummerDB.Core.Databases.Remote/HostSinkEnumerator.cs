
using System;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class HostSinkEnumerator : IEnumerator<HostSink>
    {
        private HostSinkCollection _HostSinks;
        private int _index;
        private HostSink _current;

        public HostSink Current => _current;

        object IEnumerator.Current => Current;

        public HostSinkEnumerator(HostSinkCollection collection)
        {
            _HostSinks = collection;
            _index = -1;
            _current = default(HostSink);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _HostSinks.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _HostSinks[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
