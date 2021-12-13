using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using structParticipant = Drummersoft.DrummerDB.Core.Structures.Participant;
using Grpc.Net.Client;
using static Drummersoft.DrummerDB.Common.Communication.DatabaseService.DatabaseService;
using System.Diagnostics;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    /// <summary>
    /// Represents the gRPC connection objects for a Participant
    /// </summary>
    /// <remarks>We hold onto the gRPC connection objects so that we don't exhaust TCP connections</remarks>
    internal class ParticipantSink
    {
        public structParticipant Participant { get; set; }
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
