using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Common.Communication.DatabaseService.DatabaseService;
using drumContract = Drummersoft.DrummerDB.Core.Structures.Contract;
using drumTableSchema = Drummersoft.DrummerDB.Core.Structures.TableSchema;
using drumColumn = Drummersoft.DrummerDB.Core.Structures.ColumnSchema;
using drumParticipant = Drummersoft.DrummerDB.Core.Structures.Participant;
using System.Collections.Generic;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Common.Communication.Enum;

namespace Drummersoft.DrummerDB.Core.Communication
{
    internal class DrummerDatabaseService : DatabaseServiceBase
    {
        #region Private Fields
        private readonly DatabaseServiceHandler _handler;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public DrummerDatabaseService(ILogger<DrummerDatabaseService> logger, DatabaseServiceHandler handler)
        {
            _handler = handler;
        }
        #endregion

        #region Public Methods
        public override Task<TestReply> IsOnline(TestRequest request, ServerCallContext context)
        {
            _handler.HandleTest();
            var reply = new TestReply();
            reply.ReplyTimeUTC = DateTime.UtcNow.ToString();
            reply.ReplyEchoMessage = request.RequestEchoMessage;
            return Task.FromResult(reply);
        }

        public override Task<AuthResult> IsLoginValid(AuthRequest request, ServerCallContext context)
        {
            bool hasLogin;
            AuthResult result = null;

            if (request.Pw is null || request.Pw == String.Empty)
            {
                hasLogin = _handler.SystemHasHost(request.UserName, request.Token.ToByteArray());
            }
            else
            {
                hasLogin = _handler.SystemHasLogin(request.UserName, request.Pw);
            }

            if (hasLogin)
            {
                result = new AuthResult();
                result.IsAuthenticated = true;
                result.AuthenticationMessage = "Login exists at system level";
                result.UserName = request.UserName;
            }
            else
            {
                result = new AuthResult();
                result.IsAuthenticated = false;
                result.AuthenticationMessage = "Login failure";
                result.UserName = request.UserName;
            }

            return Task.FromResult(result);
        }

        public override Task<CreateDatabaseResult> CreateUserDatabase(CreateDatabaseRequest request, ServerCallContext context)
        {
            var hasLogin = IsLoginValid(request.Authentication, context);
            var result = new CreateDatabaseResult();

            if (hasLogin.Result.IsAuthenticated)
            {
                result.AuthenticationResult = hasLogin.Result;

                if (_handler.UserHasSystemPermission(request.Authentication.UserName, SystemPermission.CreateHostDatabase))
                {
                    Guid databaseId;
                    result.IsSuccessful = _handler.CreateUserDatabase(request.DatabaseName, out databaseId);

                    result.DatabaseName = request.DatabaseName;
                    result.DatabaseId = databaseId.ToString();
                }
                else
                {
                    result.IsSuccessful = false;
                    result.ResultMessage = $"{request.Authentication.UserName} does not have permission to create databases.";
                }
            }
            else
            {
                result.AuthenticationResult = hasLogin.Result;
            }

            return Task.FromResult(result);
        }

        public override Task<InsertRowResult> InsertRowIntoTable(InsertRowRequest request, ServerCallContext context)
        {
            bool isSuccessfulInsert = false;
            var result = new InsertRowResult();

            if (request.MessageInfo is not null)
            {
                LogMessageInfo(request.MessageInfo);
            }

            var hasLogin = IsLoginValid(request.Authentication, context);
            if (hasLogin.Result.IsAuthenticated)
            {
                isSuccessfulInsert = _handler.InsertRowIntoTable(request);
            }
            else
            {
                throw new InvalidOperationException("The requestor has not been authenticated");
            }

            result.IsSuccessful = isSuccessfulInsert;

            if (!isSuccessfulInsert)
            {
                throw new NotImplementedException("Need to fill out failure details");
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Record a copy of contract sent from a host
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<SaveContractResult> SaveContract(SaveContractRequest request, ServerCallContext context)
        {
            // we need to check authentication here

            string errorMessage = string.Empty;
            var databaseContract = ConvertContractRequestToContract(request);

            var result = _handler.SaveContract(databaseContract, out errorMessage);

            var reply = new SaveContractResult();
            reply.IsSaved = result;
            reply.ErrorMessage = errorMessage;

            return Task.FromResult(reply);
        }

        /// <summary>
        /// Record that a participant has accepted a pending contract
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Task<ParticipantAcceptsContractResult> AcceptContract(ParticipantAcceptsContractRequest request, ServerCallContext context)
        {
            string errorMessage = string.Empty;

            // we need to check authentication here

            var participant = new drumParticipant();
            participant.Alias = request.Participant.Alias;
            participant.IP4Address = request.Participant.Ip4Address;
            participant.IP6Address = request.Participant.Ip6Address;
            participant.PortNumber = Convert.ToInt32(request.Participant.DatabasePortNumber);
            participant.Id = Guid.Parse(request.Participant.ParticipantGUID);
            participant.Url = string.Empty;

            var contract = new drumContract();
            contract.ContractGUID = Guid.Parse(request.ContractGUID);
            contract.DatabaseName = request.DatabaseName;

            var result = _handler.AcceptContract(participant, contract, out errorMessage);

            var comResult = new ParticipantAcceptsContractResult();
            comResult.ContractAcceptanceIsAcknowledged = result;
            comResult.ErrorMessage = errorMessage;

            return Task.FromResult(comResult);
        }
        #endregion

        #region Private Methods
        private drumContract ConvertContractRequestToContract(SaveContractRequest request)
        {
            var dContract = new drumContract();
            dContract.Host = _handler.HostInfo;
            dContract.DatabaseName = request.Contract.Schema.DatabaseName;
            dContract.DatabaseId = Guid.Parse(request.Contract.Schema.DatabaseId);
            dContract.Description = request.Contract.Description;
            dContract.Version = Guid.Parse(request.Contract.ContractVersion);
            dContract.GeneratedDate = request.Contract.GeneratedDate.ToDateTime();
            dContract.ContractGUID = Guid.Parse(request.Contract.ContractGUID);
            dContract.Status = ContractStatus.Pending;

            foreach (var table in request.Contract.Schema.Tables)
            {
                int tableId = Convert.ToInt32(table.TableId);
                string tableName = table.TableName;
                int logicalStoragePolicy = Convert.ToInt32(table.LogicalStoragePolicy);

                var dColumns = new List<drumColumn>();

                foreach (var column in table.Columns)
                {
                    int colOrdinal = Convert.ToInt32(column.Ordinal);
                    int colLength = Convert.ToInt32(column.ColumnLength);
                    var enumColType = (SQLColumnType)column.ColumnType;
                    var colType = SQLColumnTypeConverter.Convert(enumColType, colLength);
                    var dColumn = new drumColumn(column.ColumnName, colType, colOrdinal);
                    dColumns.Add(dColumn);
                }

                var tableSchema = new drumTableSchema(tableId, tableName, dContract.DatabaseId, dColumns, dContract.DatabaseName);
                tableSchema.SetStoragePolicy((LogicalStoragePolicy)logicalStoragePolicy);

                dContract.Tables.Add(tableSchema);
            }

            var host = new HostInfo();
            host.DatabasePortNumber = Convert.ToInt32(request.Contract.HostInfo.DatabasePortNumber);
            host.HostName = request.Contract.HostInfo.HostName;
            host.HostGUID = Guid.Parse(request.Contract.HostInfo.HostGUID);
            host.IP4Address = request.Contract.HostInfo.Ip4Address;
            host.IP6Address = request.Contract.HostInfo.Ip6Address;
            host.Token = request.Contract.HostInfo.Token.ToByteArray();

            dContract.Host = host;

            return dContract;
        }

        private void LogMessageInfo(MessageInfo info)
        {
            var addresses = new string[info.MessageAddresses.Count];

            for (int i = 0; i < addresses.Length; i++)
            {
                addresses[i] = info.MessageAddresses[i];
            }

            MessageType type = (MessageType)info.MessageType;

            _handler.LogMessageInfo(info.IsLittleEndian, addresses, DateTime.Parse(info.MessageGeneratedTimeUTC), type, Guid.Parse(info.MessageGUID));
        }

        #endregion






    }
}
