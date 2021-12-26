using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Databases.Remote;

using System;
using System.Collections.Generic;
using System.Linq;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables;
using Drummersoft.DrummerDB.Common;

namespace Drummersoft.DrummerDB.Core.Databases
{
    /// <summary>
    /// Represents a database hosted by a Process
    /// </summary>
    internal class HostDb : UserDatabase
    {
        #region Private Fields
        private BaseUserDatabase _baseDb;
        private RemoteDataManager _remote;
        #endregion

        #region Public Properties
        public override DatabaseType DatabaseType => DatabaseType.Host;
        public override Guid Id => _baseDb.Id;
        public override string Name => _baseDb.Name;
        public override int Version => _baseDb.Version;
        #endregion

        #region Constructors
        // TODO - this entire class needs a bit more thoughtfulness.
        internal HostDb(DatabaseMetadata metadata, ITransactionEntryManager xEntryManager) : base(metadata)
        {
            _baseDb = new BaseUserDatabase(metadata, xEntryManager);
            _remote = metadata.RemoteDataManager;
        }

        internal HostDb(DatabaseMetadata metadata, ITransactionEntryManager xEntryManager, LogService log) : base(metadata)
        {
            _baseDb = new BaseUserDatabase(metadata, xEntryManager, log);
            _remote = metadata.RemoteDataManager;
        }

        #endregion

        #region Public Methods
        public void UpdateHostInfo(Guid hostGuid, string hostName, byte[] token)
        {
            _remote.UpdateHostInfo(hostGuid, hostName, token);
        }

        public void UpdateHostInfo(HostInfo hostInfo)
        {
            _remote.UpdateHostInfo(hostInfo);
        }

        public override bool AddTable(TableSchema schema, out Guid tableObjectId)
        {
            return _baseDb.AddTable(schema, out tableObjectId);
        }

        public override bool AuthorizeUser(string userName, string pwInput, DbPermission permission, Guid objectId)
        {
            return _baseDb.AuthorizeUser(userName, pwInput, permission, objectId);
        }

        public override bool CreateUser(string userName, string pwInput)
        {
            return _baseDb.CreateUser(userName, pwInput);
        }

        public Contract GetContract(Guid contractGUID)
        {
            string guid = contractGUID.ToString();

            var contracts = _baseDb.GetTable(Tables.DatabaseContracts.TABLE_NAME);

            var contractValue = RowValueMaker.Create(contracts, Tables.DatabaseContracts.Columns.ContractGUID, guid);
            var rows = contracts.GetRowsWithValue(contractValue);

            if (rows.Count != 1)
            {
                throw new InvalidOperationException("Contract not found");
            }

            var contractData = rows.First();

            var contract = new Contract();
            contract.Host = _remote.HostInfo;

            contract.ContractGUID = Guid.Parse(contractData.GetValueInString(DatabaseContracts.Columns.ContractGUID));
            contract.Description = contractData.GetValueInString(DatabaseContracts.Columns.Description);
            contract.GeneratedDate = DateTime.Parse(contractData.GetValueInString(DatabaseContracts.Columns.GeneratedDate));
            contract.Version = Guid.Parse(contractData.GetValueInString(DatabaseContracts.Columns.Version));

            // now we need to send the entire database schema over as part of the contract
            // note: we exclude the sys schema since it is reserved and should not participate in logical storage policies
            // also, we should neevr allow the user to create tables in the sys schema to "hide" data
            contract.DatabaseName = Name;
            contract.DatabaseId = Id;
            contract.Tables = _baseDb.MetaData.GetCopyOfUserTables();

            return contract;
        }

        public Guid GetCurrentContractGUID()
        {
            // need to find the max contract in the sys.DatabaseContracts table
            var contracts = _baseDb.GetTable(Tables.DatabaseContracts.TABLE_NAME);
            DateTime maxDate = DateTime.MinValue;

            var rows = contracts.GetRows();
            foreach (var row in rows)
            {
                var data = contracts.GetRow(row);
                var stringDate = data.GetValueInString(Tables.DatabaseContracts.Columns.GeneratedDate);

                var date = DateTime.Parse(stringDate);
                if (date > maxDate)
                {
                    maxDate = date;
                }
            }

            var maxDateValue = RowValueMaker.Create(contracts, DatabaseContracts.Columns.GeneratedDate, maxDate.ToString());
            var maxContractRow = contracts.GetRowsWithValue(maxDateValue);

            if (maxContractRow.Count != 1)
            {
                throw new InvalidOperationException("Max contract not found");
            }

            string stringContractGuid = maxContractRow.First().GetValueInString(DatabaseContracts.Columns.ContractGUID);

            return Guid.Parse(stringContractGuid);
        }

        public override int GetMaxTableId()
        {
            return _baseDb.GetMaxTableId();
        }

        public override List<TransactionEntry> GetOpenTransactions()
        {
            throw new NotImplementedException();
        }

        public Participant GetParticipant(string aliasName)
        {
            Participant result = new();
            var participants = _baseDb.GetTable(Tables.Participants.TABLE_NAME);
            var searchItem = RowValueMaker.Create(participants, Tables.Participants.Columns.Alias, aliasName);
            int resultCount = participants.CountOfRowsWithValue(searchItem);

            if (resultCount > 1)
            {
                throw new InvalidOperationException($"There exists multiple participants with the same alias {aliasName}");
            }

            if (resultCount == 0)
            {
                throw new InvalidOperationException($"There are no participants with the alias {aliasName}");
            }

            if (resultCount == 1)
            {
                var results = participants.GetRowsWithValue(searchItem);
                foreach (var row in results)
                {
                    result.Id = Guid.Parse(row.GetValueInString(Participants.Columns.ParticpantGUID));
                    result.IP4Address = row.GetValueInString(Participants.Columns.IP4Address);
                    result.IP6Address = row.GetValueInString(Participants.Columns.IP6Address);
                    result.PortNumber = Convert.ToInt32(row.GetValueInString(Participants.Columns.PortNumber));
                    result.Alias = row.GetValueInString(Participants.Columns.Alias);

                    result.Url = string.Empty;
                    result.UseHttps = false;
                }
            }

            return result;
        }

        public ResultsetValue XactRequestValueFromParticipant(ValueAddress address, TransactionRequest transaction, Participant participant)
        {
            var result = new ResultsetValue();
            string errorMessage = string.Empty;
            var table = GetTable(address.TableId);

            // this is inefficent
            // we only need a single value, but we've requested the entire row from the participant
            // this is because the api for com's was partially started before the api for queries
            // will need to revisit so that the com api exposes a way to get a single value from a participant

            var data = _remote.GetRowFromParticipant(participant, address.ToSQLAddress(), Name, table.Name, out errorMessage);

            // filter out by the value we're interested in
            foreach (var value in data.Values)
            {
                if (string.Equals(value.Column.Name, address.ColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    if (value.IsNull())
                    {
                        result.IsNullValue = true;
                    }
                    else
                    {
                        result.Value = value.GetValueInBinary(false, value.Column.IsNullable);
                    }
                }
            }

            return result;
        }

        public Participant GetParticipant(Guid participantId)
        {
            Participant result = new();
            var participants = _baseDb.GetTable(Tables.Participants.TABLE_NAME);
            var searchItem = RowValueMaker.Create(participants, Tables.Participants.Columns.ParticpantGUID, participantId.ToString());
            int resultCount = participants.CountOfRowsWithValue(searchItem);

            if (resultCount > 1)
            {
                throw new InvalidOperationException($"There exists multiple participants with the same id {participantId}");
            }

            if (resultCount == 0)
            {
                throw new InvalidOperationException($"There are no participants with the id {participantId}");
            }

            if (resultCount == 1)
            {
                var results = participants.GetRowsWithValue(searchItem);
                foreach (var row in results)
                {
                    result.Id = Guid.Parse(row.GetValueInString(Participants.Columns.ParticpantGUID));
                    result.IP4Address = row.GetValueInString(Participants.Columns.IP4Address);
                    result.IP6Address = row.GetValueInString(Participants.Columns.IP6Address);
                    result.PortNumber = Convert.ToInt32(row.GetValueInString(Participants.Columns.PortNumber));
                    result.Alias = row.GetValueInString(Participants.Columns.Alias);

                    result.Url = string.Empty;
                    result.UseHttps = false;
                }
            }

            return result;
        }

        public override DatabaseSchemaInfo GetSchemaInformation(string schemaName)
        {
            return _baseDb.GetSchemaInformation(schemaName);
        }

        public override Table GetTable(int tableId)
        {
            return _baseDb.GetTable(tableId);
        }

        public override Table GetTable(string tableName, string schemaName)
        {
            return _baseDb.GetTable(tableName, schemaName);
        }

        public override Table GetTable(string tableName)
        {
            return _baseDb.GetTable(tableName);
        }

        public override Guid GetTableObjectId(string tableName)
        {
            return _baseDb.GetTableObjectId(tableName);
        }

        public override bool GrantUserPermission(string userName, DbPermission permission, Guid objectId)
        {
            return _baseDb.GrantUserPermission(userName, permission, objectId);
        }

        public bool HasParticipantAlias(string aliasName)
        {
            var participants = _baseDb.GetTable(Tables.Participants.TABLE_NAME);
            var searchItem = RowValueMaker.Create(participants, Tables.Participants.Columns.Alias, aliasName);
            return participants.HasValue(searchItem);
        }

        public override bool HasSchema(string schemaName)
        {
            return _baseDb.HasSchema(schemaName);
        }

        public override bool HasTable(int tableId)
        {
            return _baseDb.HasTable(tableId);
        }

        public override bool HasTable(string tableName, string schemaName)
        {
            return _baseDb.HasTable(tableName, schemaName);
        }

        public override bool HasTable(string tableName)
        {
            return _baseDb.HasTable(tableName);
        }

        public override bool HasUser(string userName, Guid userId)
        {
            return _baseDb.HasUser(userName, userId);
        }

        public override bool HasUser(string userName)
        {
            return _baseDb.HasUser(userName);
        }

        public bool IsReadyForCooperation()
        {
            foreach (var table in _baseDb.InMemoryTables)
            {
                string schemaName = table.Schema().Schema.SchemaName;
                // need to ignore sys tables
                if (string.Equals(schemaName, Constants.SYS_SCHEMA, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lsp = table.GetLogicalStoragePolicy();
                if (lsp == LogicalStoragePolicy.None)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool LogFileHasOpenTransaction(TransactionEntryKey key)
        {
            throw new NotImplementedException();
        }

        public bool XactRequestParticipantUpdateRow(
            Participant participant,
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

            var result = _remote.UpdateRemoteRow(
                participant,
                tableName,
                tableId,
                databaseName,
                dbId,
                rowId,
                updateValue,
                transaction,
                transactionMode,
                out errorMessage
                );

            return result;
        }

        public bool XactRequestParticipantSaveLatestContract(TransactionRequest transaction, TransactionMode transactionMode, Participant participant, out string errorMessage)
        {
            var contractId = GetCurrentContractGUID();
            var contract = GetContract(contractId);
            _baseDb.XactLogParticipantSaveLatestContract(transaction, transactionMode, participant, contract);
            var isSaved = _remote.SaveContractAtParticipant(participant, contract, out errorMessage);

            var participantTable = GetTable(Participants.TABLE_NAME);
            var participantSearch = RowValueMaker.Create(participantTable, Participants.Columns.ParticpantGUID, participant.Id.ToString());
            int totalParticipants = participantTable.CountOfRowsWithValue(participantSearch);

            if (totalParticipants > 1)
            {
                throw new InvalidOperationException($"More than 1 participant found for alias {participant.Alias}");
            }

            if (totalParticipants == 0)
            {
                throw new InvalidOperationException($"Participant with alias {participant.Alias} not found. " +
                    $"Participant must be added first using DRUMMER keyword ADD PARTICIPANT");
            }
            else
            {
                var rowsForParticipant = participantTable.GetRowsWithValue(participantSearch);

                if (rowsForParticipant.Count != 1)
                {
                    throw new InvalidOperationException($"More that 1 or no participant found for alias {participant.Alias}");
                }

                foreach (var row in rowsForParticipant)
                {
                    row.SetValue(Participants.Columns.Status, Convert.ToInt32(ContractStatus.Pending).ToString());
                    row.SetValue(Participants.Columns.LastCommunicationUTC, DateTime.UtcNow.ToString());
                    participantTable.XactUpdateRow(row, transaction, transactionMode);
                }
            }

            if (!isSaved)
            {
                return false;
            }

            return true;
        }

        public bool XactUpdateParticipantAcceptsContract(Participant participant, Guid contractGuid, TransactionRequest transaction, TransactionMode transactionMode, out string errorMessage)
        {
            // ?? this bypasses the query transaction layer
            //_baseDb.XactLogParticipantAcceptsContract(null, null, participant, null);

            var contract = GetContract(contractGuid);
            _baseDb.XactLogParticipantAcceptsContract(transaction, transactionMode, participant, contract);

            // need to update the appropriate tables to show that the contract is accepted.

            var participantTable = GetTable(Participants.TABLE_NAME);

            // should we be doing this by participant GUID or participant Alias? We don't generally save off the particicpant id we're adding,
            // only the alias. probably need syntax for this.

            // we wind up in a situation where the participant id's don't match. when we add participant with alias ABCD 
            // we generate the participant id locally
            // and when the participant executes their own GENERATE HOSTINFO AS HOSTNAME ZYXW they generate their own id
            // and so these don't match when searching by participant GUID
            var participantSearch = RowValueMaker.Create(participantTable, Participants.Columns.Alias, participant.Alias);
            int totalParticipants = participantTable.CountOfRowsWithValue(participantSearch);

            if (totalParticipants != 1)
            {
                throw new InvalidOperationException($"More than 1 or no participant found for alias {participant.Alias}");
            }

            var rowsForParticipant = participantTable.GetRowsWithValue(participantSearch);

            if (rowsForParticipant.Count != 1)
            {
                throw new InvalidOperationException($"More that 1 or no participant found for alias {participant.Alias}");
            }

            foreach (var row in rowsForParticipant)
            {
                row.SetValue(Participants.Columns.Status, (Convert.ToInt32(ContractStatus.Accepted)).ToString());
                row.SetValue(Participants.Columns.AcceptedContractVersion, contractGuid.ToString());
                row.SetValue(Participants.Columns.LastCommunicationUTC, DateTime.UtcNow.ToString());
                row.SetValue(Participants.Columns.AcceptedContractDateTimeUTC, DateTime.UtcNow.ToString());
                participantTable.XactUpdateRow(row, transaction, transactionMode);
            }

            errorMessage = string.Empty;
            return true;
        }

        public bool SendContractToParticipant(string aliasName, Guid contractGUID, out string errorMessage)
        {
            var participant = GetParticipant(aliasName);
            Guid currentId = GetCurrentContractGUID();
            var contract = GetContract(currentId);

            return _remote.SaveContractAtParticipant(participant, contract, out errorMessage);
        }
        public bool SetStoragePolicyForTable(string tableName, LogicalStoragePolicy policy)
        {
            if (_baseDb.HasTable(tableName))
            {
                var table = _baseDb.GetTable(tableName);
                table.SetLogicalStoragePolicy(policy);

                var schema = _baseDb.MetaData.GetSchema(tableName, Name) as TableSchema;
                schema.SetStoragePolicy(policy);
                _baseDb.MetaData.UpdateTableSchema(schema);

                return true;
            }

            return false;
        }

        // need to refactor to leverage the underlying base db rather than internal objects
        public bool SetStoragePolicyForTable(string tableName, LogicalStoragePolicy policy, TransactionRequest transaction, TransactionMode transactionMode)
        {
            if (_baseDb.HasTable(tableName))
            {
                var table = _baseDb.GetTable(tableName);
                table.XactSetLogicalStoragePolicy(policy, transaction, transactionMode);

                var schema = _baseDb.MetaData.GetSchema(tableName, Name) as TableSchema;
                schema.SetStoragePolicy(policy);
                _baseDb.MetaData.UpdateTableSchema(schema);

                return true;
            }

            return false;
        }

        public override bool ValidateUser(string userName, string pwInput)
        {
            return _baseDb.ValidateUser(userName, pwInput);
        }

        public override bool XactAddTable(TableSchema schema, TransactionRequest transaction, TransactionMode transactionMode, out Guid tableObjectId)
        {
            return _baseDb.XactAddTable(schema, transaction, transactionMode, out tableObjectId);
        }

        public override bool XactCreateSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode)
        {
            return _baseDb.XactCreateSchema(schemaName, request, transactionMode);
        }

        public override bool XactDropTable(string tableName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            return _baseDb.XactDropTable(tableName, transaction, transactionMode);
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
