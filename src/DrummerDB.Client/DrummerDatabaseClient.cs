using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Client
{
    public class DrummerDatabaseClient
    {
        #region Private Fields
        private DatabaseService.DatabaseServiceClient _client;
        private GrpcChannel _channel;
        private string _url;
        private const string TEST_MESSAGE = "Test From Client";
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        /// <summary>
        /// Initalizes a client connect for a database process running at the specified url and port number
        /// </summary>
        /// <param name="url">The url of the database process</param>
        /// <param name="portNumber">The port number of the database process</param>
        public DrummerDatabaseClient(string url, int portNumber)
        {
            InitConnection(url, portNumber);
        }

        #endregion

        #region Public Methods
        public void Shutdown()
        {
            _client = null;
            _channel.ShutdownAsync().Wait();
            _channel.Dispose();
            _channel = null;
        }
        public bool IsClientOnline()
        {
            return IsOnline();
        }

        public Task<bool> IsClientOnlineAsync()
        {
            return IsOnlineAsync();
        }

        public async Task<bool> IsLoginValidAsync(string userName, string pw)
        {
            var loginMesage = new AuthRequest();
            loginMesage.UserName = userName;
            loginMesage.Pw = pw;

            var reply = await _client.IsLoginValidAsync(loginMesage);

            return reply.IsAuthenticated;
        }

        public async Task<CreateDatabaseResult> CreateUserDatabase(string userName, string pw, string dbName)
        {
            var loginMesage = new AuthRequest();
            loginMesage.UserName = userName;
            loginMesage.Pw = pw;

            var createDbRequest = new CreateDatabaseRequest { Authentication = loginMesage, DatabaseName = dbName };

            return await _client.CreateUserDatabaseAsync(createDbRequest);
        }
        #endregion

        #region Private Methods
        private void InitConnection(string url, int portNumber)
        {
            string completeUrl = url + ":" + portNumber.ToString();
            _url = completeUrl;

            _channel = GrpcChannel.ForAddress(completeUrl);
            _client = new DatabaseService.DatabaseServiceClient(_channel);
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
