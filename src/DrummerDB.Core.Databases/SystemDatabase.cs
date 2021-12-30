using Drummersoft.DrummerDB.Core.Cryptography.Interface;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100;
using static Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100.Tables;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables;
using login = Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100.Tables.LoginTable.Columns;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;

namespace Drummersoft.DrummerDB.Core.Databases
{
    /// <summary>
    /// A system database used internally by a Process
    /// </summary>
    internal class SystemDatabase : IDatabase
    {
        #region Private Fields
        // managers
        private ICacheManager _cache;
        private ICryptoManager _crypt;
        private IStorageManager _storage;
        private ITransactionEntryManager _xEntryManager;
        private TableCollection _systemTables;

        // internal objects
        DatabaseMetadata _metadata;
        int _version;
        Guid _dbId;
        private Table _systemLogins;
        private Table _systemLoginRoles;
        private Table _systemRoles;
        private Table _systemRolePermissions;
        private Table _databaseSchemas;
        private Table _databaseSchemaPermissions;
        private Table _databaseTableDatabases;
        private Table _hostInfo;
        private Table _hosts;
        private Table _coopContracts;
        private Table _coopTables;
        private Table _coopTableSchema;
        private LogService _log;
        #endregion

        #region Public Properties
        public string Name { get; set; }
        public int Version { get; set; }
        public Guid Id => _dbId;
        #endregion

        #region Constructors
        public SystemDatabase(ICacheManager cache, ICryptoManager crypt, int version, Guid dbId, ITransactionEntryManager xEntryManager)
        {
            _cache = cache;
            _crypt = crypt;
            _version = version;
            _dbId = dbId;
            _xEntryManager = xEntryManager;

            _systemTables = new TableCollection();

            SetupTables();
        }

        public SystemDatabase(DatabaseMetadata metadata)
        {
            _metadata = metadata;
            Name = metadata.Name;
            Version = metadata.Version;
            _dbId = metadata.Id;
            _crypt = metadata.CryptoManager;
            _cache = metadata.CacheManager;
            _storage = metadata.StorageManager;
            _xEntryManager = metadata.TransactionEntryManager;

            _systemTables = new TableCollection();

            SetupTables();
        }

        public SystemDatabase(DatabaseMetadata metadata, LogService log) : this(metadata)
        {
            _log = log;
        }

        #endregion

        #region Public Methods
        public byte[] HostToken()
        {
            var hostTable = GetTable(Tables.HostInfo.TABLE_NAME);

            if (hostTable.RowCount() > 0)
            {
                var rowAddresses = hostTable.GetRows();
                foreach (var address in rowAddresses)
                {
                    var row = hostTable.GetLocalRow(address);
                    return row.GetValueInByte(Tables.HostInfo.Columns.Token);
                }
            }

            return new byte[0];
        }

        public bool IsCooperatingAsParticipant()
        {
            var hosts = GetTable(Tables.Hosts.TABLE_NAME);
            return hosts.RowCount() > 0;
        }

        public Guid HostGUID()
        {
            var hostTable = GetTable(Tables.HostInfo.TABLE_NAME);

            if (hostTable.RowCount() > 0)
            {
                var rowAddresses = hostTable.GetRows();
                foreach (var address in rowAddresses)
                {
                    var row = hostTable.GetLocalRow(address);
                    string stringGuid = row.GetValueInString(Tables.HostInfo.Columns.HostGUID);
                    return Guid.Parse(stringGuid);
                }
            }

            return Guid.Empty;
        }

        public string HostName()
        {
            var hostTable = GetTable(Tables.HostInfo.TABLE_NAME);

            if (hostTable.RowCount() > 0)
            {
                var rowAddresses = hostTable.GetRows();
                foreach (var address in rowAddresses)
                {
                    var row = hostTable.GetLocalRow(address);
                    return row.GetValueInString(Tables.HostInfo.Columns.HostName);
                }
            }

            return string.Empty;
        }

        public bool IsReadyForCooperation()
        {
            // in the system database, this will always be false becuase we don't store cooperative tables
            // in the system database, only in user databases
            return false;
        }

        public bool HasTable(string tableName)
        {
            if (tableName.Contains('.'))
            {
                var values = tableName.Split('.');
                string schema = values[0];
                string name = values[1];

                return _systemTables.Contains(name, schema);
            }
            else
            {
                return _systemTables.Contains(tableName);
            }
        }

        public bool HasTable(string tableName, string schemaName)
        {
            return _systemTables.Contains(tableName, schemaName);
        }

        public Table GetTable(uint tableId)
        {
            return _systemTables.Get(tableId);
        }

        public Table GetTable(string tableName, string schemaName)
        {
            if (tableName.Contains('.'))
            {
                var values = tableName.Split('.');
                string schema = values[0];
                string name = values[1];

                return _systemTables.Get(name, schema);

            }
            else
            {
                return _systemTables.Get(tableName, schemaName);
            }
        }

        public Table GetTable(string tableName)
        {
            if (tableName.Contains('.'))
            {
                var values = tableName.Split('.');
                string schema = values[0];
                string name = values[1];

                return _systemTables.Get(name, schema);

            }
            else
            {
                return _systemTables.Get(tableName);
            }
        }

        public Contract GetContractFromHostsTable(Guid contractGuid)
        {
            throw new NotImplementedException();
        }

        public Contract GetLatestAcceptedContractFromHostsTable(Guid hostId)
        {
            Contract returnValue = new Contract();

            var hosts = GetTable(Hosts.TABLE_NAME);
            var searchHosts = RowValueMaker.Create(hosts, Tables.Hosts.Columns.HostGUID, hostId.ToString());

            // sanity check - make sure that we actually have this host in our cooperative tables
            if (hosts.CountOfRowsWithValue(searchHosts) > 0)
            {
                var contractTable = GetTable(CooperativeContracts.TABLE_NAME);

                // need to find the latest accepted contract
                ContractStatus acceptedStatus = ContractStatus.Accepted;
                var searchAcceptedValue = RowValueMaker.Create(contractTable, CooperativeContracts.Columns.Status, Convert.ToInt32(acceptedStatus).ToString());

                var rowSearchValues = new IRowValue[2] { searchHosts, searchAcceptedValue };
                var searchResults = contractTable.GetLocalRowsWithAllValues(rowSearchValues);

                if (searchResults.Count() > 0)
                {
                    // need to find the latest accepted contract
                    DateTime contractGeneratedDate = DateTime.MinValue;
                    Guid contractGuid = Guid.Empty;
                    string contractDescription = string.Empty;
                    string contractDbName = string.Empty;
                    Guid contractDbId = Guid.Empty;
                    Guid contractVersion = Guid.Empty;

                    foreach (var result in searchResults)
                    {
                        var dt = DateTime.Parse(result.GetValueInString(CooperativeContracts.Columns.GeneratedDate));
                        if (dt > contractGeneratedDate)
                        {
                            contractGeneratedDate = dt;
                            contractGuid = Guid.Parse(result.GetValueInString(CooperativeContracts.Columns.ContractGUID));
                            contractDescription = result.GetValueInString(CooperativeContracts.Columns.Description);
                            contractDbName = result.GetValueInString(CooperativeContracts.Columns.DatabaseName);
                            contractDbId = Guid.Parse(result.GetValueInString(CooperativeContracts.Columns.DatabaseId));
                            contractVersion = Guid.Parse(result.GetValueInString(CooperativeContracts.Columns.Version));
                        }
                    }

                    if (contractGuid != Guid.Empty)
                    {
                        // we've found the max accepted contract guid for the specified host, now fill out the returnValue

                        // fill out host info
                        var hostInfo = hosts.GetLocalRowsWithValue(searchHosts);

                        if (hostInfo.Count() != 1)
                        {
                            throw new InvalidOperationException($"There are multiple hosts found with id {hostId}");
                        }

                        foreach (var host in hostInfo)
                        {
                            var hInfo = new Structures.HostInfo();
                            hInfo.HostGUID = hostId;
                            hInfo.HostName = host.GetValueInString(Hosts.Columns.HostName);
                            hInfo.DatabasePortNumber = Convert.ToInt32(host.GetValueInString(Hosts.Columns.PortNumber));
                            hInfo.IP4Address = host.GetValueInString(Hosts.Columns.IP4Address);
                            hInfo.IP6Address = host.GetValueInString(Hosts.Columns.IP6Address);
                            hInfo.Token = host.GetValueInByte(Hosts.Columns.Token);
                            returnValue.Host = hInfo;
                        }

                        // fill out contract information
                        returnValue.ContractGUID = contractGuid;
                        returnValue.GeneratedDate = contractGeneratedDate;
                        returnValue.Description = contractDescription;
                        returnValue.DatabaseId = contractDbId;
                        returnValue.DatabaseName = contractDbName;
                        returnValue.Version = contractVersion;
                        returnValue.Status = ContractStatus.Accepted;

                        // need to get the table schemas
                        var contractTables = GetTable(CooperativeTables.TABLE_NAME);
                        var searchDbId = RowValueMaker.Create(contractTables, CooperativeTables.Columns.DatabaseId, returnValue.DatabaseId.ToString());

                        var searchTableResults = contractTables.GetLocalRowsWithValue(searchDbId);
                        if (searchTableResults.Count > 0)
                        {
                            foreach (var resultTable in searchTableResults)
                            {
                                uint tableId = Convert.ToUInt32(resultTable.GetValueInString(CooperativeTables.Columns.TableId));
                                string tableName = resultTable.GetValueInString(CooperativeTables.Columns.TableName);
                                LogicalStoragePolicy policy = (LogicalStoragePolicy)Convert.ToInt32(resultTable.GetValueInString(CooperativeTables.Columns.LogicalStoragePolicy));

                                var tableSchemaTable = GetTable(CooperativeTableSchemas.TABLE_NAME);
                                var tableSchemaSearchTableId = RowValueMaker.Create(tableSchemaTable, CooperativeTableSchemas.Columns.TableId, tableId.ToString());
                                var tableSchemaSearchDatabaseId = RowValueMaker.Create(tableSchemaTable, CooperativeTableSchemas.Columns.DatabaseId, returnValue.DatabaseId.ToString());

                                var searchColumnValues = new IRowValue[2] { tableSchemaSearchTableId, tableSchemaSearchDatabaseId };
                                var schemaTablesResults = contractTables.GetRowsWithAllValues(searchColumnValues);

                                List<ColumnSchema> columns = new List<ColumnSchema>();

                                if (schemaTablesResults.Count() > 0)
                                {
                                    // for each table in CooperativeTables
                                    foreach (var schemaTableResult in schemaTablesResults)
                                    {
                                        // look up each column for that table
                                        var tableColumnSchema = GetTable(CooperativeTableSchemas.TABLE_NAME);
                                        var columnResults = tableColumnSchema.GetLocalRowsWithAllValues(searchColumnValues);

                                        foreach (var columnResult in columnResults)
                                        {
                                            string columnName = columnResult.GetValueInString(CooperativeTableSchemas.Columns.ColumnName).Trim();
                                            var enumType = (SQLColumnType)Convert.ToInt32(columnResult.GetValueInString(CooperativeTableSchemas.Columns.ColumnType));
                                            ISQLType type = SQLColumnTypeConverter.Convert(enumType, Constants.DatabaseVersions.V100);
                                            uint colLength = Convert.ToUInt32(columnResult.GetValueInString(CooperativeTableSchemas.Columns.ColumnLength));
                                            uint colOrdinal = Convert.ToUInt32(columnResult.GetValueInString(CooperativeTableSchemas.Columns.ColumnOrdinal));
                                            bool colIsNullable = DbBinaryConvert.BinaryToBoolean(columnResult.GetValueInByteSpan(CooperativeTableSchemas.Columns.ColumnIsNullable));
                                            int colBinaryOrder = Convert.ToInt32(columnResult.GetValueInString(CooperativeTableSchemas.Columns.ColumnBinaryOrder));

                                            var columnSchema = new ColumnSchema(columnName, type, colOrdinal, colIsNullable);
                                            columnSchema.Length = colLength;
                                            columns.Add(columnSchema);
                                        }
                                    }
                                }

                                var tableSchema = new TableSchema(tableId, tableName.Trim(), returnValue.DatabaseId, columns, Name);

                                if (returnValue.Tables is null)
                                {
                                    returnValue.Tables = new List<ITableSchema>();
                                }

                                returnValue.Tables.Add(tableSchema);
                            }
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool HasContractInHostsTable(Contract contract)
        {
            var hosts = GetTable(Hosts.TABLE_NAME);
            string hostId = contract.Host.HostGUID.ToString();

            var hostValue = RowValueMaker.Create(hosts, Hosts.Columns.HostGUID, hostId);
            uint countOfHosts = hosts.CountOfRowsWithValue(hostValue);

            if (countOfHosts > 0)
            {
                var coContractsTable = GetTable(CooperativeContracts.TABLE_NAME);
                var contractGUID = contract.ContractGUID;
                var contractGUIDValue = RowValueMaker.Create(coContractsTable, CooperativeContracts.Columns.ContractGUID, contractGUID.ToString());
                uint countOfExistingContracts = coContractsTable.CountOfRowsWithValue(contractGUIDValue);

                return countOfExistingContracts > 0;
            }

            return false;
        }

        public bool XactSaveContractToHostsTable(Contract contract, TransactionRequest transaction, TransactionMode transactionMode)
        {
            var hosts = GetTable(Hosts.TABLE_NAME);
            string hostId = contract.Host.HostGUID.ToString();

            var hostValue = RowValueMaker.Create(hosts, Hosts.Columns.HostGUID, hostId);
            uint countOfHosts = hosts.CountOfRowsWithValue(hostValue);

            if (countOfHosts == 0)
            {
                _storage.SaveContractToDisk(contract);

                // need host, we need to add this
                return XactSaveNewContract(contract, transaction, transactionMode);
            }

            if (countOfHosts == 1)
            {
                // we need to update an existing contract
                return XactSaveExistingContract(contract, transaction, transactionMode);
            }

            if (countOfHosts > 1)
            {
                throw new InvalidOperationException("There somehow exists mutiple hosts with the same id");
            }

            return false;
        }

        public bool HasLogin(string userName)
        {
            var valueUserName = new RowValue();
            valueUserName.SetColumn(_systemLogins.GetColumn(login.UserName));
            valueUserName.SetValue(userName);

            return _systemLogins.HasValue(valueUserName);
        }

        public bool HasLogin(string userName, Guid userId)
        {
            var values = new List<RowValue>();

            var valueUserName = RowValueMaker.Create(_systemLogins, login.UserName, userName);
            values.Add(valueUserName);

            var valueUserId = RowValueMaker.Create(_systemLogins, login.UserGUID, userId.ToString().ToUpper());
            values.Add(valueUserId);

            return _systemLogins.HasAllValues(values);
        }

        /// <summary>
        /// Adds the login to the System database (for the Process)
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="pwInput">The pw input.</param>
        /// <param name="userGUID">The user unique identifier.</param>
        /// <param name="isAdminLogin">If the login is a system admin</param>
        /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
        /// <remarks><seealso cref="DbMetaSystemDataPages.CreateUser(string, string)"/></remarks>
        public bool AddLogin(string userName, string pwInput, Guid userGUID, bool isAdminLogin)
        {
            int iterations = _crypt.GetRandomNumber();
            int length = _crypt.GetByteLength();

            var pwByte = Encoding.ASCII.GetBytes(pwInput);

            var salt = _crypt.GenerateSalt(length);
            var hash = _crypt.GenerateHash(pwByte, salt, iterations, length);

            var row = _systemLogins.GetNewLocalRow();
            row.SetValue(login.UserName, userName);
            row.SetValue(login.UserGUID, userGUID.ToString().ToUpper());
            row.SetValue(login.Salt, salt);
            row.SetValue(login.ByteLength, length.ToString());
            row.SetValue(login.Hash, hash);
            row.SetValue(login.IsBanned, "false");
            row.SetValue(login.Workfactor, iterations.ToString());
            _systemLogins.XactAddRow(row);

            if (_log is not null)
            {
                _log.Info($"User {userName} login created in database {_metadata.Name}");
            }

            return true;
        }

        public bool ValidateHost(string hostName, byte[] token)
        {
            var hosts = GetTable(Hosts.TABLE_NAME);

            var searchValue = RowValueMaker.Create(hosts, Hosts.Columns.HostName, hostName);
            uint totalCount = hosts.CountOfRowsWithValue(searchValue);

            if (totalCount == 0)
            {
                return false;
            }

            if (totalCount > 1)
            {
                throw new InvalidOperationException("We have multiple entries for the same host somehow.");
            }
            else
            {
                var searchValueToken = RowValueMaker.Create(hosts, Hosts.Columns.Token, token);
                var searchValues = new RowValue[2] { searchValue, searchValueToken };
                uint countOfHosts = hosts.CountOfRowsWithAllValues(searchValues);

                if (countOfHosts == 1)
                {
                    var rows = hosts.GetLocalRowsWithAllValues(searchValues);
                    foreach (var row in rows)
                    {
                        var storedToken = row.GetValueInByte(Hosts.Columns.Token);

                        // need to trim sent token - the sent token includes the size of bool and the size of the token itself
                        // that we need to remove before we compare with what we have in storage
                        var span = new ReadOnlySpan<byte>(token);
                        int prefix = Constants.SIZE_OF_BOOL + Constants.SIZE_OF_INT;
                        var trimToken = span.Slice(prefix, token.Length - prefix);

                        if (DbBinaryConvert.BinaryEqual(trimToken, storedToken))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }

            return false;
        }

        public bool ValidateLogin(string userName, string pwInput)
        {
            bool result = false;

            var pwByte = Encoding.ASCII.GetBytes(pwInput);

            var valueUserName = RowValueMaker.Create(_systemLogins, login.UserName, userName);

            uint count = _systemLogins.CountOfRowsWithValue(valueUserName);
            if (count > 1)
            {
                throw new InvalidOperationException($"Muliple logins found for user {userName}");
            }

            var rows = _systemLogins.GetLocalRowsWithValue(valueUserName);

            foreach (var row in rows)
            {
                byte[] hash = row.GetValueInByte(login.Hash);
                byte[] salt = row.GetValueInByte(login.Salt);
                int iWorkFactor =
                    Convert.ToInt32(row.GetValueInString(login.Workfactor));
                int iByteLength =
                    Convert.ToInt32(row.GetValueInString(login.ByteLength));

                byte[] computedHash = _crypt.GenerateHash(pwByte, salt, iWorkFactor, iByteLength);

                if (hash.SequenceEqual(computedHash))
                {
                    result = true;
                }
            }

            return result;
        }

        public bool HasHost(Guid hostId)
        {
            var hostTable = GetTable(Hosts.TABLE_NAME);
            var hostIdValue = RowValueMaker.Create(hostTable, Hosts.Columns.HostGUID, hostId.ToString());

            return hostTable.CountOfRowsWithValue(hostIdValue) > 0;
        }

        public void AssignUserToDefaultSystemAdmin(string userName)
        {
            if (HasLogin(userName))
            {
                string name = SystemDatabaseConstants100.SystemLoginConstants.SystemRoles.Names.SystemAdmin;
                var permissions = new List<SystemPermission>();
                permissions.Add(SystemPermission.FullAccess);
                permissions.Add(SystemPermission.CreateHostDatabase);
                SystemRole admin = new SystemRole(name, permissions);
                AddUserToSystemRole(admin, userName);
            }
            else
            {
                throw new InvalidOperationException($"{userName} was not found as a login");
            }
        }

        public bool IsUserInSystemRole(string userName)
        {
            var searchItem = RowValueMaker.Create(_systemLoginRoles, LoginRolesTable.Columns.UserName, userName);
            return _systemLoginRoles.HasValue(searchItem);
        }

        public bool UserHasSystemPermission(string userName, SystemPermission permission)
        {
            var searchUserName = RowValueMaker.Create(_systemLoginRoles, LoginRolesTable.Columns.UserName, userName);
            List<Row> rolesForUser = _systemLoginRoles.GetRowsWithValue(searchUserName);

            foreach (var role in rolesForUser)
            {
                string roleName = role.AsLocal().GetValueInString(LoginRolesTable.Columns.RoleName);

                RowValue searchRoleName = RowValueMaker.Create(_systemRolePermissions, SystemRolesPermissions.Columns.RoleName,
                    roleName);

                List<Row> permissions = _systemRolePermissions.GetRowsWithValue(searchRoleName);
                foreach (var item in permissions)
                {
                    string permissionString = item.AsLocal().GetValueInString(SystemRolesPermissions.Columns.SystemPermission);
                    int permissionInt = Convert.ToInt32(permissionString);
                    SystemPermission storedPermission = (SystemPermission)permissionInt;
                    if (storedPermission == permission || storedPermission == SystemPermission.FullAccess)
                    {
                        return true;
                    }
                }

            }

            return false;
        }

        public void LogOpenTransaction(Guid databaseId, TransactionEntry transaction)
        {
            _storage.LogOpenTransaction(databaseId, transaction);
        }

        public void LogCloseTransaction(Guid databaseId, TransactionEntry transaction)
        {
            _storage.LogCloseTransaction(databaseId, transaction);
        }

        public void RemoveOpenTransaction(Guid databaseId, TransactionEntry transaction)
        {
            _storage.RemoveOpenTransaction(databaseId, transaction);
        }

        public void LoadDbTableWithDbNames(UserDatabaseCollection databases)
        {
            foreach (var db in databases)
            {
                var dbName = RowValueMaker.Create(_databaseTableDatabases, DatabaseTableDatabases.Columns.DatabaseName, db.Name);
                uint count = _databaseTableDatabases.CountOfRowsWithValue(dbName);

                if (count == 0)
                {
                    var record = _databaseTableDatabases.GetNewLocalRow();
                    record.SetValue(DatabaseTableDatabases.Columns.DatabaseName, db.Name);

                    int dbType = (int)db.DatabaseType;
                    record.SetValue(DatabaseTableDatabases.Columns.DatabaseType, dbType.ToString());

                    _databaseTableDatabases.XactAddRow(record);
                }
            }
        }

        public void XactAddNewPartDbNameToDatabasesTable(string dbName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            // this needs to specify the database type of partial
            var dbNameSearch = RowValueMaker.Create(_databaseTableDatabases, DatabaseTableDatabases.Columns.DatabaseName, dbName);
            var dbTypeSearch = RowValueMaker.Create(_databaseTableDatabases, DatabaseTableDatabases.Columns.DatabaseType, Convert.ToInt32(DatabaseType.Partial).ToString());

            var searchItems = new IRowValue[2] { dbNameSearch, dbTypeSearch };

            uint count = _databaseTableDatabases.CountOfRowsWithAllValues(searchItems);

            if (count == 0)
            {
                int hostDbType = (int)DatabaseType.Partial;

                var record = _databaseTableDatabases.GetNewLocalRow();
                record.SetValue(DatabaseTableDatabases.Columns.DatabaseName, dbName);
                record.SetValue(DatabaseTableDatabases.Columns.DatabaseType, hostDbType.ToString());

                _databaseTableDatabases.XactAddRow(record, transaction, transactionMode);
            }
        }

        public void XactAddNewHostDbNameToDatabasesTable(string dbName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            // this needs to specify the database type of host
            var dbNameSearch = RowValueMaker.Create(_databaseTableDatabases, DatabaseTableDatabases.Columns.DatabaseName, dbName);
            var dbTypeSearch = RowValueMaker.Create(_databaseTableDatabases, DatabaseTableDatabases.Columns.DatabaseType, Convert.ToInt32(DatabaseType.Host).ToString());

            var searchItems = new IRowValue[2] { dbNameSearch, dbTypeSearch };

            uint count = _databaseTableDatabases.CountOfRowsWithAllValues(searchItems);

            if (count == 0)
            {
                int hostDbType = (int)DatabaseType.Host;

                var record = _databaseTableDatabases.GetNewLocalRow();
                record.SetValue(DatabaseTableDatabases.Columns.DatabaseName, dbName);
                record.SetValue(DatabaseTableDatabases.Columns.DatabaseType, hostDbType.ToString());

                _databaseTableDatabases.XactAddRow(record, transaction, transactionMode);
            }
        }

        public void XactRemoveDbNameFromDatabasesTable(string dbName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            var dbNameSearch = RowValueMaker.Create(_databaseTableDatabases, DatabaseTableDatabases.Columns.DatabaseName, dbName);
            uint count = _databaseTableDatabases.CountOfRowsWithValue(dbNameSearch);

            if (count > 0)
            {
                var records = _databaseTableDatabases.GetRowsWithValue(dbNameSearch);
                foreach (var record in records)
                {
                    _databaseTableDatabases.XactDeleteRow(record, transaction, transactionMode);
                }
            }
        }
        #endregion

        #region Private Methods
        private void SetupTables()
        {
            SetupSystemLoginsTable();
            SetupSystemLoginsRolesTable();
            SetupSystemRolesTable();
            SetupSystemRolePermisionsTable();
            AddDefaultRolesAndPermissionsToTable();
            SetupSchemas();
            SetupDatabaseTable();
            SetupHostInfoTable();
            SetupHostsTable();
            SetupCoopContractTable();
            SetupCoopTablesTable();
            SetupCoopTableSchemaTable();
        }

        private void SetupHostInfoTable()
        {
            _hostInfo = new Table(SystemDatabaseConstants100.Tables.HostInfo.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_hostInfo);
        }

        private void SetupDatabaseTable()
        {
            _databaseTableDatabases = new Table(DatabaseTableDatabases.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_databaseTableDatabases);
        }

        private void SetupSchemas()
        {
            _databaseSchemas = new Table(DatabaseSchemas.Schema(_dbId, Name), _cache, _storage, _xEntryManager);
            _databaseSchemaPermissions = new Table(DatabaseSchemaPermissions.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_databaseSchemas);
            _systemTables.Add(_databaseSchemaPermissions);

            // check to see if default schemas exist, if not, add them
            var dboSchema = RowValueMaker.Create(_databaseSchemas, DatabaseSchemas.Columns.SchemaName, Constants.DBO_SCHEMA);
            if (!_databaseSchemas.HasValue(dboSchema))
            {
                var row = _databaseSchemas.GetNewLocalRow();
                row.SetValue(DatabaseSchemas.Columns.SchemaName, Constants.DBO_SCHEMA);
                row.SetValue(DatabaseSchemas.Columns.SchemaGUID, Constants.DBO_SCHEMA_GUID);
                row.SetValueAsNullForColumn(DatabaseSchemas.Columns.ContractGUID);
                _databaseSchemas.XactAddRow(row);
            }

            var sysSchema = RowValueMaker.Create(_databaseSchemas, DatabaseSchemas.Columns.SchemaName, Constants.SYS_SCHEMA);
            if (!_databaseSchemas.HasValue(sysSchema))
            {
                var row = _databaseSchemas.GetNewLocalRow();
                row.SetValue(DatabaseSchemas.Columns.SchemaName, Constants.SYS_SCHEMA);
                row.SetValue(DatabaseSchemas.Columns.SchemaGUID, Constants.SYS_SCHEMA_GUID);
                row.SetValueAsNullForColumn(DatabaseSchemas.Columns.ContractGUID);
                _databaseSchemas.XactAddRow(row);
            }

            // auto grant any role with full access permission to dbo and sys schemas
            var fullAccess = RowValueMaker.Create(_systemRolePermissions, SystemDatabaseConstants100.Tables.SystemRolesPermissions.Columns.SystemPermission,
                Convert.ToString((int)SystemPermission.FullAccess));

            var count = _systemRolePermissions.CountOfRowsWithValue(fullAccess);

            if (count > 0)
            {
                var rows = _systemRolePermissions.GetLocalRowsWithValue(fullAccess);
                foreach (var row in rows)
                {
                    // find the users in the role that has full access and grant those users full rights to the dbo and sys schemas
                    var findUsers = RowValueMaker.Create(_systemLoginRoles,
                        SystemDatabaseConstants100.Tables.LoginRolesTable.Columns.RoleName, row.GetValueInString(SystemDatabaseConstants100.Tables.SystemRolesPermissions.Columns.RoleName));

                    var loginCount = _systemLoginRoles.CountOfRowsWithValue(findUsers);

                    if (loginCount > 0)
                    {
                        var users = _systemLoginRoles.GetLocalRowsWithValue(findUsers);
                        foreach (var user in users)
                        {
                            var record = _databaseSchemaPermissions.GetNewLocalRow();

                            record.SetValue(DatabaseSchemaPermissions.Columns.UserName, user.GetValueInByte(LoginRolesTable.Columns.UserName));
                            record.SetValue(DatabaseSchemaPermissions.Columns.UserGUID, user.GetValueInByte(LoginRolesTable.Columns.UserGUID));
                            record.SetValue(DatabaseSchemaPermissions.Columns.SchemaGUID, Constants.DBO_SCHEMA_GUID);
                            record.SetValue(DatabaseSchemaPermissions.Columns.DbPermission, Convert.ToString((int)DbPermission.FullAccess));
                            record.SetValue(DatabaseSchemaPermissions.Columns.SchemaGUID, Constants.SYS_SCHEMA_GUID);
                            record.SetValue(DatabaseSchemaPermissions.Columns.DbPermission, Convert.ToString((int)DbPermission.FullAccess));

                            _databaseSchemaPermissions.XactAddRow(record);
                        }
                    }
                }
            }
        }

        private void AddUserToSystemRole(SystemRole role, string userName)
        {
            if (HasSystemRole(role))
            {
                Guid roleGuid = Guid.Empty;
                string roleName = string.Empty;

                RowValue searchForRole = RowValueMaker.Create(_systemRoles, SystemRolesTable.Columns.RoleName, role.Name);
                var roles = _systemRoles.GetLocalRowsWithValue(searchForRole);

                foreach (var x in roles)
                {
                    roleName = x.GetValueInString(SystemRolesTable.Columns.RoleName);
                    if (roleName == role.Name)
                    {
                        string roleGuidString = x.GetValueInString(SystemDatabaseConstants100.Tables.SystemRolesTable.Columns.RoleGUID);
                        roleGuid = Guid.Parse(roleGuidString);
                    }
                }

                LocalRow row = _systemLoginRoles.GetNewLocalRow();
                row.SetValue(LoginRolesTable.Columns.RoleName, role.Name);
                row.SetValue(LoginRolesTable.Columns.RoleGUID, roleGuid.ToString());
                row.SetValue(LoginRolesTable.Columns.UserName, userName);
                row.SetValue(LoginRolesTable.Columns.UserGUID, Guid.Empty.ToString()); // this is lazy
                _systemLoginRoles.XactAddRow(row);

                foreach (var permission in role.Permisisons)
                {
                    var permissionToCheck =
                        RowValueMaker.Create(_systemRolePermissions, SystemRolesPermissions.Columns.SystemPermission, Convert.ToString((int)permission));

                    uint count = _systemRolePermissions.CountOfRowsWithValue(permissionToCheck);

                    if (count == 0)
                    {
                        var permissionToAdd = _systemRolePermissions.GetNewLocalRow();
                        permissionToAdd.SetValue(SystemRolesPermissions.Columns.RoleName, roleName);
                        permissionToAdd.SetValue(SystemRolesPermissions.Columns.RoleGUID, roleGuid.ToString());
                        permissionToAdd.SetValue(SystemRolesPermissions.Columns.SystemPermission, Convert.ToString((int)permission));
                        _systemRolePermissions.XactAddRow(permissionToAdd);
                    }
                }

                if (_log is not null)
                {
                    _log.Info($"User {userName} added to system role {role.Name}");
                }
            }
            else
            {
                throw new InvalidOperationException($"{role.Name} is not a defined system role");
            }
        }

        private bool XactSaveNewContract(Contract contract, TransactionRequest transaction, TransactionMode transactionMode)
        {
            // add record to hosts table
            var hosts = GetTable(Hosts.TABLE_NAME);

            var host = contract.Host;

            string hostId = host.HostGUID.ToString();
            string hostName = host.HostName;
            byte[] token = host.Token;
            string ip4 = host.IP4Address;
            string ip6 = host.IP6Address;
            int portNumber = host.DatabasePortNumber;

            var hostRow = hosts.GetNewLocalRow();
            hostRow.SetValue(Hosts.Columns.HostGUID, hostId);
            hostRow.SetValue(Hosts.Columns.HostName, hostName);

            // need to add the 1 byte NOT NULL prefix to token
            bool isNotFalse = false;
            var bIsNotFalse = DbBinaryConvert.BooleanToBinary(isNotFalse);
            var bTokenArray = new byte[bIsNotFalse.Length + token.Length];

            Array.Copy(bIsNotFalse, 0, bTokenArray, 0, bIsNotFalse.Length);
            Array.Copy(token, 0, bTokenArray, bIsNotFalse.Length, token.Length);

            hostRow.SetValue(Hosts.Columns.Token, bTokenArray);
            //hostRow.SetValue(Hosts.Columns.Token, host.Token);
            // end token save value

            hostRow.SetValue(Hosts.Columns.IP4Address, ip4);
            hostRow.SetValue(Hosts.Columns.IP6Address, ip6);
            hostRow.SetValue(Hosts.Columns.PortNumber, portNumber.ToString());
            hostRow.SetValue(Hosts.Columns.LastCommunicationUTC, DateTime.UtcNow.ToString());
            hosts.XactAddRow(hostRow, transaction, transactionMode);

            // save contract data to all contract tables
            var coopContracts = GetTable(CooperativeContracts.TABLE_NAME);

            var coopContractRow = coopContracts.GetNewLocalRow();
            coopContractRow.SetValue(CooperativeContracts.Columns.HostGuid, hostId);
            coopContractRow.SetValue(CooperativeContracts.Columns.ContractGUID, contract.ContractGUID.ToString());
            coopContractRow.SetValue(CooperativeContracts.Columns.DatabaseName, contract.DatabaseName);
            coopContractRow.SetValue(CooperativeContracts.Columns.DatabaseId, contract.DatabaseId.ToString());
            coopContractRow.SetValue(CooperativeContracts.Columns.Description, contract.Description);
            coopContractRow.SetValue(CooperativeContracts.Columns.Version, contract.Version.ToString());
            coopContractRow.SetValue(CooperativeContracts.Columns.GeneratedDate, contract.GeneratedDate.ToString());
            coopContractRow.SetValue(CooperativeContracts.Columns.Status, Convert.ToInt32(contract.Status).ToString());
            coopContracts.XactAddRow(coopContractRow, transaction, transactionMode);

            var coopTable = GetTable(CooperativeTables.TABLE_NAME);
            var coopTableColumn = GetTable(CooperativeTableSchemas.TABLE_NAME);

            foreach (var table in contract.Tables)
            {
                var coopTableRow = coopTable.GetNewLocalRow();
                coopTableRow.SetValue(CooperativeTables.Columns.TableId, table.Id.ToString());
                coopTableRow.SetValue(CooperativeTables.Columns.TableName, table.Name);
                coopTableRow.SetValue(CooperativeTables.Columns.DatabaseName, contract.DatabaseName);
                coopTableRow.SetValue(CooperativeTables.Columns.DatabaseId, contract.DatabaseId.ToString());
                coopTableRow.SetValue(CooperativeTables.Columns.LogicalStoragePolicy, Convert.ToInt32(table.StoragePolicy).ToString());
                coopTable.XactAddRow(coopTableRow, transaction, transactionMode);

                foreach (var column in table.Columns)
                {
                    var colRow = coopTableColumn.GetNewLocalRow();
                    colRow.SetValue(CooperativeTableSchemas.Columns.TableId, table.Id.ToString());
                    colRow.SetValue(CooperativeTableSchemas.Columns.DatabaseId, contract.DatabaseId.ToString());
                    colRow.SetValue(CooperativeTableSchemas.Columns.ColumnName, column.Name);
                    colRow.SetValue(CooperativeTableSchemas.Columns.ColumnId, column.Id.ToString());

                    var enumColDataType = SQLColumnTypeConverter.Convert(column.DataType, Constants.DatabaseVersions.V100);

                    colRow.SetValue(CooperativeTableSchemas.Columns.ColumnType, Convert.ToInt32(enumColDataType).ToString());
                    colRow.SetValue(CooperativeTableSchemas.Columns.ColumnLength, column.Length.ToString());
                    colRow.SetValue(CooperativeTableSchemas.Columns.ColumnOrdinal, column.Ordinal.ToString());
                    colRow.SetValue(CooperativeTableSchemas.Columns.ColumnIsNullable, column.IsNullable.ToString());
                    colRow.SetValue(CooperativeTableSchemas.Columns.ColumnBinaryOrder, column.Ordinal.ToString());
                    coopTableColumn.XactAddRow(colRow, transaction, transactionMode);
                }
            }

            return true;
        }

        private bool XactSaveExistingContract(Contract contract, TransactionRequest transaction, TransactionMode transactionMode)
        {
            throw new NotImplementedException();
        }

        private void SetupSystemLoginsTable()
        {
            _systemLogins = new Table(LoginTable.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_systemLogins);
        }

        private void SetupSystemLoginsRolesTable()
        {
            _systemLoginRoles = new Table(LoginRolesTable.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_systemLoginRoles);
        }

        private void SetupSystemRolesTable()
        {
            _systemRoles = new Table(SystemRolesTable.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_systemRoles);

        }

        private void SetupSystemRolePermisionsTable()
        {
            _systemRolePermissions = new Table(SystemRolesPermissions.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_systemRolePermissions);
        }

        private void SetupHostsTable()
        {
            _hosts = new Table(Hosts.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_hosts);
        }

        private void SetupCoopContractTable()
        {
            _coopContracts = new Table(CooperativeContracts.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_coopContracts);
        }

        private void SetupCoopTablesTable()
        {
            _coopTables = new Table(CooperativeTables.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_coopTables);
        }

        private void SetupCoopTableSchemaTable()
        {
            _coopTableSchema = new Table(CooperativeTableSchemas.Schema(_dbId, Name), _cache, _storage, _xEntryManager);

            _systemTables.Add(_coopTableSchema);
        }

        private void AddDefaultRolesAndPermissionsToTable()
        {
            var systemAdmin = RowValueMaker.Create(_systemRoles, SystemDatabaseConstants100.Tables.SystemRolesTable.Columns.RoleName,
                SystemDatabaseConstants100.SystemLoginConstants.SystemRoles.Names.SystemAdmin);

            if (!_systemRoles.HasValue(systemAdmin))
            {
                LocalRow role = _systemRoles.GetNewLocalRow();

                var guid = Guid.NewGuid();

                role.SetValue(SystemDatabaseConstants100.Tables.SystemRolesTable.Columns.RoleName,
                    SystemDatabaseConstants100.SystemLoginConstants.SystemRoles.Names.SystemAdmin);
                role.SetValue(SystemDatabaseConstants100.Tables.SystemRolesTable.Columns.RoleGUID, guid.ToString());
                _systemRoles.XactAddRow(role);

                LocalRow permission = _systemRolePermissions.GetNewLocalRow();
                permission.SetValue(SystemDatabaseConstants100.Tables.SystemRolesPermissions.Columns.RoleName,
                    SystemDatabaseConstants100.SystemLoginConstants.SystemRoles.Names.SystemAdmin);
                permission.SetValue(SystemDatabaseConstants100.Tables.SystemRolesPermissions.Columns.RoleGUID, guid.ToString());
                permission.SetValue(SystemDatabaseConstants100.Tables.SystemRolesPermissions.Columns.SystemPermission,
                    Convert.ToInt32(SystemPermission.FullAccess).ToString());

                _systemRolePermissions.XactAddRow(permission);
            }

        }

        private bool HasSystemRole(SystemRole role)
        {
            RowValue searchItem = RowValueMaker.Create(_systemRoles, SystemDatabaseConstants100.Tables.SystemRolesTable.Columns.RoleName,
                role.Name);
            return _systemRoles.HasValue(searchItem);
        }
        #endregion

    }
}