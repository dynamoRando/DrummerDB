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
using Google.Protobuf;
using structRow = Drummersoft.DrummerDB.Core.Structures.Row;
using comRowValue = Drummersoft.DrummerDB.Common.Communication.RowValue;
using comColumnSchema = Drummersoft.DrummerDB.Common.Communication.ColumnSchema;
using comTableSchema = Drummersoft.DrummerDB.Common.Communication.TableSchema;
using System.Net;
using Drummersoft.DrummerDB.Common.Communication.Enum;
using Drummersoft.DrummerDB.Core.Diagnostics;
using System.Text;

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
        private structHost _hostInfo;
        private LogService _logger;
        #endregion

        #region Public Properties
        public HostInfo HostInfo => _hostInfo;
        #endregion

        #region Constructors
        public RemoteDataManager(structHost hostInfo)
        {
            _participantSinkCollection = new ParticipantSinkCollection();
            _hostSinkCollection = new HostSinkCollection();
            _hostInfo = hostInfo;
        }

        public RemoteDataManager(structHost hostInfo, LogService logger) : this(hostInfo)
        {
            _logger = logger;
        }
        #endregion

        #region Public Methods
        public bool SaveRowAtParticipant(
            structRow row,
            string dbName,
            Guid dbId,
            string tableName,
            int tableId,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            structParticipant participant = row.Participant;

            ParticipantSink sink;
            sink = GetOrAddParticipantSink(participant);

            if (!sink.IsOnline())
            {
                errorMessage = $"Participant {participant.Alias} is not online";
                return false;
            }

            var request = new InsertRowRequest();
            InsertRowResult? result = null;

            // need authentication information to send
            request.Authentication = GetAuthRequest();
            request.MessageInfo = GetMessageInfo(MessageType.InsertRowRequest);

            var comTableSchema = new comTableSchema();
            comTableSchema.DatabaseId = dbId.ToString();
            comTableSchema.DatabaseName = dbName;
            comTableSchema.TableId = Convert.ToUInt32(tableId);
            comTableSchema.TableName = tableName;

            request.Table = comTableSchema;

            // need to build row values
            foreach (var sRV in row.Values)
            {
                var cValue = new comRowValue();

                // build out column for the value
                var cColumn = new comColumnSchema();
                cColumn.ColumnName = sRV.Column.Name;
                cColumn.ColumnId = Convert.ToUInt32(sRV.Column.Id);
                cColumn.ColumnType = Convert.ToUInt32(
                    SQLColumnTypeConverter.Convert(sRV.Column.DataType, Constants.DatabaseVersions.V100));
                cColumn.ColumnLength = Convert.ToUInt32(sRV.Column.Length);
                cColumn.IsNullable = sRV.Column.IsNullable;

                // build out value
                cValue.IsNullValue = sRV.IsNull();
                if (!sRV.IsNull())
                {
                    cValue.Value = ByteString.CopyFrom(sRV.GetValueInBinary());
                }

                cValue.Column = cColumn;
                request.Values.Add(cValue);
            }

            try
            {
                LogMessageInfo(request.MessageInfo);
                result = sink.Client.InsertRowIntoTable(request);
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
                return result.IsSuccessful;
            }
        }

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
            request.DatabaseName = contract.DatabaseName;

            var comParticipant = new Common.Communication.Participant();
            comParticipant.Alias = _hostInfo.HostName;
            comParticipant.Ip4Address = _hostInfo.IP4Address;
            comParticipant.Ip6Address = _hostInfo.IP6Address;
            comParticipant.DatabasePortNumber = Convert.ToUInt32(_hostInfo.DatabasePortNumber);
            comParticipant.Token = ByteString.CopyFrom(_hostInfo.Token);
            comParticipant.ParticipantGUID = _hostInfo.HostGUID.ToString();

            request.Participant = comParticipant;

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

        private AuthRequest GetAuthRequest()
        {
            var request = new AuthRequest();
            request.UserName = _hostInfo.HostName;
            request.Token = ByteString.CopyFrom(_hostInfo.Token);

            return request;
        }

        private MessageInfo GetMessageInfo(MessageType type)
        {
            var info = new MessageInfo();
            info.IsLittleEndian = BitConverter.IsLittleEndian;

            var addresses = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (var address in addresses)
            {
                info.MessageAddresses.Add(address.ToString());
            }

            info.MessageGeneratedTimeUTC = DateTime.UtcNow.ToString();
            info.MessageType = Convert.ToUInt32(type);
            info.MessageGUID = Guid.NewGuid().ToString();

            return info;
        }

        private void LogMessageInfo(MessageInfo info)
        {
            var type = (MessageType)info.MessageType;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"DrummerDB.Core.Databases.Remote - Remote Data Manager: Sending Message {type} ");
            stringBuilder.Append($"Message Id: {info.MessageGUID}");
            stringBuilder.Append($"Message UTC Generated: {info.MessageGeneratedTimeUTC}");
            stringBuilder.Append($"IsLittleEndian: {info.IsLittleEndian}");
            foreach (var address in info.MessageAddresses)
            {
                stringBuilder.Append($"Message address: {address}");
            }

            _logger.Info(stringBuilder.ToString());
        }
        #endregion

    }
}
