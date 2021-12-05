using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Cryptography.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables;
using dbs = Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables.DatabaseSchemas.Columns;
using u = Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables.Users.Columns;
using uo = Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables.UserObjects.Columns;
using uop = Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables.UserObjectPermissions.Columns;
using ut = Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables.UserTable.Columns;
using uts = Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables.UserTableSchema.Columns;

namespace Drummersoft.DrummerDB.Core.Databases
{
    /// <summary>
    ///  An abstraction over the system data pages
    /// </summary>
    internal class DbMetaSystemDataPages
    {
        #region Private Fields
        private ICacheManager _cache;
        private ICryptoManager _crypt;
        private IStorageManager _storage;
        private Guid _dbId;
        private int _version;
        private ITransactionEntryManager _xEntryManager;
        private string _dbName;
        private List<Table> _systemTables;

        // user tables
        private Table _userTable;
        private Table _userTableSchema;
        private Table _userObjects;
        private Table _users;
        private Table _userObjectPermissions;
        private Table _databaseSchemas;
        private Table _databaseSchemaPermissions;
        private Table _participants;
        private Table _databaseContracts;
        // TODO - going to need a system participant table that has a Tree in cache
        #endregion

        #region Public Properties
        public List<Table> SystemTables => _systemTables;
        #endregion

        #region Constructors        
        /// <summary>
        /// Makes a new <see cref="DbMetaSystemDataPages"/>. Serves as an abstraction over the system data pages in the database.
        /// </summary>
        /// <param name="cache">A reference to the cache manager</param>
        /// <param name="dbId">The database id</param>
        /// <param name="version">The database version</param>
        /// <param name="crypt">A reference to the cryto manager</param>
        public DbMetaSystemDataPages(ICacheManager cache, Guid dbId, int version, ICryptoManager crypt, IStorageManager storage, ITransactionEntryManager xEntryManager, string dbName)
        {
            _cache = cache;
            _dbId = dbId;
            _version = version;
            _crypt = crypt;
            _storage = storage;
            _xEntryManager = xEntryManager;
            _dbName = dbName;
            _systemTables = new List<Table>();

            SetupDatabaseSchemas();
            SetupUserTable();
            SetupUserTableSchema();
            SetupUserObjects();
            SetUserTable();
            SetupUserTablePermissions();
            SetupParticipantTable();
            SetupDatabaseContracts();

            AddSystemTables();
        }
        #endregion

        #region Public Methods
        public void DropTable(string tableName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            // need to remove the specified table from all the metadata tables
            // user tables
            // user table schemas
            // etc

            var ut_value = RowValueMaker.Create(_userTable, ut.TableName, tableName, true);
            var ut_results = _userTable.GetRowsWithValue(ut_value);

            foreach (var row in ut_results)
            {
                _userTable.TryDeleteRow(row, transaction, transactionMode);
            }

            var uts_value = RowValueMaker.Create(_userTableSchema, uts.TableId, ut_results.First().GetValueInString(ut.TableId), true);
            var uts_results = _userTableSchema.GetRowsWithValue(uts_value);

            foreach (var row in uts_results)
            {
                _userTableSchema.TryDeleteRow(row, transaction, transactionMode);
            }

            var uto_value = RowValueMaker.Create(_userObjects, uo.ObjectId, ut_results.First().GetValueInString(ut.UserObjectId), true);
            var uto_results = _userObjects.GetRowsWithValue(uto_value);

            foreach (var row in uto_results)
            {
                _userObjects.TryDeleteRow(row, transaction, transactionMode);
            }

            var uop_value = RowValueMaker.Create(_userObjectPermissions, uop.ObjectId, ut_results.First().GetValueInString(ut.UserObjectId), true);
            var uop_results = _userObjectPermissions.GetRowsWithValue(uop_value);

            foreach (var row in uop_results)
            {
                _userObjectPermissions.TryDeleteRow(row, transaction, transactionMode);
            }

        }

        public DatabaseSchemaInfo GetSchemaInfo(string schemaName)
        {
            if (HasDbSchema((schemaName)))
            {
                var schema = RowValueMaker.Create(_databaseSchemas, DatabaseSchemas.Columns.SchemaName, schemaName);

                int count = _databaseSchemas.CountOfRowsWithValue(schema);

                if (count > 0)
                {
                    var records = _databaseSchemas.GetRowsWithValue(schema);

                    foreach (var record in records)
                    {
                        var recordSchemaName = record.GetValueInString(DatabaseSchemas.Columns.SchemaName);
                        var recordSchemaGuid = record.GetValueInString(DatabaseSchemas.Columns.SchemaGUID);
                        return new DatabaseSchemaInfo(recordSchemaName, Guid.Parse(recordSchemaGuid));
                    }
                }

            }
            return null;
        }

        public bool HasDbSchema(string schemaName)
        {
            var schema = RowValueMaker.Create(_databaseSchemas, dbs.SchemaName, schemaName);

            return _databaseSchemas.HasValue(schema);
        }

        public bool TryAddDbSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode)
        {
            if (!HasDbSchema(schemaName))
            {
                var row = _databaseSchemas.GetNewLocalRow();

                row.SetValue(dbs.SchemaName, schemaName);
                row.SetValue(dbs.SchemaGUID, Guid.NewGuid().ToString());
                row.SetValueAsNullForColumn(dbs.ContractGUID);

                return _databaseSchemas.TryAddRow(row, request, transactionMode);
            }

            return false;
        }

        public bool TryDropDbSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode)
        {
            if (HasDbSchema(schemaName))
            {
                var schema = RowValueMaker.Create(_databaseSchemas, dbs.SchemaName, schemaName);
                List<IRow> rows = _databaseSchemas.GetRowsWithValue(schema);

                foreach (var row in rows)
                {
                    if (!_databaseSchemas.TryDeleteRow(row, request, transactionMode))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool HasUser(string userName)
        {
            var valueUserName = RowValueMaker.Create(_users, u.UserName, userName);

            return _users.HasValue(valueUserName);
        }

        public bool HasUser(string userName, Guid userId)
        {
            var values = new List<RowValue>();

            var valueUserName = RowValueMaker.Create(_users, u.UserName, userName);
            values.Add(valueUserName);

            var valueUserId = new RowValue();
            valueUserId.SetColumn(_users.Schema().Columns.Where(column => column.Name == u.UserGUID).FirstOrDefault());
            valueUserId.SetValue(userId.ToString());

            values.Add(valueUserId);

            return _users.HasAllValues(values);
        }

        /// <summary>
        /// Adds the schema to the sys tables and saves the result to disk
        /// </summary>
        /// <param name="schema">The schema of the table</param>
        /// <param name="tableObjectId">The new table object id</param>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool AddTable(ITableSchema schema, out Guid tableObjectId)
        {
            bool result = false;

            if (!HasTable(schema.Name))
            {
                tableObjectId = SetUserTableValue(schema);
                SetUserTableSchemaValues(schema);
                SetUserObjectTableValues(tableObjectId, schema.Name);
                result = true;
                return result;
            }

            tableObjectId = Guid.Empty;
            return result;
        }

        public void UpdateTableSchema(ITableSchema schema)
        {
            UpdateUserTable100(schema);
            UpdateUserTableSchemaValues100(schema);
        }

        public void UpdateTableSchema(ITableSchema schema, TransactionRequest transaction, TransactionMode transactionMode)
        {
            UpdateUserTable100(schema, transaction, transactionMode);
            UpdateUserTableSchemaValues100(schema, transaction, transactionMode);
        }

        /// <summary>
        /// Creates a user in the database and generates hash information.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="pwInput">The pw input.</param>
        /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
        /// <remarks><seealso cref="SystemDatabase.AddLogin(string, string, Guid)"/></remarks>
        public bool CreateUser(string userName, string pwInput)
        {
            int iterations = _crypt.GetRandomNumber();
            int length = _crypt.GetByteLength();

            var pwByte = Encoding.ASCII.GetBytes(pwInput);

            var salt = _crypt.GenerateSalt(length);
            var hash = _crypt.GenerateHash(pwByte, salt, iterations, length);

            var row = _users.GetNewLocalRow();
            row.SetValue(u.UserName, userName);
            row.SetValue(u.UserGUID, Guid.NewGuid().ToString());
            row.SetValue(u.Salt, salt);
            row.SetValue(u.ByteLength, length.ToString());
            row.SetValue(u.Hash, hash);
            row.SetValue(u.IsBanned, "false");
            row.SetValue(u.Workfactor, iterations.ToString());
            _users.TryAddRow(row);

            return true;

            /*
             * 
            //https://www.mking.net/blog/password-security-best-practices-with-examples-in-csharp
            Putting it all together:

            Store the following in your user database (alongside any additional data you need):

                Password salt
                Password hash
                Iterations / work factor

            When a user creates an account:

                Generate a new salt.
                Generate a hash using the generated salt and the provided password.
                Save the salt, hash, and work factor in the database.

            When a user tries to log in:

                Generate a hash using the provided password and the stored salt and work factor.
                If the hash generated above matches the stored hash, the password was correct; otherwise, the password was incorrect!

            If you want to increase the work factor at a later date, write a script that will run on user login to:

                Verify the user's password by comparing the hash generated using the provided password, the stored salt, and the stored work factor with the stored hash.
                Generate a new password hash using the provided password, the stored salt, and the new (increased) work factor, and store the new password hash and the new work factor in the database.

             */
        }

        public bool HasTable(string tableName)
        {
            bool result = false;

            switch (_version)
            {
                case Constants.DatabaseVersions.V100:
                    var searchItem = RowValueMaker.Create(_userTable, ut.TableName, tableName, true);
                    result = _userTable.HasValue(searchItem);

                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        public ITableSchema GetTable(string tableName, string dbName)
        {
            TableSchema schema = null;
            var columnSchema = new List<ColumnSchema>();

            var tableSearch = RowValueMaker.Create(_userTable, ut.TableName, tableName, true);
            if (_userTable.HasValue(tableSearch))
            {
                int count = _userTable.CountOfRowsWithValue(tableSearch);

                if (count == 1)
                {
                    var schemaRowData = _userTable.GetRowsWithValue(tableSearch);
                    var row = schemaRowData.First();

                    // need to parse table schema information
                    Guid schemaGuid = Guid.Empty;
                    schemaGuid = Guid.Parse(row.GetValueInString(ut.SchemaGUID));

                    DatabaseSchemaInfo schemaInfo = null;

                    // check to see if it's one of the defaults, otherwise we'll need to look it up
                    if (schemaGuid == Guid.Parse(Constants.SYS_SCHEMA_GUID))
                    {
                        schemaInfo = new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    }
                    else if (schemaGuid == Guid.Parse(Constants.DBO_SCHEMA_GUID))
                    {
                        schemaInfo = new DatabaseSchemaInfo(Constants.DBO_SCHEMA, Guid.Parse(Constants.DBO_SCHEMA_GUID));
                    }
                    else
                    {
                        var searchSchemaInfo = RowValueMaker.Create(_databaseSchemas, dbs.SchemaGUID, schemaGuid.ToString());

                        count = _databaseSchemas.CountOfRowsWithValue(searchSchemaInfo);

                        if (count == 1)
                        {
                            var schemaRows = _databaseSchemas.GetRowsWithValue(searchSchemaInfo);
                            var userDefinedSchema = schemaRows.First();
                            schemaInfo = new DatabaseSchemaInfo(userDefinedSchema.GetValueInString(dbs.SchemaName), Guid.Parse(userDefinedSchema.GetValueInString(dbs.SchemaGUID)));
                        }
                        else
                        {
                            throw new InvalidOperationException("Multiple schemas found");
                        }
                    }

                    // get the rest of the table schema information, see the "GetTables" function
                    string storedTableName = row.GetValueInString(ut.TableName).Trim();
                    int storedTableId = DbBinaryConvert.BinaryToInt(row.GetValueInByte(ut.TableId));

                    var searchItem = new RowValue();
                    searchItem.Column = UserTableSchema.GetColumns().Where(c => c.Name == uts.TableId).FirstOrDefault();
                    searchItem.SetValue(storedTableId.ToString());

                    var columnsForTable = _userTableSchema.GetRowsWithValue(searchItem);
                    foreach (var columns in columnsForTable)
                    {
                        var columnName = columns.GetValueInString(uts.ColumnName).Trim();
                        var columnType = columns.GetValueInString(uts.ColumnType).Trim();
                        var columnOrdinal = columns.GetValueInString(uts.ColumnOrdinal).Trim();
                        var columnLength = columns.GetValueInString(uts.ColumnLength).Trim();

                        var iColumnOrdinal = Convert.ToInt32(columnOrdinal);
                        var iColumnType = Convert.ToInt32(columnType);
                        var iColumnLength = Convert.ToInt32(columnLength);

                        SQLColumnType columnEnumType = (SQLColumnType)iColumnType;
                        ColumnSchema cs = null;

                        switch (columnEnumType)
                        {
                            case SQLColumnType.Int:
                                cs = new ColumnSchema(columnName, new SQLInt(), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            case SQLColumnType.Bit:
                                cs = new ColumnSchema(columnName, new SQLBit(), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            case SQLColumnType.Char:
                                cs = new ColumnSchema(columnName, new SQLChar(iColumnLength), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            case SQLColumnType.DateTime:
                                cs = new ColumnSchema(columnName, new SQLDateTime(), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            case SQLColumnType.Decimal:
                                cs = new ColumnSchema(columnName, new SQLDecimal(), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            case SQLColumnType.Varchar:
                                cs = new ColumnSchema(columnName, new SQLVarChar(iColumnLength), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            case SQLColumnType.Binary:
                                cs = new ColumnSchema(columnName, new SQLBinary(iColumnLength), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            case SQLColumnType.Varbinary:
                                cs = new ColumnSchema(columnName, new SQLVarbinary(iColumnLength), iColumnOrdinal);
                                columnSchema.Add(cs);
                                break;
                            default:
                                throw new InvalidOperationException("Unknown column type");
                        }
                    }

                    schema = new TableSchema(storedTableId, storedTableName, _dbId, columnSchema, schemaInfo);
                    schema.DatabaseName = dbName;

                }
                else
                {
                    throw new InvalidOperationException("Multiple tables found with the same name");
                }
            }

            return schema;
        }

        /// <summary>
        /// Returns the schemas for the user defined tables in the database. References <seealso cref="SystemDatabaseConstants100"/> on how to read the system tables.
        /// </summary>
        /// <returns>The schemas for the user defined tables in the database.</returns>
        /// <exception cref="InvalidOperationException">Unknown column type</exception>
        public TableSchema[] GetTables(string dbName)
        {
            var rows = _userTable.GetRows();
            var result = new TableSchema[rows.Count];
            int i = 0;

            foreach (var row in rows)
            {
                var table = _userTable.GetRow(row);

                int tableId = Convert.ToInt32(table.GetValueInString(ut.TableId));
                string tableName = table.GetValueInString(ut.TableName).Trim();

                Guid tableSchemaId = Guid.Empty;
                string tableSchemaName = string.Empty;

                var columnSchema = new List<ColumnSchema>();

                if (!table.IsValueNull(ut.SchemaGUID))
                {
                    tableSchemaId = Guid.Parse(table.GetValueInString(ut.SchemaGUID));

                    var recordSchemaName = RowValueMaker.Create(_databaseSchemas, DatabaseSchemas.Columns.SchemaGUID, table.GetValueInString(ut.SchemaGUID));

                    int count = _databaseSchemas.CountOfRowsWithValue(recordSchemaName);

                    if (count > 0)
                    {
                        var schemas = _databaseSchemas.GetRowsWithValue(recordSchemaName);

                        foreach (var schema in schemas)
                        {
                            var schemaName = schema.GetValueInString(DatabaseSchemas.Columns.SchemaName);
                        }
                    }
                    else
                    {
                        // see if it's a default schema
                        if (tableSchemaId == Guid.Parse(Constants.DBO_SCHEMA_GUID))
                        {
                            tableSchemaName = Constants.DBO_SCHEMA;
                        }

                        if (tableSchemaId == Guid.Parse(Constants.SYS_SCHEMA_GUID))
                        {
                            tableSchemaName = Constants.SYS_SCHEMA;
                        }
                    }
                }

                var searchItem = new RowValue();
                searchItem.Column = UserTableSchema.GetColumns().Where(c => c.Name == uts.TableId).FirstOrDefault();
                searchItem.SetValue(tableId.ToString());

                var columnsForTable = _userTableSchema.GetRowsWithValue(searchItem);
                foreach (var columns in columnsForTable)
                {
                    var columnName = columns.GetValueInString(uts.ColumnName).Trim();
                    var columnType = columns.GetValueInString(uts.ColumnType).Trim();
                    var columnOrdinal = columns.GetValueInString(uts.ColumnOrdinal).Trim();
                    var columnLength = columns.GetValueInString(uts.ColumnLength).Trim();

                    var iColumnOrdinal = Convert.ToInt32(columnOrdinal);
                    var iColumnType = Convert.ToInt32(columnType);
                    var iColumnLength = Convert.ToInt32(columnLength);

                    SQLColumnType columnEnumType = (SQLColumnType)iColumnType;
                    ColumnSchema cs = null;

                    switch (columnEnumType)
                    {
                        case SQLColumnType.Int:
                            cs = new ColumnSchema(columnName, new SQLInt(), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        case SQLColumnType.Bit:
                            cs = new ColumnSchema(columnName, new SQLBit(), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        case SQLColumnType.Char:
                            cs = new ColumnSchema(columnName, new SQLChar(iColumnLength), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        case SQLColumnType.DateTime:
                            cs = new ColumnSchema(columnName, new SQLDateTime(), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        case SQLColumnType.Decimal:
                            cs = new ColumnSchema(columnName, new SQLDecimal(), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        case SQLColumnType.Varchar:
                            cs = new ColumnSchema(columnName, new SQLVarChar(iColumnLength), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        case SQLColumnType.Binary:
                            cs = new ColumnSchema(columnName, new SQLBinary(iColumnLength), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        case SQLColumnType.Varbinary:
                            cs = new ColumnSchema(columnName, new SQLVarbinary(iColumnLength), iColumnOrdinal);
                            columnSchema.Add(cs);
                            break;
                        default:
                            throw new InvalidOperationException("Unknown column type");
                    }
                }

                TableSchema tableSchema = null;

                if (!string.IsNullOrEmpty(tableSchemaName))
                {
                    tableSchema = new TableSchema(tableId, tableName, _dbId, columnSchema, new DatabaseSchemaInfo(tableSchemaName, tableSchemaId));
                }
                else
                {
                    tableSchema = new TableSchema(tableId, tableName, _dbId, columnSchema);
                }
                tableSchema.DatabaseName = dbName;
                result[i] = tableSchema;
                i++;

            }

            return result;
        }

        /// <summary>
        /// Validates that the user exists in the database
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="pwInput">The pw of the user</param>
        /// <returns><c>TRUE</c> if the user exist, otherwise <c>FALSE</c></returns>
        /// <exception cref="InvalidOperationException">Thrown if somehow multiple users are found</exception>
        public bool ValidateUser(string userName, string pwInput)
        {
            bool result = false;

            var pwByte = Encoding.ASCII.GetBytes(pwInput);

            var valueUserName = RowValueMaker.Create(_users, SystemSchemaConstants100.Tables.Users.Columns.UserName, userName);

            int count = _users.CountOfRowsWithValue(valueUserName);

            if (count > 1)
            {
                throw new InvalidOperationException($"Muliple users found for user {userName}");
            }

            var rows = _users.GetRowsWithValue(valueUserName);

            foreach (var row in rows)
            {
                var hash = row.GetValueInByte(u.Hash);
                var salt = row.GetValueInByte(u.Salt);
                var iWorkFactor =
                    Convert.ToInt32(row.GetValueInString(u.Workfactor));
                var iByteLength =
                    Convert.ToInt32(row.GetValueInString(u.ByteLength));

                var computedHash = _crypt.GenerateHash(pwByte, salt, iWorkFactor, iByteLength);

                if (hash.SequenceEqual(computedHash))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks to see if the specified user has the specified permission in the database
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="pwInput">The pw of the user</param>
        /// <param name="permission">The database permisison to check for</param>
        /// <param name="objectId">The object to check if the user has permissions to</param>
        /// <returns><c>TRUE</c> if the user has rights, otherwise <c>FALSE</c></returns>
        public bool AuthorizeUser(string userName, string pwInput, DbPermission permission, Guid objectId)
        {
            if (ValidateUser(userName, pwInput))
            {
                var searchValue = RowValueMaker.Create(_userObjectPermissions, uop.UserName, userName);
                int count = _userObjectPermissions.CountOfRowsWithValue(searchValue);

                if (count > 0)
                {
                    var results = _userObjectPermissions.GetRowsWithValue(searchValue);

                    foreach (var row in results)
                    {
                        var guid = row.GetValueInString(uop.ObjectId);

                        if (string.Equals(guid, objectId.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var savedPermision = (DbPermission)DbBinaryConvert.BinaryToInt(row.GetValueInByteSpan(uop.DbPermission));
                            if (savedPermision == permission)
                            {
                                return true;
                            }

                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the specified user has the specified permission in the database
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="permission">The database permisison to check for</param>
        /// <param name="objectId">The object to check if the user has permissions to</param>
        /// <returns><c>TRUE</c> if the user has rights, otherwise <c>FALSE</c></returns>
        public bool AuthorizeUser(string userName, DbPermission permission, Guid objectId)
        {
            if (HasUser(userName))
            {
                var searchValue = RowValueMaker.Create(_userObjectPermissions, uop.UserName, userName);

                int count = _userObjectPermissions.CountOfRowsWithValue(searchValue);

                if (count > 0)
                {
                    var results = _userObjectPermissions.GetRowsWithValue(searchValue);
                    foreach (var row in results)
                    {
                        var guid = row.GetValueInString(uop.ObjectId);

                        if (string.Equals(guid, objectId.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var savedPermision = (DbPermission)DbBinaryConvert.BinaryToInt(row.GetValueInByteSpan(uop.DbPermission));
                            if (savedPermision == permission)
                            {
                                return true;
                            }

                        }
                    }
                }
            }

            return false;
        }

        public Guid GetTableObjectId(string tableName)
        {
            var sv1 = RowValueMaker.Create(_userObjects, uo.ObjectName, tableName, true);
            var sv2 = RowValueMaker.Create(_userObjects, uo.ObjectType, Convert.ToInt32(ObjectType.Table).ToString());

            RowValue[] searchItems = new RowValue[2];
            searchItems[0] = sv1;
            searchItems[1] = sv2;

            int count = _userObjects.CountOfRowsWithAllValues(searchItems);

            if (count > 0)
            {
                if (count == 1)
                {
                    var result = _userObjects.GetRowsWithAllValues(searchItems);

                    foreach (var row in result)
                    {
                        var resultTableName = row.GetValueInString(uo.ObjectName).Trim();
                        if (string.Equals(resultTableName, tableName, StringComparison.OrdinalIgnoreCase))
                        {
                            var resultObjectId = row.GetValueInString(uo.ObjectId);
                            return Guid.Parse(resultObjectId);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("There exists multiple tables with the same name somehow");
                }
            }

            return Guid.Empty;
        }

        public bool HasObject(Guid objectId)
        {
            var searchValue = RowValueMaker.Create(_userObjects, uo.ObjectId, objectId.ToString());

            int count = _userObjects.CountOfRowsWithValue(searchValue);

            if (count > 0)
            {
                var results = _userObjects.GetRowsWithValue(searchValue);

                foreach (var row in results)
                {
                    var guid = row.GetValueInString(uo.ObjectId);
                    if (string.Equals(guid, objectId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool GrantUserPermissionToObject(string userName, DbPermission permission, Guid objectId)
        {
            if (HasUser(userName))
            {
                if (HasObject(objectId))
                {
                    if (!AuthorizeUser(userName, permission, objectId))
                    {
                        var row = _userObjectPermissions.GetNewLocalRow();
                        row.SetValue(uop.UserName, userName);
                        row.SetValue(uop.ObjectId, objectId.ToString());
                        row.SetValue(uop.DbPermission, Convert.ToInt32(permission).ToString());

                        // need to look this up.
                        row.SetValue(uop.UserGUID, string.Empty);

                        _userObjectPermissions.TryAddRow(row);

                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Private Methods
        private void AddSystemTables()
        {
            _systemTables.Clear();

            _systemTables.Add(_userTable);
            _systemTables.Add(_userTableSchema);
            _systemTables.Add(_userObjects);
            _systemTables.Add(_users);
            _systemTables.Add(_userObjectPermissions);
            _systemTables.Add(_databaseSchemas);
            _systemTables.Add(_databaseSchemaPermissions);
            _systemTables.Add(_participants);
            _systemTables.Add(_databaseContracts);
        }
        private void SetupDatabaseSchemas()
        {
            _databaseSchemas = new Table(DatabaseSchemas.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
            _databaseSchemaPermissions = new Table(DatabaseSchemaPermissions.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }

        private void SetupParticipantTable()
        {
            _participants = new Table(Participants.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }

        private void SetupDatabaseContracts()
        {
            _databaseContracts = new Table(DatabaseContracts.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }

       

        private void SetupUserTablePermissions()
        {
            _userObjectPermissions = new Table(UserObjectPermissions.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }
        private void SetupUserObjects()
        {
            _userObjects = new Table(UserObjects.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }
        private void SetupUserTable()
        {
            _userTable = new Table(UserTable.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }

        private void SetupUserTableSchema()
        {
            _userTableSchema = new Table(UserTableSchema.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }

        private void SetUserTable()
        {
            _users = new Table(Users.Schema(_dbId, _dbName), _cache, _storage, _xEntryManager);
        }

        /// <summary>
        /// Saves the table information to the UserTable table and returns the new table's ObjectId
        /// </summary>
        /// <param name="schema">The schema of the newly created table.</param>
        /// <returns>The new table's ObjectId (a GUID)</returns>
        /// <exception cref="UnknownDbVersionException"></exception>
        private Guid SetUserTableValue(ITableSchema schema)
        {
            Guid tableObjectId = Guid.Empty;

            switch (_version)
            {
                case Constants.DatabaseVersions.V100:
                    tableObjectId = SetUserTableValue100(schema);
                    break;
                default:
                    throw new UnknownDbVersionException(_version);
            }

            return tableObjectId;
        }

        private void UpdateUserTable100(ITableSchema schema)
        {
            Row row = null;

            var search = RowValueMaker.Create(_userTable, ut.TableName, schema.Name, true);

            int count = _userTable.CountOfRowsWithValue(search);

            var searchResult = _userTable.GetRowsWithValue(search);
            if (count > 1)
            {
                throw new InvalidOperationException("Multiple tables found with same name");
            }

            row = searchResult.First() as Row;

            row.SetValue(ut.TableId, schema.Id.ToString());
            row.SetValue(ut.TableName, schema.Name);
            row.SetValue(ut.TotalRows, 0.ToString());
            row.SetValue(ut.TotalLogicalRows, 0.ToString());
            row.SetValue(ut.IsDeleted, false.ToString());
            row.SetValue(ut.UserObjectId, schema.ObjectId.ToString());

            if (schema.Columns.Any(col => col.Name == ut.ContractGUID))
            {
                row.SetValue(ut.ContractGUID, schema.ContractGUID.ToString());
            }
            else
            {
                row.SetValueAsNullForColumn(ut.ContractGUID);
            }

            row.SetValue(ut.LogicalStoragePolicy, Convert.ToInt32(schema.StoragePolicy).ToString());

            if (schema.Schema is null)
            {
                row.SetValueAsNullForColumn(ut.SchemaGUID);
            }
            else
            {
                row.SetValue(ut.SchemaGUID, schema.Schema.SchemaGUID.ToString());
            }

            _userTable.TryUpdateRow(row);
        }

        private void UpdateUserTable100(ITableSchema schema, TransactionRequest transaction, TransactionMode transactionMode)
        {
            Row row = null;

            var search = RowValueMaker.Create(_userTable, ut.TableName, schema.Name, true);

            int count = _userTable.CountOfRowsWithValue(search);

            var searchResult = _userTable.GetRowsWithValue(search);
            if (count > 1)
            {
                throw new InvalidOperationException("Multiple tables found with same name");
            }

            row = searchResult.First() as Row;

            row.SetValue(ut.TableId, schema.Id.ToString());
            row.SetValue(ut.TableName, schema.Name);
            row.SetValue(ut.TotalRows, 0.ToString());
            row.SetValue(ut.TotalLogicalRows, 0.ToString());
            row.SetValue(ut.IsDeleted, false.ToString());
            row.SetValue(ut.UserObjectId, schema.ObjectId.ToString());

            if (schema.Columns.Any(col => col.Name == ut.ContractGUID))
            {
                row.SetValue(ut.ContractGUID, schema.ContractGUID.ToString());
            }
            else
            {
                row.SetValueAsNullForColumn(ut.ContractGUID);
            }

            row.SetValue(ut.LogicalStoragePolicy, Convert.ToInt32(schema.StoragePolicy).ToString());

            if (schema.Schema is null)
            {
                row.SetValueAsNullForColumn(ut.SchemaGUID);
            }
            else
            {
                row.SetValue(ut.SchemaGUID, schema.Schema.SchemaGUID.ToString());
            }

            _userTable.TryUpdateRow(row, transaction, transactionMode);
        }

        private Guid SetUserTableValue100(ITableSchema schema)
        {
            var tableObjectId = Guid.NewGuid();

            var row = _userTable.GetNewLocalRow();
            row.SetValue(ut.TableId, schema.Id.ToString());
            row.SetValue(ut.TableName, schema.Name);
            row.SetValue(ut.TotalRows, 0.ToString());
            row.SetValue(ut.TotalLogicalRows, 0.ToString());
            row.SetValue(ut.IsDeleted, false.ToString());
            row.SetValue(ut.UserObjectId, tableObjectId.ToString());
            row.SetValueAsNullForColumn(ut.ContractGUID);
            row.SetValue(ut.LogicalStoragePolicy, Convert.ToInt32(schema.StoragePolicy).ToString());

            if (schema.Schema is null)
            {
                row.SetValueAsNullForColumn(ut.SchemaGUID);
            }
            else
            {
                row.SetValue(ut.SchemaGUID, schema.Schema.SchemaGUID.ToString());
            }

            _userTable.TryAddRow(row);


            return tableObjectId;
        }

        private void SetUserTableSchemaValues(ITableSchema schema)
        {
            switch (_version)
            {
                case Constants.DatabaseVersions.V100:
                    SetUserTableSchemaValues100(schema);
                    break;
                default:
                    throw new UnknownDbVersionException(_version);
            }
        }

        private void SetUserObjectTableValues(Guid tableObjectId, string objectName)
        {
            switch (_version)
            {
                case Constants.DatabaseVersions.V100:
                    SetUserObectTableValues100(tableObjectId, objectName);
                    break;
                default:
                    throw new UnknownDbVersionException(_version);
            }
        }

        private void SetUserObectTableValues100(Guid tableObjectId, string objectName)
        {
            int enumType = (int)ObjectType.Table;
            var row = _userObjects.GetNewLocalRow();
            row.SetValue(uo.ObjectId, tableObjectId.ToString());
            row.SetValue(uo.ObjectType, enumType.ToString());
            row.SetValueAsNullForColumn(uo.ContractGUID);

            var name = objectName.PadRight(Constants.FIXED_LENGTH_OF_OBJECT_NAME);

            row.SetValue(uo.ObjectName, name);
            _userObjects.TryAddRow(row);
        }

        private void SetUserTableSchemaValues100(ITableSchema schema)
        {
            string tableId = schema.Id.ToString();
            foreach (var column in schema.Columns)
            {
                var row = _userTableSchema.GetNewLocalRow();
                row.SetValue(uts.TableId, tableId);
                row.SetValue(uts.ColumnId, column.Ordinal.ToString());
                row.SetValue(uts.ColumnName, column.Name);
                row.SetValue(uts.ColumnType, GetColumnType(column.DataType).ToString());
                row.SetValue(uts.ColumnLength, column.Length.ToString());
                row.SetValue(uts.ColumnOrdinal, column.Ordinal.ToString());
                row.SetValue(uts.ColumnIsNullable, column.IsNullable.ToString());
                row.SetValue(uts.ColumnBinaryOrder, column.Id.ToString());
                row.SetValue(uts.UserObjectId, Guid.NewGuid().ToString());
                row.SetValueAsNullForColumn(uts.ContractGUID);

                _userTableSchema.TryAddRow(row);
            }
        }

        private void UpdateUserTableSchemaValues100(ITableSchema schema)
        {
            string tableId = schema.Id.ToString();

            var search = RowValueMaker.Create(_userTableSchema, uts.TableId, schema.Id.ToString());

            int count = _userTableSchema.CountOfRowsWithValue(search);

            // if we haven't added any columns to the table
            if (schema.Columns.Count() == count)
            {
                var results = _userTableSchema.GetRowsWithValue(search);
                foreach (var value in results)
                {
                    int iterValue = DbBinaryConvert.BinaryToInt(value.GetValueInByte(uts.TableId));
                    // make sure we're updating a column of the table we're sending
                    if (iterValue == schema.Id)
                    {
                        foreach (var column in schema.Columns)
                        {
                            var iterColId = DbBinaryConvert.BinaryToInt(value.GetValueInByte(uts.ColumnId));
                            if (iterColId == column.Id)
                            {
                                value.SetValue(uts.ColumnName, column.Name);
                                value.SetValue(uts.ColumnType, GetColumnType(column.DataType).ToString());
                                value.SetValue(uts.ColumnLength, column.Length.ToString());
                                value.SetValue(uts.ColumnOrdinal, column.Ordinal.ToString());
                                value.SetValue(uts.ColumnIsNullable, column.IsNullable.ToString());
                                value.SetValue(uts.ColumnBinaryOrder, column.Id.ToString());
                                value.SetValue(uts.UserObjectId, Guid.NewGuid().ToString());
                                value.SetValueAsNullForColumn(uts.ContractGUID);

                                _userTableSchema.TryUpdateRow(value);
                            }
                        }
                    }
                }
            }
            // we've added a column to the table
            else if (schema.Columns.Count() > count)
            {
                // we need to figure out what to do here, because then we're going to need to change 
                // all page structures in cache and on disk
                throw new NotImplementedException();
            }
            // we'ev removed a column from the table
            else if (schema.Columns.Count() < count)
            {
                // we need to figure out what to do here, because then we're going to need to change 
                // all page structures in cache and on disk
                throw new NotImplementedException();
            }
        }

        private void UpdateUserTableSchemaValues100(ITableSchema schema, TransactionRequest transaction, TransactionMode transactionMode)
        {
            string tableId = schema.Id.ToString();

            var search = RowValueMaker.Create(_userTableSchema, uts.TableId, schema.Id.ToString());

            int count = _userTableSchema.CountOfRowsWithValue(search);

            // if we haven't added any columns to the table
            if (schema.Columns.Count() == count)
            {
                var results = _userTableSchema.GetRowsWithValue(search);
                foreach (var value in results)
                {
                    int iterValue = DbBinaryConvert.BinaryToInt(value.GetValueInByte(uts.TableId));
                    // make sure we're updating a column of the table we're sending
                    if (iterValue == schema.Id)
                    {
                        foreach (var column in schema.Columns)
                        {
                            var iterColId = DbBinaryConvert.BinaryToInt(value.GetValueInByte(uts.ColumnId));
                            if (iterColId == column.Id)
                            {
                                value.SetValue(uts.ColumnName, column.Name);
                                value.SetValue(uts.ColumnType, GetColumnType(column.DataType).ToString());
                                value.SetValue(uts.ColumnLength, column.Length.ToString());
                                value.SetValue(uts.ColumnOrdinal, column.Ordinal.ToString());
                                value.SetValue(uts.ColumnIsNullable, column.IsNullable.ToString());
                                value.SetValue(uts.ColumnBinaryOrder, column.Id.ToString());
                                value.SetValue(uts.UserObjectId, Guid.NewGuid().ToString());
                                value.SetValueAsNullForColumn(uts.ContractGUID);

                                _userTableSchema.TryUpdateRow(value, transaction, transactionMode);
                            }
                        }
                    }
                }
            }
            // we've added a column to the table
            else if (schema.Columns.Count() > count)
            {
                // we need to figure out what to do here, because then we're going to need to change 
                // all page structures in cache and on disk
                throw new NotImplementedException();
            }
            // we'ev removed a column from the table
            else if (schema.Columns.Count() < count)
            {
                // we need to figure out what to do here, because then we're going to need to change 
                // all page structures in cache and on disk
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the integer enum value from <seealso cref="SQLColumnType"/>
        /// </summary>
        /// <param name="type">The SQL type.</param>
        /// <returns>An integer enum value that represents <seealso cref="SQLColumnType"/> from <seealso cref="ISQLType"/></returns>
        /// <exception cref="UnknownDbVersionException"></exception>
        private int GetColumnType(ISQLType type)
        {
            int enumtype;

            switch (_version)
            {
                case Constants.DatabaseVersions.V100:
                    enumtype = GetColumnType100(type);
                    break;
                default:
                    throw new UnknownDbVersionException(_version);
            }

            return enumtype;
        }

        private int GetColumnType100(ISQLType type)
        {
            return SQLColumnTypeConverter.ConvertToInt(type, Constants.DatabaseVersions.V100);
        }
        #endregion
    }
}
