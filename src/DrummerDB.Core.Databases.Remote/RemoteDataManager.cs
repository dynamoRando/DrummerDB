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
using structRowValue = Drummersoft.DrummerDB.Core.Structures.RowValue;
using structColumnSchema = Drummersoft.DrummerDB.Core.Structures.ColumnSchema;
using comRowValue = Drummersoft.DrummerDB.Common.Communication.RowValue;
using comColumnSchema = Drummersoft.DrummerDB.Common.Communication.ColumnSchema;
using comTableSchema = Drummersoft.DrummerDB.Common.Communication.TableSchema;
using System.Net;
using Drummersoft.DrummerDB.Common.Communication.Enum;
using Drummersoft.DrummerDB.Core.Diagnostics;
using System.Text;
using static Drummersoft.DrummerDB.Common.Communication.DatabaseService.DatabaseService;
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;

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
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
        }
        #endregion

        #region Public Methods
        public bool RemoveRowAtParticipant(structRow row,
            string dbName,
            Guid dbId,
            string tableName,
            int tableId,
            out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool SaveRowAtParticipant(
            structRow row,
            string dbName,
            Guid dbId,
            string tableName,
            int tableId,
            TransactionRequest transaction,
            TransactionMode transactionMode,
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
            request.MessageInfo = GetMessageInfo(MessageType.InsertRow);
            request.Transaction = GetTransactionInfo(transaction, transactionMode);
            request.RowId = Convert.ToUInt32(row.Id);

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
                LogMessageInfo(request.MessageInfo, sink);
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
            request.MessageInfo = GetMessageInfo(MessageType.AcceptContract);

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
                LogMessageInfo(request.MessageInfo, sink);
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
            request.MessageInfo = GetMessageInfo(MessageType.SaveContract);

            try
            {
                LogMessageInfo(request.MessageInfo, sink);
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

        public bool UpdateRemoteRow(
            structParticipant participant,
            string tableName,
            int tableId,
            string databaseName,
            Guid dbId,
            int rowId,
            RemoteValueUpdate updateValue,
            TransactionRequest transaction,
            TransactionMode transactionMode,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            ParticipantSink sink;
            sink = GetOrAddParticipantSink(participant);
            var result = new UpdateRowInTableResult();

            var request = new UpdateRowInTableRequest();
            request.Authentication = GetAuthRequest();
            request.MessageInfo = GetMessageInfo(MessageType.UpdateRow);
            request.DatabaseName = databaseName;
            request.DatabaseId = dbId.ToString();
            request.TableId = Convert.ToUInt32(tableId);
            request.TableName = tableName;
            request.WhereRowId = Convert.ToUInt32(rowId);
            request.UpdateColumn = updateValue.ColumnName;
            request.UpdateValue = updateValue.Value;

            try
            {
                LogMessageInfo(request.MessageInfo, sink);
                result = sink.Client.UpdateRowInTable(request);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            if (result is not null)
            {
                return result.IsSuccessful;
            }

            return false;
        }

        // should probably include username/pw or token as a method of auth'd the request
        public IRow GetRowFromParticipant(structParticipant participant, SQLAddress address, string databaseName, string tableName, out string errorMessage)
        {
            errorMessage = string.Empty;
            ParticipantSink sink;
            sink = GetOrAddParticipantSink(participant);
            GetRowFromPartialDatabaseResult? result = null;
            IRow rowResult = null;

            if (!sink.IsOnline())
            {
                throw new InvalidOperationException("Participant is offline");
            }

            var request = new GetRowFromPartialDatabaseRequest();
            request.RowAddress = new RowParticipantAddress();
            request.RowAddress.DatabaseId = address.DatabaseId.ToString();
            request.RowAddress.TableId = (uint)address.TableId;
            request.RowAddress.RowId = (uint)address.RowId;
            request.RowAddress.DatabaseName = databaseName;
            request.RowAddress.TableName = tableName;
            request.Authentication = GetAuthRequest();
            request.MessageInfo = GetMessageInfo(MessageType.GetRow);

            try
            {
                LogMessageInfo(request.MessageInfo, sink);
                result = sink.Client.GetRowFromPartialDatabase(request);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return null;
            }

            if (result is not null)
            {
                // do something with the result
                rowResult = ConvertRequestToRow(result, participant.Id);
                return rowResult;
            }

            errorMessage = "Unable to get row from participant";
            return null;

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

            bool isTokenNull = false;
            var bIsTokenNull = DbBinaryConvert.BooleanToBinary(isTokenNull);

            // we have to format this with the leading isNull value and the size prefix
            // note: we have a mistake here, we probably need to do a redesign
            // on the other side in cache/storage, we save off the leading isNull byte (which is always false)
            // with the data itself, so we need to match that on this side

            var tokenOriginalValue = _hostInfo.Token;

            var newTokenValue = new byte[tokenOriginalValue.Length + Constants.SIZE_OF_BOOL];
            Array.Copy(bIsTokenNull, 0, newTokenValue, 0, bIsTokenNull.Length);
            Array.Copy(tokenOriginalValue, 0, newTokenValue, bIsTokenNull.Length, tokenOriginalValue.Length);

            int tokenLength = newTokenValue.Length;

            var bTokenLength = DbBinaryConvert.IntToBinary(tokenLength);

            byte[] messageToken;

            messageToken = new byte[bIsTokenNull.Length + bTokenLength.Length + newTokenValue.Length];
            Array.Copy(bIsTokenNull, 0, messageToken, 0, bIsTokenNull.Length);
            Array.Copy(bTokenLength, 0, messageToken, bIsTokenNull.Length, bTokenLength.Length);
            Array.Copy(newTokenValue, 0, messageToken, bIsTokenNull.Length + bTokenLength.Length, newTokenValue.Length);

            request.Token = ByteString.CopyFrom(messageToken);

            return request;
        }

        private TransactionInfo GetTransactionInfo(TransactionRequest request, TransactionMode mode)
        {
            // we are currently sending this information to the participant as informational only
            // we are not as of yet logging this information at the participant nor obeying the transaction mode
            // TBD if we will actually log/act on the transaction information

            var xact = new TransactionInfo();
            xact.TransactionBatchId = request.TransactionBatchId.ToString();
            xact.TransactionMode = Convert.ToUInt32(mode);
            return xact;
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

        private void LogMessageInfo(MessageInfo info, ParticipantSink sink)
        {
            var type = (MessageType)info.MessageType;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"Remote Data Manager: Sending Message {type} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"Message Id: {info.MessageGUID} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"Message UTC Generated: {info.MessageGeneratedTimeUTC} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"IsLittleEndian: {info.IsLittleEndian} ");
            stringBuilder.Append(Environment.NewLine);
            foreach (var address in info.MessageAddresses)
            {
                stringBuilder.Append($"Message address: {address} ");
                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.Append($"Message Sent from Host: {_hostInfo} ");
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.Append($"Destination ParticipantSink: {sink.Participant}");

            _logger.Info(stringBuilder.ToString());
        }

        private void LogMessageInfo(MessageInfo info, HostSink sink)
        {
            var type = (MessageType)info.MessageType;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"Remote Data Manager: Sending Message {type} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"Message Id: {info.MessageGUID} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"Message UTC Generated: {info.MessageGeneratedTimeUTC} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"IsLittleEndian: {info.IsLittleEndian} ");
            stringBuilder.Append(Environment.NewLine);
            foreach (var address in info.MessageAddresses)
            {
                stringBuilder.Append($"Message address: {address} ");
                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.Append($"Message Sent from Participant: {_hostInfo} ");
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.Append($"Destination HostSink: {sink.Host} ");

            _logger.Info(stringBuilder.ToString());
        }

        private IRow ConvertRequestToRow(GetRowFromPartialDatabaseResult request, Guid? participantId)
        {
            var row = new structRow(Convert.ToInt32(request.Row.RowId), false, participantId);
            var values = new List<structRowValue>(request.Row.Values.Count);

            foreach (var comValue in request.Row.Values)
            {
                var comColumn = comValue.Column;
                int enumType = Convert.ToInt32(comColumn.ColumnType);
                var type = SQLColumnTypeConverter.Convert((SQLColumnType)enumType, Convert.ToInt32(comColumn.ColumnLength));
                var col = new structColumnSchema(comColumn.ColumnName, type, Convert.ToInt32(comColumn.Ordinal));

                var structValue = new structRowValue(col, comValue.Value.ToByteArray());
                values.Add(structValue);
            }

            row.Values = values.ToArray();

            return row;
        }
        #endregion

    }
}
