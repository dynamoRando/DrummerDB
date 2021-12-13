using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using structHost = Drummersoft.DrummerDB.Core.Structures.HostInfo;
using Grpc.Net.Client;
using static Drummersoft.DrummerDB.Common.Communication.DatabaseService.DatabaseService;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class HostSink
    {
        public structHost Host { get; set; }
        public GrpcChannel? Channel { get; set; }
        public DatabaseServiceClient? Client { get; set; }

        public bool IsOnline()
        {
            var request = new TestRequest();
            string echo = "Test";
            request.RequestEchoMessage = echo;
            var reply = Client.IsOnline(request);

            if (string.Equals(reply.ReplyEchoMessage, echo))
            {
                return true;
            }

            return false;
        }
    }
}
