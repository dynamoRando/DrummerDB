using System;
using System.Collections;
using System.Collections.Generic;
using structParticipant = Drummersoft.DrummerDB.Core.Structures.Participant;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class ParticipantSinkCollection : IEnumerable<ParticipantSink>
    {
        #region Private Fields
        private List<ParticipantSink> _ParticipantSinks;
        #endregion

        #region Public Properties
        public int Count => _ParticipantSinks.Count;
        public bool IsReadOnly => false;
        public List<ParticipantSink> List => _ParticipantSinks;
        #endregion

        #region Constructors
        public ParticipantSinkCollection()
        {
            _ParticipantSinks = new List<ParticipantSink>();
        }

        public ParticipantSinkCollection(int size)
        {
            _ParticipantSinks = new List<ParticipantSink>(size);
        }
        #endregion

        #region Public Methods
        public ParticipantSink GetSink(string alias)
        {
            foreach (var ParticipantSink in _ParticipantSinks)
            {
                if (string.Equals(ParticipantSink.Participant.Alias, alias, StringComparison.OrdinalIgnoreCase))
                {
                    return ParticipantSink;
                }
            }

            return null;
        }

        public ParticipantSink GetSink(structParticipant participant)
        {
            foreach (var ParticipantSink in _ParticipantSinks)
            {
                if (string.Equals(ParticipantSink.Participant.Alias, participant.Alias, StringComparison.OrdinalIgnoreCase))
                {
                    return ParticipantSink;
                }
            }

            return null;
        }

        public void Add(ParticipantSink item)
        {
            if (!Contains(item.Participant.Alias))
            {
                _ParticipantSinks.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a ParticipantSink with alias {item.Participant.Alias}");
            }
        }

        public bool Contains(structParticipant participant)
        {
            foreach (var p in _ParticipantSinks)
            {
                if (string.Equals(participant.Alias, p.Participant.Alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(ParticipantSink ParticipantSink)
        {
            foreach (var x in _ParticipantSinks)
            {
                if (string.Equals(x.Participant.Alias, ParticipantSink.Participant.Alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string alias)
        {
            foreach (var ParticipantSink in _ParticipantSinks)
            {
                if (string.Equals(ParticipantSink.Participant.Alias, alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(ParticipantSink item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _ParticipantSinks.Count; i++)
            {
                ParticipantSink currentParticipantSink = _ParticipantSinks[i];

                if (string.Equals(currentParticipantSink.Participant.Alias, item.Participant.Alias, StringComparison.OrdinalIgnoreCase))
                {
                    _ParticipantSinks.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(string alias)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _ParticipantSinks.Count; i++)
            {
                ParticipantSink currentParticipantSink = _ParticipantSinks[i];

                if (string.Equals(currentParticipantSink.Participant.Alias, alias, StringComparison.OrdinalIgnoreCase))
                {
                    _ParticipantSinks.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public ParticipantSink this[int index]
        {
            get { return _ParticipantSinks[index]; }
            set { _ParticipantSinks[index] = value; }
        }

        public IEnumerator<ParticipantSink> GetEnumerator()
        {
            return new ParticipantSinkEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ParticipantSinkEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
