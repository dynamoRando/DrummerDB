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
            int tableId,
            string tableName,
            int rowId
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

        public bool UpdateRowInPartialDb(
            Guid dbId,
            string dbName,
            int tableId,
            string tableName,
            int rowId,
            RemoteValueUpdate updateValues)
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
            if (row is not null)
            {
                row.SetValue(updateValues.ColumnName, updateValues.Value);
                isSuccessful = table.XactUpdateRow(row);
            }

            return isSuccessful;
        }

        public IRow GetRowFromPartDb(Guid databaseId, int tableId, int rowId, string dbName, string tableName)
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
        private Row GetRowFromInsertRequest(InsertRowRequest request)
        {
            string dbName = request.Table.DatabaseName;
            string tableName = request.Table.TableName;

            var partDb = _dbManager.GetPartialDb(dbName);
            var table = partDb.GetTable(tableName);
            var localRow = table.GetNewLocalRow();
            localRow.Id = Convert.ToInt32(request.RowId);

            foreach (var value in request.Values)
            {
                var binaryData = value.Value.ToByteArray();
                var colType = (SQLColumnType)value.Column.ColumnType;

                foreach (var rowValue in localRow.Values)
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

            return localRow;
        }
        #endregion
    }
}
