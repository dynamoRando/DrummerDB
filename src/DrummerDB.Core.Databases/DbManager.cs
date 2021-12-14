using Drummersoft.DrummerDB.Core.Cryptography.Interface;
using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Factory;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Databases.Remote;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Databases
{
    /// <summary>
    /// Manages database actions for a Process
    /// </summary>
    internal class DbManager : IDbManager, IDbManagerInformation
    {
        #region Private Fields
        private ICacheManager _cache;

        private ICryptoManager _crypt;

        private HostInfo _hostInfo;

        private LogService _log;

        // settings
        private ProcessUserDatabaseSettings _settings;

        // managers
        private IStorageManager _storage;
        private SystemDatabaseCollection _systemDatabases;
        // internal objects
        private UserDatabaseCollection _userDatabases;

        private ITransactionEntryManager _xEntryManager;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        internal DbManager() { }

        /// <summary>
        /// Constructs an instance of a DbManager and loads Dbs into memory
        /// </summary>
        /// <param name="cache">A cache manager for managing objects in memory</param>
        /// <param name="crypt">A crypt manager for manging security actions</param>
        internal DbManager(ITransactionEntryManager xEntryManager)
        {
            _xEntryManager = xEntryManager;
        }


        internal DbManager(ITransactionEntryManager xEntryManager, LogService log)
        {
            _xEntryManager = xEntryManager;
            _log = log;
        }

        internal DbManager(IStorageManager storage, ICacheManager cache, ICryptoManager crypt, ITransactionEntryManager xEntryManager) : this(xEntryManager)
        {
            _storage = storage;
            _cache = cache;
            _crypt = crypt;
        }


        internal DbManager(IStorageManager storage, ICacheManager cache, ICryptoManager crypt, ITransactionEntryManager xEntryManager, LogService log) : this(xEntryManager, log)
        {
            _storage = storage;
            _cache = cache;
            _crypt = crypt;
        }

        #endregion

        #region Public Methods    
        internal void SetHostInfo(HostInfo hostInfo)
        {
            _hostInfo = hostInfo;
        }

        internal bool CreateAdminLogin(string userName, string pwInput, Guid userGUID)
        {
            bool result = false;

            SystemDatabase db = GetGuSystemDatabase();
            if (!db.HasLogin(userName, userGUID))
            {
                result = db.AddLogin(userName, pwInput, userGUID, true);
                db.AssignUserToDefaultSystemAdmin(userName);

                if (_log is not null)
                {
                    _log.Info($"Created login {userName}");
                }
            }

            return result;
        }

        internal bool CreateLogin(string userName, string pwInput, Guid userGUID)
        {
            SystemDatabase db = GetGuSystemDatabase();
            return db.AddLogin(userName, pwInput, userGUID, false);
        }

        internal bool DeletePartDatabase(string dbName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the specified database from the <see cref="UserDatabaseCollection"/> and removes the file from disk
        /// </summary>
        /// <param name="dbName">The name of the db to remove</param>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        internal bool DeleteHostDatabase(string dbName)
        {
            var result = _storage.DeleteHostDatabase(dbName);
            _userDatabases.Remove(dbName);
            return result;
        }

        /// <summary>
        /// Checks the <see cref="_systemDatabases"/> collection to see if the specified system db name exists
        /// </summary>
        /// <param name="name">The systme db name to check</param>
        /// <returns><c>true</c> if it exists in the collection, otherwise <c>false</c></returns>
        internal bool HasSystemDatabaseInCollection(string name)
        {
            // occurs if we have not called LoadSystemDatabses(), normally happens during testing
            if (_systemDatabases is null)
            {
                _systemDatabases = new SystemDatabaseCollection();
                return false;
            }

            return _systemDatabases.Contains(name);
        }

        /// <summary>
        /// Attempts to load the <see cref="SystemDatabase"/>s into <see cref="_systemDatabases"/> collection from disk. If there are no system dbs on disk it will create one. Also populates <see cref="IStorageManager"/> and 
        /// <see cref="ICacheManager"/> with the appropriate structures as it loads into memory.
        /// </summary>
        /// <param name="cache">A reference to cache for each system database</param>
        /// <param name="storage">A reference to storage for each system database</param>
        /// <param name="crypt">A reference to crypt for each system database</param>
        internal void LoadSystemDatabases(ICacheManager cache, IStorageManager storage, ICryptoManager crypt, HostInfo hostInfo)
        {
            // set the managers if they have not already been set
            if (_storage is null)
            {
                _storage = storage;
            }

            if (_crypt is null)
            {
                _crypt = crypt;
            }

            if (_cache is null)
            {
                _cache = cache;
            }

            int sysDbCount = _storage.TotalSystemDatabasesOnDisk();

            if (_systemDatabases is null)
            {
                if (sysDbCount > 0)
                {
                    _systemDatabases = new SystemDatabaseCollection(sysDbCount);
                }
                else
                {
                    _systemDatabases = new SystemDatabaseCollection();
                }
            }

            // if we have no system databases, create one
            if (sysDbCount == 0)
            {
                _systemDatabases.Add(CreateSystemDatabaseOnDisk(storage, crypt, cache, _xEntryManager));
            }
            else
            {
                // load the system databases into memory
                var systemDbs = _storage.GetSystemDatabaseNames();

                foreach (var dbName in systemDbs)
                {
                    if (!HasSystemDatabaseInCollection(dbName))
                    {
                        var systemPage = _storage.GetSystemPageForSystemDatabase(dbName);
                        _cache.AddSystemDbSystemPage(systemPage);
                        var metaData = new DatabaseMetadata(systemPage, _cache, _crypt, this, _storage, _xEntryManager, new RemoteDataManager(hostInfo));
                        var system = new SystemDatabase(metaData, _log);

                        _systemDatabases.Add(system);
                    }
                }
            }

            if (_log is not null)
            {
                _log.Info($"System databases loaded. Total System Database count {SystemDatabaseCount().ToString()}");
            }
        }

        internal void LoadSystemDatabaseTableWithActiveDbs()
        {
            var db = GetGuSystemDatabase();
            db.LoadDbTableWithDbNames(_userDatabases);
        }

        /// <summary>
        /// Loads database references into memory
        /// </summary>
        internal void LoadUserDatabases(ICacheManager cache, IStorageManager storage, ICryptoManager crypt, HostInfo hostInfo)
        {
            _hostInfo = hostInfo;

            // set the managers if they have not already been set
            if (_storage is null)
            {
                _storage = storage;
            }

            if (_crypt is null)
            {
                _crypt = crypt;
            }

            if (_cache is null)
            {
                _cache = cache;
            }

            if (_userDatabases is null)
            {
                _userDatabases = new UserDatabaseCollection(_storage.TotalUserDatabasesOnDisk());
            }

            _storage.LoadUserDatabaseFilesIntoMemory();

            var userDbMeta = _storage.GetUserDatabasesInformation();

            foreach (var meta in userDbMeta)
            {
                var remote = new RemoteDataManager(hostInfo);
                var metadata = new DatabaseMetadata(_cache, meta.DatabaseId, meta.DatabaseVersion, _crypt, _storage, meta.DatabaseName, remote);
                var hostDb = new HostDb(metadata, _xEntryManager, _log);
                _userDatabases.Add(hostDb);
            }

            if (_log is not null)
            {
                _log.Info($"User databases loaded. Total User Database count {UserDatabaseCount().ToString()}");
            }

        }

        /// <summary>
        /// Tries to create a new host database in a non-transactional manner. Defaults to no-transaction mode and no transaction request data
        /// </summary>
        /// <param name="dbName">The name of db to reate</param>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        internal bool XactCreateNewHostDatabase(string dbName, out Guid databaseId)
        {
            return XactCreateNewHostDatabase(dbName, TransactionRequest.GetEmpty(), TransactionMode.None, out databaseId);
        }

        /// <summary>
        /// Tries to create a new host database in a transactional manner
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="transaction"></param>
        /// <param name="transactionMode"></param>
        /// <param name="databaseId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        internal bool XactCreateNewHostDatabase(string dbName, TransactionRequest transaction, TransactionMode transactionMode, out Guid databaseId, int version = Constants.MAX_DATABASE_VERSION)
        {
            HostDb host = null;
            TransactionEntry xact = null;
            Guid systemDbId = GetGuSystemDatabase().Id;
            databaseId = Guid.Empty;
            SystemDatabase system = null;

            switch (transactionMode)
            {
                case TransactionMode.None:

                    host = CreateHostUserDatabaseOnDisk(dbName, _storage, _crypt, _cache) as HostDb;
                    databaseId = host.Id;

                    if (_log is not null)
                    {
                        _log.Info($"New Host User Database {dbName} created");
                    }

                    system = GetSystemDatabase();
                    system.XactAddNewHostDbNameToDatabasesTable(dbName, transaction, transactionMode);

                    return true;

                case TransactionMode.Try:

                    if (!HasUserDatabase(dbName, DatabaseType.Host))
                    {
                        xact = GetTransactionEntryForNewHostDatabase(transaction, dbName, systemDbId);
                        _xEntryManager.AddEntry(xact);
                        host = CreateHostUserDatabaseOnDisk(dbName, _storage, _crypt, _cache) as HostDb;
                        _storage.LogOpenTransaction(systemDbId, xact);
                        databaseId = host.Id;

                        if (_log is not null)
                        {
                            _log.Info($"Try: New User Database {dbName} created");
                        }

                        system = GetSystemDatabase();
                        system.XactAddNewHostDbNameToDatabasesTable(dbName, transaction, transactionMode);

                        return true;
                    }

                    databaseId = Guid.Empty;
                    return false;

                case TransactionMode.Rollback:

                    if (HasUserDatabase(dbName, DatabaseType.Host))
                    {
                        DeleteHostDatabase(dbName);
                        xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                        xact.MarkDeleted();
                        _storage.RemoveOpenTransaction(systemDbId, xact);
                        _xEntryManager.RemoveEntry(xact);


                        if (_log is not null)
                        {
                            _log.Info($"Rollback: New User Database {dbName} created");
                        }

                        system = GetSystemDatabase();
                        system.XactRemoveDbNameFromDatabasesTable(dbName, transaction, transactionMode);

                        return true;
                    }

                    databaseId = Guid.Empty;
                    return false;

                case TransactionMode.Commit:

                    if (HasUserDatabase(dbName, DatabaseType.Host))
                    {
                        xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                        xact.MarkComplete();
                        _storage.LogCloseTransaction(systemDbId, xact);
                        _xEntryManager.RemoveEntry(xact);

                        if (_log is not null)
                        {
                            _log.Info($"Commit: User Database {dbName} created");
                        }

                        system = GetSystemDatabase();
                        system.XactAddNewHostDbNameToDatabasesTable(dbName, transaction, transactionMode);

                        return true;
                    }

                    return false;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }
        }

        internal bool XactCreateNewPartialDatabase(Contract contract, TransactionRequest transaction, TransactionMode transactionMode, out Guid databaseId, int version = Constants.MAX_DATABASE_VERSION)
        {
            PartialDb partDb = null;
            SystemDatabase system = null;
            string dbName = contract.DatabaseName;
            TransactionEntry xact = null;
            system = GetGuSystemDatabase();
            Guid systemDbId = GetGuSystemDatabase().Id;
            databaseId = Guid.Empty;

            switch (transactionMode)
            {
                case TransactionMode.None:
                    partDb = CreatePartialUserDatabaseOnDisk(contract, _storage, _crypt, _cache);
                    databaseId = partDb.Id;

                    if (_log is not null)
                    {
                        _log.Info($"New Partial User Database {dbName} created");
                    }

                    system = GetSystemDatabase();
                    system.XactAddNewPartDbNameToDatabasesTable(dbName, transaction, transactionMode);

                    return true;

                case TransactionMode.Try:

                    if (!HasUserDatabase(dbName, DatabaseType.Host))
                    {
                        xact = GenerateTransactionEntryForNewPartDatabase(transaction, contract, systemDbId);
                        _xEntryManager.AddEntry(xact);
                        partDb = CreatePartialUserDatabaseOnDisk(contract, _storage, _crypt, _cache);
                        _storage.LogOpenTransaction(systemDbId, xact);
                        databaseId = partDb.Id;

                        if (_log is not null)
                        {
                            _log.Info($"Try: New Partial User Database {dbName} created");
                        }

                        system = GetSystemDatabase();
                        system.XactAddNewPartDbNameToDatabasesTable(dbName, transaction, transactionMode);

                        return true;
                    }

                    databaseId = Guid.Empty;
                    return false;

                case TransactionMode.Rollback:

                    /*
                    if (HasUserDatabase(dbName, DatabaseType.Host))
                    {
                        DeleteHostDatabase(dbName);
                        xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                        xact.MarkDeleted();
                        _storage.RemoveOpenTransaction(systemDbId, xact);
                        _xEntryManager.RemoveEntry(xact);


                        if (_log is not null)
                        {
                            _log.Info($"Rollback: New User Database {dbName} created");
                        }

                        system = GetSystemDatabase();
                        system.XactRemoveDbNameFromDatabasesTable(dbName, transaction, transactionMode);

                        return true;
                    }

                    databaseId = Guid.Empty;
                    return false;
                    */

                    if (HasUserDatabase(dbName, DatabaseType.Partial))
                    {
                        throw new NotImplementedException();
                    }

                    databaseId = Guid.Empty;
                    return false;

                case TransactionMode.Commit:
                    /*
                    if (HasUserDatabase(dbName, DatabaseType.Host))
                    {
                        xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                        xact.MarkComplete();
                        _storage.LogCloseTransaction(systemDbId, xact);
                        _xEntryManager.RemoveEntry(xact);

                        if (_log is not null)
                        {
                            _log.Info($"Commit: User Database {dbName} created");
                        }

                        system = GetSystemDatabase();
                        system.XactAddNewHostDbNameToDatabasesTable(dbName, transaction, transactionMode);

                        return true;
                    }

                    return false;
                    */

                    throw new NotImplementedException();

                    break;
                default:
                    throw new NotImplementedException("Unknown transaction mode");
            }

            throw new NotImplementedException();
        }

        public IDatabase GetDatabase(string dbName, DatabaseType type)
        {
            IDatabase database;
            database = _userDatabases.GetUserDatabase(dbName, type);

            if (database is null)
            {
                database = _systemDatabases.GetSystemDatabase(dbName);
                if (database is not null)
                {
                    return database;
                }
            }
            else
            {
                return database;
            }

            return null;
        }

        public HostDb GetHostDatabase(string dbName)
        {
            return _userDatabases.GetUserDatabase(dbName, DatabaseType.Host) as HostDb;
        }

        public HostDb GetHostDb(string dbName)
        {
            HostDb db = _userDatabases.GetUserDatabase(dbName, DatabaseType.Host) as HostDb;
            if (db is not null)
            {
                return db;
            }

            return null;
        }

        public PartialDb GetPartialDb(string dbName)
        {
            PartialDb db = _userDatabases.GetUserDatabase(dbName, DatabaseType.Partial) as PartialDb;
            if (db is not null)
            {
                return db;
            }

            return null;
        }

        public SystemDatabase GetSystemDatabase()
        {
            return GetGuSystemDatabase();
        }

        public Table GetTable(TreeAddress address)
        {
            if (_userDatabases.Contains(address.DatabaseId))
            {
                var db = _userDatabases.GetUserDatabase(address.DatabaseId);
                if (db.HasTable(address.TableId))
                {
                    return db.GetTable(address.TableId);
                }
            }
            else
            {
                var system = _systemDatabases.Where(db => db.Id == address.DatabaseId).FirstOrDefault();
                if (system is not null)
                {
                    return system.GetTable(address.TableId);
                }
            }

            return null;
        }

        public UserDatabase GetUserDatabase(Guid dbId)
        {
            return _userDatabases.GetUserDatabase(dbId);
        }

        /// <summary>
        /// Returns a database from the in memory collection
        /// </summary>
        /// <param name="dbName">The db to return</param>
        /// <returns>The specified db</returns>
        public UserDatabase GetUserDatabase(string dbName, DatabaseType type)
        {
            return _userDatabases.GetUserDatabase(dbName, type);
        }

        public bool HasDatabase(string dbName, DatabaseType type)
        {
            // occurs if we have not called LoadUserDatabases(), normally happens during testing
            if (_userDatabases is null)
            {
                _userDatabases = new UserDatabaseCollection();
            }

            if (_systemDatabases is null)
            {
                _systemDatabases = new SystemDatabaseCollection();
            }

            if (_userDatabases.Contains(dbName, type))
            {
                return true;
            }

            if (_systemDatabases.Contains(dbName))
            {
                return true;
            }

            return false;
        }

        public bool HasDatabase(string dbName)
        {
            // occurs if we have not called LoadUserDatabases(), normally happens during testing
            if (_userDatabases is null)
            {
                _userDatabases = new UserDatabaseCollection();
            }

            if (_systemDatabases is null)
            {
                _systemDatabases = new SystemDatabaseCollection();
            }

            if (_userDatabases.Contains(dbName))
            {
                return true;
            }

            if (_systemDatabases.Contains(dbName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the system database to see if we have generated host information. 
        /// This data is used to authorize ourselves to participants.
        /// </summary>
        /// <returns><c>TRUE</c> if there are values in the coop.HostInfo table, otherwise <c>FALSE</c></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool HasHostInfo()
        {
            var sysDb = GetSystemDatabase();
            var hostInfo = sysDb.GetTable(SystemDatabaseConstants100.Tables.HostInfo.TABLE_NAME);
            var totalCount = hostInfo.RowCount();
            return totalCount > 0;
        }

        public bool HasSystemDatabase(string name)
        {

            if (_systemDatabases is null)
            {
                _systemDatabases = new SystemDatabaseCollection();
                return false;
            }

            return _systemDatabases.Contains(name);
        }

        public bool HasTable(TreeAddress address)
        {
            if (_userDatabases.Contains(address.DatabaseId))
            {
                var db = _userDatabases.GetUserDatabase(address.DatabaseId);
                if (db.HasTable(address.TableId))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasUserDatabase(Guid dbId)
        {
            return _userDatabases.Contains(dbId);
        }

        /// <summary>
        /// Checks the <see cref="_userDatabases"/> collection to see if the specified user db exists of the specified type
        /// </summary>
        /// <param name="name">The user db name to check</param>
        /// <param name="type">The type of user database to check for</param>
        /// <returns><c>true</c> if it exists in the collection, otherwise <c>false</c></returns>
        public bool HasUserDatabase(string name, DatabaseType type)
        {
            // occurs if we have not called LoadUserDatabases(), normally happens during testing
            if (_userDatabases is null)
            {
                _userDatabases = new UserDatabaseCollection();
                return false;
            }

            return _userDatabases.Contains(name, type);
        }

        /// <summary>
        /// Checsk the <see cref="_userDatabases"/> collect to see if the specified user db exists regardless of the type
        /// </summary>
        /// <param name="name">The user db name to check</param>
        /// <returns><c>true</c> if it exists in the collection, otherwise <c>false</c></returns>
        public bool HasUserDatabase(string name)
        {
            // occurs if we have not called LoadUserDatabases(), normally happens during testing
            if (_userDatabases is null)
            {
                _userDatabases = new UserDatabaseCollection();
                return false;
            }

            return _userDatabases.Contains(name);
        }

        public bool IsHostDatabase(string dbName)
        {
            var db = _userDatabases.GetUserDatabase(dbName, DatabaseType.Host);
            if (db is not null)
            {
                return true;
            }

            return false;
        }

        public bool IsPartialDatabase(string dbName)
        {
            var db = _userDatabases.GetUserDatabase(dbName, DatabaseType.Partial);
            if (db is not null)
            {
                return true;
            }

            return false;
        }

        public int SystemDatabaseCount() => _systemDatabases.Count();

        public string[] SystemDatabaseNames() => _systemDatabases.Names();

        public void UpdateHostInfoInDatabases(Guid hostGuid, string hostName, byte[] token)
        {
            _hostInfo.HostGUID = hostGuid;
            _hostInfo.HostName = hostName;
            _hostInfo.Token = token;

            foreach (var db in _userDatabases)
            {
                if (db is HostDb)
                {
                    var host = db as HostDb;
                    host.UpdateHostInfo(hostGuid, hostName, token);
                }
            }
        }

        public void UpdateHostInfoInDatabases(HostInfo hostInfo)
        {
            _hostInfo = hostInfo;

            // this happens during testing, where we haven't gone thru
            // the full bootup process yet
            if (_userDatabases is not null)
            {
                foreach (var db in _userDatabases)
                {
                    if (db is HostDb)
                    {
                        var host = db as HostDb;
                        host.UpdateHostInfo(hostInfo);
                    }
                }
            }
        }

        public int UserDatabaseCount() => _userDatabases.Count();
        public string[] UserDatabaseNames() => _userDatabases.Names();
        /// <summary>
        /// Validates that the login exists
        /// </summary>
        /// <param name="userName">The user name to validate</param>
        /// <param name="pwInput">The pw of the user</param>
        /// <returns><c>true</c> if the username/pw is valid, otherwise false</returns>
        /// <remarks>This function is deprecated and should be moved to <see cref="IAuthenticationManager"/></remarks>
        public bool ValidateLogin(string userName, string pwInput)
        {
            SystemDatabase db = GetGuSystemDatabase();
            return db.ValidateLogin(userName, pwInput);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Adds a database from disk to the in memory collection if it is not already in memory
        /// </summary>
        /// <param name="dbName"></param>
        private void AddUserDatabaseToCollection(UserDatabase database)
        {
            _userDatabases.Add(database);
        }

        /// <summary>
        /// Creates the needed system pages from <see cref="DatabasePageFactory"/>, asks <see cref="IStorageManager"/> to save those pages to disk, and adds the <see cref="ISystemPage"/> to <see cref="ICacheManager"/> and returns the 
        /// newly created <see cref="UserDatabase"/>
        /// </summary>
        /// <param name="dbName">The name of the new database</param>
        /// <param name="storage">A reference to the storage manage</param>
        /// <param name="crypt">A reference to the crypt manager</param>
        /// <param name="cache">A reference to the cache manager</param>
        /// <returns>A newly constructored <see cref="UserDatabase"/> after needed structures and persisted to disk and loaded into memory</returns>
        private UserDatabase CreateHostUserDatabaseOnDisk(string dbName, IStorageManager storage, ICryptoManager crypt, ICacheManager cache)
        {
            if (_storage is null)
            {
                _storage = storage;
            }

            if (_crypt is null)
            {
                _crypt = crypt;
            }

            if (_cache is null)
            {
                _cache = cache;
            }

            int version = Constants.DatabaseVersions.V100;
            Guid dbId = Guid.NewGuid();

            List<IPage> pages = DatabasePageFactory.GetNewDatabasePages(dbName, DataFileType.Host, dbId, version);
            _storage.CreateUserDatabase(dbName, pages, DataFileType.Host, version);

            SystemPage systemPage = null;

            foreach (var page in pages)
            {
                if (page is SystemPage)
                {
                    systemPage = page as SystemPage;
                    _cache.AddUserDbSystemPage(systemPage);
                }
            }

            var metadata = new DatabaseMetadata(systemPage, _cache, _crypt, this, _storage, _xEntryManager, new RemoteDataManager(_hostInfo));
            var hostDb = new HostDb(metadata, _xEntryManager);

            if (!HasUserDatabase(hostDb.Name, DatabaseType.Host))
            {
                AddUserDatabaseToCollection(hostDb);
            }

            return hostDb;
        }

        private PartialDb CreatePartialUserDatabaseOnDisk(Contract contract, IStorageManager storage, ICryptoManager crypt, ICacheManager cache)
        {
            if (_storage is null)
            {
                _storage = storage;
            }

            if (_crypt is null)
            {
                _crypt = crypt;
            }

            if (_cache is null)
            {
                _cache = cache;
            }

            int version = Constants.DatabaseVersions.V100;
            Guid dbId = Guid.NewGuid();

            List<IPage> pages = DatabasePageFactory.GetNewDatabasePages(contract.DatabaseName, DataFileType.Partial, dbId, version);
            _storage.CreateUserDatabase(contract.DatabaseName, pages, DataFileType.Partial, version);

            SystemPage systemPage = null;

            foreach (var page in pages)
            {
                if (page is SystemPage)
                {
                    systemPage = page as SystemPage;
                    _cache.AddUserDbSystemPage(systemPage);
                }
            }

            var metadata = new DatabaseMetadata(systemPage, _cache, _crypt, this, _storage, _xEntryManager, new RemoteDataManager(_hostInfo));
            var partDb = new PartialDb(metadata, _xEntryManager, contract);

            if (!HasUserDatabase(partDb.Name, DatabaseType.Partial))
            {
                AddUserDatabaseToCollection(partDb);
            }

            return partDb;
        }
        /// <summary>
        /// Creates the needed system pages from <see cref="DatabasePageFactory"/>, asks <see cref="IStorageManager"/> to save those pages to disk, and adds the <see cref="ISystemPage"/> to <see cref="ICacheManager"/> and returns the 
        /// newly created <see cref="SystemDatabase"/>
        /// </summary>
        /// <param name="storage">A reference to the storage manager</param>
        /// <param name="crypt">A reference to the crypt manager</param>
        /// <param name="cache">A reference to the cache manager</param>
        /// <returns>A newly constructed <see cref="SystemDatabase"/> after needed structures are created and persisted to disk and loaded into memory</returns>
        private SystemDatabase CreateSystemDatabaseOnDisk(IStorageManager storage, ICryptoManager crypt, ICacheManager cache, ITransactionEntryManager xEntryManager)
        {
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;
            int version = Constants.DatabaseVersions.V100;
            Guid dbId = Guid.NewGuid();

            List<IPage> pages = DatabasePageFactory.GetNewDatabasePages(dbName, DataFileType.System, dbId, version);
            _storage.CreateSystemDatabase(dbName, pages, DataFileType.System, version);

            SystemPage systemPage = null;

            foreach (var page in pages)
            {
                if (page is SystemPage)
                {
                    systemPage = page as SystemPage;
                    _cache.AddSystemDbSystemPage(systemPage);
                }
            }

            var metadata = new DatabaseMetadata(systemPage, _cache, _crypt, this, _storage, xEntryManager, new RemoteDataManager(_hostInfo));
            var system = new SystemDatabase(metadata);

            return system;
        }

        private SystemDatabase GetGuSystemDatabase()
        {
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;
            return _systemDatabases.GetSystemDatabase(dbName);
        }

        public TransactionEntry GetTransactionEntryForNewHostDatabase(TransactionRequest request, string dbName, Guid systemDbId)
        {
            var createDbX = new CreateHostDbTransaction(dbName);
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(request.TransactionBatchId);

            var xEntry = new TransactionEntry
                (request.TransactionBatchId,
                systemDbId,
                TransactionActionType.Schema,
                Constants.DatabaseVersions.V100,
                createDbX,
                request.UserName,
                false,
                sequenceId
                );

            return xEntry;
        }

        public TransactionEntry GenerateTransactionEntryForNewPartDatabase(TransactionRequest request, Contract contract, Guid systemDbId)
        {
            var createDbX = new CreatePartialDbTransaction(contract);
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(request.TransactionBatchId);

            var xEntry = new TransactionEntry
                (request.TransactionBatchId,
                systemDbId,
                TransactionActionType.Schema,
                Constants.DatabaseVersions.V100,
                createDbX,
                request.UserName,
                false,
                sequenceId
                );

            return xEntry;
        }

        #endregion
    }
}
