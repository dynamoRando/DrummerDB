using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using Drummersoft.DrummerDB.Core.Databases.Remote.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Grpc.Net.Client;
using System;
using structParticipant = Drummersoft.DrummerDB.Core.Structures.Participant;
using structHost = Drummersoft.DrummerDB.Core.Structures.HostInfo;
using structContract = Drummersoft.DrummerDB.Core.Structures.Contract;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class RemoteDataManager : IRemoteDataManager
    {

        /*
         * This class should be an abstraction over DrummerDB.Common.Communication and should be a client to calling another database process (DatabaseService)
         * in getting remote data. It will need to identify itself (so we need to pass the identity to the call)
         */
        #region Private Fields
        private ParticipantSinkCollection _participantSinkCollection;
        private HostSinkCollection _hostSinkCollection;

        // used to identify/authorize ourselves to our participants
        private HostInfo _hostInfo;
        #endregion

        #region Public Properties
        public HostInfo HostInfo => _hostInfo;
        #endregion

        #region Constructors
        public RemoteDataManager(HostInfo hostInfo)
        {
            _participantSinkCollection = new ParticipantSinkCollection();
            _hostSinkCollection = new HostSinkCollection();
            _hostInfo = hostInfo;
        }
        #endregion

        #region Public Methods
        public bool NotifyAcceptContract(structContract contract, out string errorMessage)
        {
            errorMessage = string.Empty;
            ParticipantAcceptsContractResult? result = null;
            HostSink sink;
            sink = GetOrAddHostSink(contract.Host);

            if (!sink.IsOnline())
            {
                errorMessage = $"Host {contract.Host.HostName} is not online";
                return false;
            }

            var request = new ParticipantAcceptsContractRequest();
            request.ContractGUID = contract.ContractGUID.ToString();

            try
            {
                result = sink.Client.AcceptContract(request);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (result is null)
            {
                return false;
            }
            else
            {
                errorMessage = string.Empty;
                return result.ContractAcceptanceIsAcknowledged;
            }
        }

        public void UpdateHostInfo(Guid hostId, string hostName, byte[] token)
        {
            _hostInfo.HostGUID = hostId;
            _hostInfo.HostName = hostName;
            _hostInfo.Token = token;
        }

        public void UpdateHostInfo(HostInfo hostInfo)
        {
            _hostInfo = hostInfo;
        }

        public bool SaveContractAtParticipant(structParticipant participant, structContract contract, out string errorMessage)
        {
            errorMessage = string.Empty;

            SaveContractResult? result = null;
            ParticipantSink sink;
            sink = GetOrAddParticipantSink(participant);

            if (!sink.IsOnline())
            {
                errorMessage = $"Participant {participant.Alias} is not online";
                return false;
            }

            var request = new SaveContractRequest();
            request.Contract = ContractConverter.ConvertContractForCommunication(contract, _hostInfo);

            try
            {
                result = sink.Client.SaveContract(request);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (result is null)
            {
                return false;
            }
            else
            {
                return result.IsSaved;
            }
        }

        // should probably include username/pw or token as a method of auth'd the request
        public IRow GetRowFromParticipant(structParticipant participant, SQLAddress address)
        {
            throw new NotImplementedException();

            string url = $"https://{participant.IP4Address}:{participant.PortNumber.ToString()}";

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
        private AuthRequest GenerateAuthRequest()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Tries to get the sink for the specified participant from the current collection. If it does
        /// not exist, it will create it and then add it to the collection and return it.
        /// </summary>
        /// <param name="participant">The participant to find the sink for</param>
        /// <returns>A sink from the sink collection</returns>
        private ParticipantSink GetOrAddParticipantSink(structParticipant participant)
        {
            ParticipantSink sink;

            if (_participantSinkCollection.Contains(participant))
            {
                sink = _participantSinkCollection.GetSink(participant);
            }
            else
            {
                sink = new ParticipantSink();
                sink.Participant = participant;

                string url = string.Empty;

                if (participant.UseHttps)
                {
                    url = $"https://{participant.IP4Address}:{participant.PortNumber.ToString()}";
                }
                else
                {
                    url = $"http://{participant.IP4Address}:{participant.PortNumber.ToString()}";
                }

                sink.Channel = GrpcChannel.ForAddress(url);
                sink.Client = new DatabaseService.DatabaseServiceClient(sink.Channel);

                _participantSinkCollection.Add(sink);
            }

            return sink;
        }

        private HostSink GetOrAddHostSink(structHost host)
        {
            HostSink sink;

            if (_hostSinkCollection.Contains(host))
            {
                sink = _hostSinkCollection.GetSink(host);
            }
            else
            {
                sink = new HostSink();
                sink.Host = host;

                string url = string.Empty;

                if (host.UseHttps)
                {
                    url = $"https://{host.IP4Address}:{host.DatabasePortNumber.ToString()}";
                }
                else
                {
                    url = $"http://{host.IP4Address}:{host.DatabasePortNumber.ToString()}";
                }

                sink.Channel = GrpcChannel.ForAddress(url);
                sink.Client = new DatabaseService.DatabaseServiceClient(sink.Channel);

                _hostSinkCollection.Add(sink);
            }

            return sink;
        }
        #endregion

    }
}
