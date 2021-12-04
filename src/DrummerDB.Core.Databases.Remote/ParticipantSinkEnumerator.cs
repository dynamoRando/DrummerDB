
using System;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class ParticipantSinkEnumerator : IEnumerator<ParticipantSink>
    {
        private ParticipantSinkCollection _ParticipantSinks;
        private int _index;
        private ParticipantSink _current;

        public ParticipantSink Current => _current;

        object IEnumerator.Current => Current;

        public ParticipantSinkEnumerator(ParticipantSinkCollection collection)
        {
            _ParticipantSinks = collection;
            _index = -1;
            _current = default(ParticipantSink);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _ParticipantSinks.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _ParticipantSinks[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
