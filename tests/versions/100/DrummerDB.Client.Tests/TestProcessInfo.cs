using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Client.Tests
{
    internal record struct TestProcessInfo(int ProcessId, int SQLPort, int DbPort, string Alias)
    {
    }
}
