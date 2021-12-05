using Drummersoft.DrummerDB.Core.Cryptography.Interface;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Databases.Remote;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Interface;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases
{
    /// <summary>
    /// An abstraction over metadata information in the database, i.e. User information, System Data Pages, etc.
    /// </summary>
    internal class DatabaseMetadata
    {
        #region Private Fields
        // TODO - the metadata objects should talk to cache to get things they need, example system data tables should talk to cache to get data
        // from system data pages, etc.
        private TableSchemaCollection _tables;
        private DbMetaSystemDataPages _systemDataPages;
        private DbMetaSystemPage _systemPage;
        private string _dbName;
        private readonly Guid _dbId;
        private int _version;
        private ITransactionEntryManager _xEntryManager;
        #endregion

        #region Public Properties
        /// <summary>
        /// The name of the database
        /// </summary>
        public string Name => GetDatabaseName();
        public readonly ICryptoManager CryptoManager;
        public readonly ICacheManager CacheManager;
        public readonly IDbManager DbManager;
        public readonly IStorageManager StorageManager;

        // is this ever set?
        public readonly RemoteDataManager RemoteDataManager;

        public ITransactionEntryManager TransactionEntryManager => _xEntryManager;

        /// <summary>
        /// The users of the database
        /// </summary>
        internal IUser[] Users { get; set; }

        /// <summary>
        /// The database id
        /// </summary>
        public Guid Id => _dbId;

        /// <summary>
        /// The participants of the database
        /// </summary>
        internal Participant[] Participants { get; set; }

        /// <summary>
        /// The database contract
        /// </summary>
        internal IContract Contract { get; set; }

        /// <summary>
        /// The database version
        /// </summary>
        public int Version => _version;

        /// <summary>
        /// The date the database was created
        /// </summary>
        internal DateTime CreatedDate { get; set; }
        #endregion

        #region Constructors
        public DatabaseMetadata(
            ISystemPage page, 
            ICacheManager cache, 
            ICryptoManager crypt, 
            IDbManager dbManager, 
            IStorageManager storage, 
            ITransactionEntryManager xEntryManager,
            RemoteDataManager remote)
        {
            _xEntryManager = xEntryManager;

            CacheManager = cache;
            CryptoManager = crypt;
            DbManager = dbManager;
            StorageManager = storage;

            _version = page.DatabaseVersion;
            _dbName = page.DatabaseName;
            _dbId = page.DatabaseId;
            RemoteDataManager = remote;

            _systemDataPages = new DbMetaSystemDataPages(cache, _dbId, _version, crypt, storage, _xEntryManager, _dbName);
            _systemPage = new DbMetaSystemPage(cache, _dbId, _version);

            var userTables = _systemDataPages.GetTables(page.DatabaseName);
            var systemTables = _systemDataPages.SystemTables;

            _tables = new TableSchemaCollection();

            foreach (var table in userTables)
            {
                _tables.Add(table);
            }

            foreach (var table in systemTables)
            {
                _tables.Add(table.Schema());
            }
        }

        public DatabaseMetadata(ICacheManager cache, 
            Guid dbId, 
            int version, 
            ICryptoManager crypt, 
            IStorageManager storage, 
            string dbName,
            RemoteDataManager remote)
        {
            CryptoManager = crypt;
            CacheManager = cache;
            StorageManager = storage;
            RemoteDataManager = remote;

            _systemDataPages = new DbMetaSystemDataPages(cache, dbId, version, crypt, storage, _xEntryManager, dbName);
            _systemPage = new DbMetaSystemPage(cache, dbId, version);

            _dbId = dbId;
            _version = version;
            _dbName = dbName;

            _tables = new TableSchemaCollection();

            var userTables = _systemDataPages.GetTables(_dbName);

            foreach (var table in userTables)
            {
                _tables.Add(table);
            }

            var systemTables = _systemDataPages.SystemTables;

            foreach (var table in systemTables)
            {
                _tables.Add(table.Schema());
            }

            if (!cache.UserSystemCacheHasDatabase(dbId))
            {
                var systemPage = storage.GetSystemPage(dbId);
                cache.AddUserDbSystemPage(systemPage);
            }

            // we want to get most objects from Cache, and we want this class DatabaseMetadata to be an abstraction/collection over the meta data (not User data) in the database.
            // in other words, this object is a collection of sub-objects that interface with various metadata about the database (Contracts, Users, Tables, Participants, etc.)
        }

        public DatabaseMetadata(ICacheManager cache, Guid dbId, int version, ICryptoManager crypt, string dbName, IStorageManager storage, ITransactionEntryManager xEntryManager)
        {
            CryptoManager = crypt;
            CacheManager = cache;
            StorageManager = storage;
            _xEntryManager = xEntryManager;

            _systemDataPages = new DbMetaSystemDataPages(cache, dbId, version, crypt, storage, xEntryManager, dbName);
            _systemPage = new DbMetaSystemPage(cache, dbId, version);

            _dbId = dbId;
            _version = version;

            _tables = new TableSchemaCollection();

            var userTables = _systemDataPages.GetTables(dbName);
            var systemTables = _systemDataPages.SystemTables;

            foreach (var table in userTables)
            {
                _tables.Add(table);
            }

            foreach (var table in systemTables)
            {
                _tables.Add(table.Schema());
            }

            _dbName = dbName;

            // we want to get most objects from Cache, and we want this class DatabaseMetadata to be an abstraction/collection over the meta data (not User data) in the database.
            // in other words, this object is a collection of sub-objects that interface with various metadata about the database (Contracts, Users, Tables, Participants, etc.)
        }
        #endregion

        #region Public Methods
        public List<ITableSchema> GetCopyOfUserTables()
        {
            int totalUserTables = 0;
            foreach (var table in _tables)
            {
                if (string.Equals(table.Schema.SchemaName, Constants.SYS_SCHEMA, StringComparison.OrdinalIgnoreCase))
                {
                    totalUserTables++;
                }
            }

            var result = new List<ITableSchema>(totalUserTables);

            foreach (var table in _tables)
            {
                if (!string.Equals(table.Schema.SchemaName, Constants.SYS_SCHEMA, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(table);
                }
            }

            return result;
        }


        public TableSchema GetTableSchema(string tableName)
        {
            foreach (var item in _tables)
            {
                if (string.Equals(tableName, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(item.DatabaseName))
                    {
                        item.DatabaseName = GetDatabaseName();
                    }

                    return item;
                }
            }

            return null;
        }

        public TableSchema GetTableSchema(int tableId)
        {
            foreach (var item in _tables)
            {
                if (item.Address.TableId == tableId)
                {
                    return item;
                }
            }

            return null;
        }

        public int GetMaxTableId()
        {
            int maxId = 0;
            foreach (var table in _tables)
            {
                if (table.Id > maxId)
                {
                    maxId = table.Id;
                }
            }

            return maxId;
        }

        public bool HasTable(int tableId)
        {
            foreach (var item in _tables)
            {
                if (item.Address.TableId == tableId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasTable(string tableName, string schemaName)
        {
            foreach (var table in _tables)
            {
                if (table.Schema is not null)
                {
                    if (string.Equals(tableName, table.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(schemaName, table.Schema.SchemaName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public List<TableSchema> Schemas()
        {
            return _tables.GetAll();
        }

        public DatabaseSchemaInfo GetSchemaInfo(string schemaName)
        {
            if (HasSchema(schemaName))
            {
                return _systemDataPages.GetSchemaInfo(schemaName);
            }

            return null;
        }
        public bool HasSchema(string schemaName)
        {
            return _systemDataPages.HasDbSchema(schemaName);
        }

        public bool CreateSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode)
        {
            return _systemDataPages.TryAddDbSchema(schemaName, request, transactionMode);
        }

        public Guid GetTableObjectId(string tableName)
        {
            if (_systemDataPages.HasTable(tableName))
            {
                return _systemDataPages.GetTableObjectId(tableName);
            }

            return Guid.Empty;
        }

        public bool CreateUser(string userName, string pwInput)
        {
            return _systemDataPages.CreateUser(userName, pwInput);
        }

        public bool HasUser(string userName)
        {
            return _systemDataPages.HasUser(userName);
        }

        public bool HasTable(string tableName)
        {
            return _systemDataPages.HasTable(tableName);
        }

        public bool HasUser(string userName, Guid userId)
        {
            return _systemDataPages.HasUser(userName, userId);
        }

        public ITableSchema GetSchema(string tableName, string dbName)
        {
            if (_systemDataPages.HasTable(tableName))
            {
                return _systemDataPages.GetTable(tableName, dbName);
            }

            return null;
        }

        public bool UpdateTableSchema(ITableSchema schema)
        {
            if (_systemDataPages.HasTable(schema.Name))
            {
                _systemDataPages.UpdateTableSchema(schema);
                return true;
            }

            return false;
        }

        public bool UpdateTableSchema(ITableSchema schema, TransactionRequest transaction, TransactionMode transactionMode)
        {
            if (_systemDataPages.HasTable(schema.Name))
            {
                _systemDataPages.UpdateTableSchema(schema, transaction, transactionMode);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the specified table to the metadata's collection of tables
        /// </summary>
        /// <param name="schema">The schema of the table to add</param>
        /// <remarks>Note that adding the table to this object is only to maintain what is presented to other databases. 
        /// To actually add the table to Cache and Storage, use <seealso cref="IDbMetaSystemDataPages.AddTable(ITableSchema)"/></remarks>
        public bool AddTable(TableSchema schema, out Guid tableObjectId)
        {
            bool result = false;

            if (!_systemDataPages.HasTable(schema.Name))
            {
                _systemDataPages.AddTable(schema, out tableObjectId);
                _tables.Add(schema);
                result = true;
                return result;
            }

            tableObjectId = Guid.Empty;
            return result;
        }


        /// <summary>
        /// Marks the pages for the table in memory as deleted and saves those pages to disk. 
        /// Then unloads those pages from Cache. 
        /// Then removes the table references from the metadata tables.
        /// Also removes the schema from the metadata collection.
        /// </summary>
        /// <param name="tableName">The name of the table to delete</param>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool DropTable(string tableName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            var table = _tables.Get(tableName);
            var pageAddresses = CacheManager.GetPageAddressesForTree(table.Address);

            foreach (var address in pageAddresses)
            {
                var page = CacheManager.UserDataGetPage(address);
                page.Delete();
                StorageManager.SavePageDataToDisk(address, page.Data, page.Type, page.DataPageType(), page.IsDeleted());
            }

            _systemDataPages.DropTable(tableName, transaction, transactionMode);

            CacheManager.TryRemoveTree(table.Address);
            _tables.Remove(tableName);

            // is this it?
            return true;
        }

        /// <summary>
        /// Validates that the user exists in the database
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="pwInput">The pw of the user</param>
        /// <returns><c>TRUE</c> if the user exists, otherwise <c>FALSE</c></returns>
        public bool ValidateUser(string userName, string pwInput)
        {
            return _systemDataPages.ValidateUser(userName, pwInput);
        }

        /// <summary>
        /// Authorizes the user to perform the specified action against the specified object in the database.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="pwInput">The pw input.</param>
        /// <param name="permission">The permission.</param>
        /// <param name="objectId">The object identifier.</param>
        /// <returns>True if the user has permissions to the specified object, otherwise false</returns>
        /// <remarks>This function assumes that we have already authorized the specified user to access the database Process itself. We shouldn't have to 
        /// re-authorize the user as a valid login to the system.</remarks>
        public bool AuthorizeUser(string userName, string pwInput, DbPermission permission, Guid objectId)
        {
            bool result = false;

            if (ValidateUser(userName, pwInput))
            {
                result = _systemDataPages.AuthorizeUser(userName, pwInput, permission, objectId);
            }

            // this should only check what the database knows if the user has permission or not. This should be called by AuthenticationMAnager (which has a dependency on 
            // database manager only after it's checked if the calling user has system level permissions
            return result;
        }

        public bool GrantUserPermission(string userName, DbPermission permission, Guid objectId)
        {
            if (_systemDataPages.HasObject(objectId))
            {
                return _systemDataPages.GrantUserPermissionToObject(userName, permission, objectId);
            }

            return false;
        }

        #endregion

        #region Private Methods
        private string GetDatabaseName()
        {
            if (_dbName is null)
            {
                _dbName = _systemPage.GetDatabaseName(Id);
            }

            return _dbName;
        }
        #endregion
    }
}
