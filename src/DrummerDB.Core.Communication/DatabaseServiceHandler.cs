using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using Drummersoft.DrummerDB.Common.Communication.Enum;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Diagnostics;
using System.Text;

namespace Drummersoft.DrummerDB.Core.Communication
{
    /// <summary>
    /// Provides network actions for interacting directly with databases. For more information, see INetworkManager.md
    /// </summary>
    internal class DatabaseServiceHandler
    {
        #region Private Fields
        private IAuthenticationManager _authenticationManager;
        private IDbManager _dbManager;
        private IStorageManager _storageManager;
        private HostInfo _hostInfo;
        private bool _overridesSettings = false;
        private QueryManager _queryManager;
        private LogService _logger;
        private SystemNotifications _notifications;
        #endregion

        #region Public Properties
        public HostInfo HostInfo => _hostInfo;
        #endregion

        #region Constructors
        public DatabaseServiceHandler() { }

        public DatabaseServiceHandler(IAuthenticationManager authenticationManager, IDbManager dbManager, IStorageManager storageManager, HostInfo hostInfo)
        {
            _authenticationManager = authenticationManager;
            _dbManager = dbManager;
            _storageManager = storageManager;
            _hostInfo = hostInfo;
        }
        #endregion

        #region Public Methods
        public void SetQueryManager(IQueryManager queryManager)
        {
            _queryManager = queryManager as QueryManager;
        }

        public void SetAuthentication(IAuthenticationManager auth)
        {
            _authenticationManager = auth;
        }

        public void SetDatabase(IDbManager db)
        {
            _dbManager = db;
        }

        public void SetStorage(IStorageManager storage)
        {
            _storageManager = storage;
        }

        public void SetLogger(LogService logger)
        {
            _logger = logger;
        }

        public void SetNotifications(SystemNotifications notifications)
        {
            _notifications = notifications;
            _notifications.HostInfoUpdated += HandleUpdatedHostInfo;

        }

        public bool SystemHasHost(string hostName, byte[] token)
        {
            return _authenticationManager.SystemHasHost(hostName, token);
        }

        public bool SystemHasParticipant(string participantAlias, byte[] token, string hostDbName)
        {
            var hostDb = _dbManager.GetHostDatabase(hostDbName);
            var participant = hostDb.GetParticipant(participantAlias);

            if (participant.InternalId != Guid.Empty)
            {
                if (DbBinaryConvert.BinaryEqual(participant.Token, token))
                {
                    return true;
                }
            }

            return false;
        }

        public bool SystemHasLogin(string userName, string pw)
        {
            return _authenticationManager.SystemHasLogin(userName, pw);
        }

        /// <summary>
        /// Writes method call to <see cref="Debug.WriteLine(string?)"/>
        /// </summary>
        public void HandleTest()
        {
            Debug.WriteLine("Database Service Handler Handles Test");
        }

        public void SetAuth(IAuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        public void SetHostInfo(HostInfo hostInfo, bool overridesSettings)
        {
            _hostInfo = hostInfo;
            _overridesSettings = overridesSettings;
        }

        public bool UserHasSystemPermission(string userName, SystemPermission permission)
        {
            return _authenticationManager.UserHasSystemPermission(userName, permission);
        }

        public bool CreateUserDatabase(string databaseName, out Guid databaseId)
        {
            var manager = _dbManager as DbManager;
            return manager.XactCreateNewHostDatabase(databaseName, out databaseId);
        }

        public bool InsertRowIntoTable(InsertRowRequest request)
        {
            string errorMessage = string.Empty;
            string dbName = request.Table.DatabaseName;
            string tableName = request.Table.TableName;

            var row = GetRowFromInsertRequest(request);
            var partDb = _dbManager.GetPartialDb(dbName);
            var table = partDb.GetTable(tableName);
            var insertAction = new InsertRowToPartialDbAction(row, partDb, table);

            // we should be checking the database contract if the host has permissions for this
            // or not

            return _queryManager.ExecuteDatabaseServiceAction(insertAction, out errorMessage);
        }

        /// <summary>
        /// Record in the target host databaset that the participant has accepted the contract
        /// </summary>
        /// <param name="participant">The participant sending the response</param>
        /// <param name="contract">The contract that the participant has accepted</param>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool AcceptContract(Participant participant, Contract contract, out string errorMessage)
        {
            errorMessage = string.Empty;
            participant.InternalId = _dbManager.GetHostDatabase(contract.DatabaseName).GetParticipant(participant.Alias).InternalId;
            var acceptContractAction = new AcceptContractDbAction(participant, contract, _dbManager as DbManager);
            return _queryManager.ExecuteDatabaseServiceAction(acceptContractAction, out errorMessage);
        }

        public bool SaveContract(Contract contract, out string errorMessage)
        {
            errorMessage = string.Empty;
            var saveContractAction = new SaveContractDbAction(contract, _dbManager as DbManager);
            return _queryManager.ExecuteDatabaseServiceAction(saveContractAction, out errorMessage);
        }

        public void LogMessageInfo(bool isLittleEndian, string[] ipAddresses, DateTime messageGeneratedTimeUTC, MessageType type, Guid id)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"DatabaseServiceHandler: Received Message {type} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"Message Id {id} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"Message UTC Generated: {messageGeneratedTimeUTC} ");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"IsLittleEndian: {isLittleEndian} ");
            stringBuilder.Append(Environment.NewLine);
            foreach (var address in ipAddresses)
            {
                stringBuilder.Append($"Message address: {address} ");
                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.Append($"Message recieved at participant: {_hostInfo} ");
            _logger.Info(stringBuilder.ToString());
        }

        public bool DeleteRowInPartialDb(
            Guid dbId,
            string dbName,
            uint tableId,
            string tableName,
            uint rowId
            )
        {
            // note: we didn't leverage a database action here, should we?
            // this is different from how we implemented the insert row action
            // therefore we don't have transactional data

            bool isSuccessful = false;
            PartialDb db = null;
            db = _dbManager.GetPartialDb(dbId);
            if (db is null)
            {
                db = _dbManager.GetPartialDb(dbName);
            }

            Table table = null;
            table = db.GetTable(tableId);

            if (table is null)
            {
                table = db.GetTable(tableName);
            }

            var row = table.GetRow(rowId);
            isSuccessful = table.XactDeleteRow(row);

            return isSuccessful;
        }

        public bool UpdateDeletedStatusForRow(
            Guid participantId,
            string databaseName,
            string tableName,
            uint rowId)
        {

            var db = _dbManager.GetHostDatabase(databaseName);
            var participant = db.GetParticipant(participantId, false);

            if (participant.InternalId != Guid.Empty)
            {
                var table = db.GetTable(tableName);
                var row = table.GetHostRow(rowId);

                if (row.RemoteId == participant.InternalId)
                {
                    if (db.AcceptsRemoteDeletions())
                    {
                        var hostRow = row.AsHost();
                        hostRow.IsRemoteDeleted = true;
                        hostRow.RemoteDeletionUTC = DateTime.UtcNow;
                        hostRow.Delete();
                        if (table.XactUpdateRow(hostRow))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (db.RecordsRemoteDeletions())
                    {
                        var hostRow = row.AsHost();
                        hostRow.IsRemoteDeleted = true;
                        hostRow.RemoteDeletionUTC = DateTime.UtcNow;
                        if (table.XactUpdateRow(hostRow))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (db.IgnoresRemoteDeletions())
                    {
                        // do nothing?
                        return true;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Row Remote Id does not match");
                }
            }
            else
            {
                throw new InvalidOperationException("Participant not found");
            }

            return false;

        }

        public bool UpdateDataHashForRow(
            Guid participantId,
            string databaseName,
            string tableName,
            uint rowId,
            byte[] dataHash)
        {

            var db = _dbManager.GetHostDatabase(databaseName);
            var participant = db.GetParticipant(participantId, false);

            if (participant.InternalId != Guid.Empty)
            {
                var table = db.GetTable(tableName);
                var row = table.GetHostRow(rowId);

                if (row.RemoteId == participant.InternalId)
                {
                    if (!DbBinaryConvert.BinaryEqual(dataHash, row.DataHash))
                    {
                        row.SetDataHash(dataHash);
                        table.XactUpdateRow(row);
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Data hashes are the same:");
                        Debug.WriteLine(BitConverter.ToString(dataHash));
                        Debug.WriteLine(BitConverter.ToString(row.DataHash));
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Row Remote Id does not match");
                }
            }
            else
            {
                throw new InvalidOperationException("Participant not found");
            }

            return false;
        }

        public bool UpdateRowInPartialDb(
            Guid dbId,
            string dbName,
            uint tableId,
            string tableName,
            uint rowId,
            RemoteValueUpdate updateValues,
            out byte[] newDataHash)
        {

            newDataHash = new byte[0];

            // note: we didn't leverage a database action here, should we?
            // this is different from how we implemented the insert row action
            // therefore we don't have transactional data

            bool isSuccessful = false;
            PartialDb db = null;
            db = _dbManager.GetPartialDb(dbId);
            if (db is null)
            {
                db = _dbManager.GetPartialDb(dbName);
            }

            Table table = null;
            table = db.GetTable(tableId);

            if (table is null)
            {
                table = db.GetTable(tableName);
            }

            var row = table.GetPartialRow(rowId);
            if (row is not null)
            {
                row.SetValue(updateValues.ColumnName, updateValues.Value);
                isSuccessful = table.XactUpdateRow(row);
            }

            if (isSuccessful)
            {
                var updatedRow = table.GetPartialRow(rowId);
                newDataHash = updatedRow.DataHash;
            }

            return isSuccessful;
        }

        public IRow GetRowFromPartDb(Guid databaseId, uint tableId, uint rowId, string dbName, string tableName)
        {

            // we should be checking against the database contract to make sure that the host has
            // authorization for this
            // we should also likely be logging this in the transaction log

            PartialDb db = null;
            db = _dbManager.GetPartialDb(databaseId);

            if (db is null)
            {
                db = _dbManager.GetPartialDb(dbName);
            }

            Table table = null;
            table = db.GetTable(tableId);

            if (table is null)
            {
                table = db.GetTable(tableName);
            }

            var row = table.GetRow(rowId);

            return row;
        }

        #endregion

        #region Private Methods
        private void HandleUpdatedHostInfo(object sender, EventArgs e)
        {
            if (e is HostUpdatedEventArgs)
            {
                var args = e as HostUpdatedEventArgs;
                _hostInfo = args.HostInfo;
            }
        }

        /// <summary>
        /// Attempts to turn a InsertRowRequest into a Row to be inserted in a table
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <remarks>This function serves as the inverse of the <see cref="RowValue.GetValueInBinary(bool, bool)"/> function.</remarks>
        private PartialRow GetRowFromInsertRequest(InsertRowRequest request)
        {
            string dbName = request.Table.DatabaseName;
            string tableName = request.Table.TableName;

            var partDb = _dbManager.GetPartialDb(dbName);
            var table = partDb.GetTable(tableName);
            var partialRow = table.GetNewPartialRow(request.RowId, Guid.Parse(request.HostInfo.HostGUID));

            foreach (var value in request.Values)
            {
                var binaryData = value.Value.ToByteArray();
                var colType = (SQLColumnType)value.Column.ColumnType;

                foreach (var rowValue in partialRow.Values)
                {
                    if (string.Equals(rowValue.Column.Name, value.Column.ColumnName))
                    {
                        if (!rowValue.Column.IsNullable)
                        {
                            if (rowValue.Column.IsFixedBinaryLength)
                            {
                                rowValue.SetValue(binaryData);
                                continue;
                            }
                            else
                            {
                                switch (colType)
                                {
                                    case SQLColumnType.Varchar:
                                    case SQLColumnType.Char:
                                        rowValue.SetValue(DbBinaryConvert.BinaryToString(value.Value.ToByteArray()));
                                        break;
                                    case SQLColumnType.Varbinary:
                                    case SQLColumnType.Binary:
                                        throw new NotImplementedException("Need to translate binary value");
                                    default:
                                        throw new InvalidOperationException("Unknown SQL type");
                                }
                            }
                        }
                        else // column is nullable
                        {
                            if (value.IsNullValue)
                            {
                                rowValue.SetValueAsNull();
                                continue;
                            }
                            else // is nullable column, but value is not null
                            {
                                if (rowValue.Column.IsFixedBinaryLength)
                                {
                                    rowValue.SetValue(binaryData);
                                    continue;
                                }
                                else
                                {
                                    // is a nullable column, value is not null, and is of variable length
                                    // this usually means the byte[] has
                                    // 1. a leading 1 byte BOOL indicating that the value is not null
                                    // 2. a leading 4 byte INT indicating size
                                    var span = new ReadOnlySpan<byte>(binaryData);
                                    rowValue.SetValue(span);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            return partialRow;
        }
        #endregion
    }
}
