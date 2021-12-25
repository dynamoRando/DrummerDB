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
using System.Diagnostics;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class HostSink
    {
        public structHost Host { get; set; }
        public GrpcChannel? Channel { get; set; }
        public DatabaseServiceClient? Client { get; set; }

        public bool IsOnline()
        {
            TestReply? reply = null;
            var request = new TestRequest();
            string echo = "Test";
            request.RequestEchoMessage = echo;
            try
            {
                reply = Client.IsOnline(request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw new Exception($"Error connecting to Host {Host.HostName} at {Host.IP4Address}:{Host.DatabasePortNumber}. {ex.ToString()}");
            }

            if (reply is not null)
            {
                if (string.Equals(reply.ReplyEchoMessage, echo))
                {
                    return true;
                }
            }
            

            return false;
        }
    }
}
