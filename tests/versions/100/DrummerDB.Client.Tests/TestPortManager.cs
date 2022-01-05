using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Drummersoft.DrummerDB.Client.Tests
{
    internal static class TestPortManager
    {
        private const int _startingPort = 10_000;
        private static int _maxPort;
        private static readonly object lockPortNumber = new object();

        public static int GetNextAvailablePortNumber()
        {
            lock (lockPortNumber)
            {
                if (_maxPort == 0)
                {
                    _maxPort = _startingPort;
                    return _maxPort;
                }
                else
                {
                    _maxPort++;
                    return _maxPort;
                }
            }
        }
    }
}
