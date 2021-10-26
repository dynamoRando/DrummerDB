using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.InfoService;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Client
{
    public class DrummerInfoClient
    {
        #region Private Fields
        private InfoService.InfoServiceClient _client;
        private GrpcChannel _channel;
        private string _url;
        private const string TEST_MESSAGE = "Test From Client";
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        /// <summary>
        /// Initalizes a client connect for a database proces running at the specified url and port number
        /// </summary>
        /// <param name="url">The url of the database process</param>
        /// <param name="portNumber">The port number of the database process</param>
        public DrummerInfoClient(string url, int portNumber)
        {
            InitConnection(url, portNumber);
        }

        #endregion

        #region Public Methods
        public bool IsClientOnline()
        {
            return IsOnline();
        }

        public Task<bool> IsClientOnlineAsync()
        {
            return IsOnlineAsync();
        }

        #endregion

        #region Private Methods
        private void InitConnection(string url, int portNumber)
        {
            string completeUrl = url + ":" + portNumber.ToString();
            _url = completeUrl;

            _channel = GrpcChannel.ForAddress(completeUrl);
            _client = new InfoService.InfoServiceClient(_channel);
        }

        private async Task<TestReply> SendPingAsync()
        {
            var test = new TestRequest();
            test.RequestEchoMessage = TEST_MESSAGE;
            test.RequestTimeUTC = DateTime.UtcNow.ToString();

            var reply = await _client.IsOnlineAsync(test);

            return reply;
        }

        private TestReply SendPing()
        {
            var test = new TestRequest();
            test.RequestEchoMessage = TEST_MESSAGE;
            test.RequestTimeUTC = DateTime.UtcNow.ToString();

            var reply = _client.IsOnline(test);

            return reply;
        }

        private bool IsOnline()
        {
            TestReply result = null;
            try
            {
                result = SendPing();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (string.Equals(result.ReplyEchoMessage, TEST_MESSAGE, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> IsOnlineAsync()
        {
            TestReply result = null;
            try
            {
                result = await SendPingAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }


            if (string.Equals(result.ReplyEchoMessage, TEST_MESSAGE, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
