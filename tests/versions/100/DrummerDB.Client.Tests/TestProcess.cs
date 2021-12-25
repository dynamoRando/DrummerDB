using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using drummer = Drummersoft.DrummerDB.Core.Systems;

namespace Drummersoft.DrummerDB.Client.Tests
{
    internal class TestProcess
    {
        public int ProcessId;
        public drummer.Process Process;
        public string DatabaseFolder;
        public int SQLPort;
        public int DatabasePort;
        public DrummerSQLClient SQLClient;
        public string UserName;
        public string Password;
        public Guid UserSessionId;
        public string Alias;

        public TestProcessInfo GetTestProcessInfo()
        {
            return new TestProcessInfo(ProcessId, SQLPort, DatabasePort, Alias);
        }
    }
}
