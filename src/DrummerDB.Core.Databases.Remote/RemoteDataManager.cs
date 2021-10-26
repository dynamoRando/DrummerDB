using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using Drummersoft.DrummerDB.Core.Databases.Remote.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Grpc.Net.Client;
using System;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class RemoteDataManager : IRemoteDataManager
    {

        /*
         * This class should be an abstraction over DrummerDB.Common.Communication and should be a client to calling another database process (DatabaseService)
         * in getting remote data. It will need to identify itself (so we need to pass the identity to the call)
         */
        #region Private Fields
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        // should probably include username/pw or token as a method of auth'd the request
        public IRow GetRowFromParticipant(IParticipant participant, SQLAddress address)
        {
            throw new NotImplementedException();

            string url = $"https://{participant.Url}:{participant.PortNumber.ToString()}";

            var channel = GrpcChannel.ForAddress(url);
            var client = new DatabaseService.DatabaseServiceClient(channel);

            var request = new GetRowFromPartialDatabaseRequest();
            request.RowAddress.DatabaseId = address.DatabaseId.ToString();
            request.RowAddress.TableId = (uint)address.TableId;
            request.RowAddress.RowId = (uint)address.RowId;

            const string testMessage = "RemoteTest";

            var testRequest = new TestRequest();
            testRequest.RequestEchoMessage = testMessage;

            var testResult = client.IsOnline(testRequest);

            if (testResult is not null)
            {
                if (string.Equals(testResult.ReplyEchoMessage, testMessage, StringComparison.InvariantCultureIgnoreCase))
                {
                    var result = client.GetRowFromPartialDatabase(request);

                    if (result is not null)
                    {
                        if (result.AuthenticationResult.IsAuthenticated)
                        {
                            if (result.IsSuccessful)
                            {
                                var communicationRow = result.Row;
                                // convert the result to a row, using the values
                            }
                        }
                    }
                }
            }

            channel.ShutdownAsync();

        }
        #endregion

        #region Private Methods
        #endregion

    }
}
