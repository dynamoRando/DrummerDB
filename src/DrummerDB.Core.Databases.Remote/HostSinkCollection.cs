using System;
using System.Collections;
using System.Collections.Generic;
using structHost = Drummersoft.DrummerDB.Core.Structures.HostInfo;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class HostSinkCollection : IEnumerable<HostSink>
    {
        #region Private Fields
        private List<HostSink> _HostSinks;
        #endregion

        #region Public Properties
        public int Count => _HostSinks.Count;
        public bool IsReadOnly => false;
        public List<HostSink> List => _HostSinks;
        #endregion

        #region Constructors
        public HostSinkCollection()
        {
            _HostSinks = new List<HostSink>();
        }

        public HostSinkCollection(int size)
        {
            _HostSinks = new List<HostSink>(size);
        }
        #endregion

        #region Public Methods
        public HostSink GetSink(string alias)
        {
            foreach (var HostSink in _HostSinks)
            {
                if (string.Equals(HostSink.Host.HostName, alias, StringComparison.OrdinalIgnoreCase))
                {
                    return HostSink;
                }
            }

            return null;
        }

        public HostSink GetSink(structHost host)
        {
            foreach (var HostSink in _HostSinks)
            {
                if (string.Equals(HostSink.Host.HostName, host.HostName, StringComparison.OrdinalIgnoreCase))
                {
                    return HostSink;
                }
            }

            return null;
        }

        public void Add(HostSink item)
        {
            if (!Contains(item.Host.HostName))
            {
                _HostSinks.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a HostSink with alias {item.Host.HostName}");
            }
        }

        public bool Contains(structHost host)
        {
            foreach (var p in _HostSinks)
            {
                if (string.Equals(host.HostName, p.Host.HostName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(HostSink HostSink)
        {
            foreach (var x in _HostSinks)
            {
                if (string.Equals(x.Host.HostName, HostSink.Host.HostName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string alias)
        {
            foreach (var HostSink in _HostSinks)
            {
                if (string.Equals(HostSink.Host.HostName, alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(HostSink item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _HostSinks.Count; i++)
            {
                HostSink currentHostSink = _HostSinks[i];

                if (string.Equals(currentHostSink.Host.HostName, item.Host.HostName, StringComparison.OrdinalIgnoreCase))
                {
                    _HostSinks.RemoveAt(i);
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
            for (int i = 0; i < _HostSinks.Count; i++)
            {
                HostSink currentHostSink = _HostSinks[i];

                if (string.Equals(currentHostSink.Host.HostName, alias, StringComparison.OrdinalIgnoreCase))
                {
                    _HostSinks.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public HostSink this[int index]
        {
            get { return _HostSinks[index]; }
            set { _HostSinks[index] = value; }
        }

        public IEnumerator<HostSink> GetEnumerator()
        {
            return new HostSinkEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new HostSinkEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
