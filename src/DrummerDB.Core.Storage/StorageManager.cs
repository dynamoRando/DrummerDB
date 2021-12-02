using Drummersoft.DrummerDB.Core.Storage.Abstract;
using Drummersoft.DrummerDB.Core.Storage.Factory;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Storage.Version;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// Manages all actions related to saving data to disk.
    /// </summary>
    internal class StorageManager : IStorageManager
    {
        #region Private Fields
        private string _hostDbExtension = string.Empty;
        private string _logFileExtension = string.Empty;
        private string _partialDbExtension = string.Empty;
        private string _storageFolder = string.Empty;
        private string _systemDbExtension = string.Empty;
        private string _contractFolder = string.Empty;
        private string _contractFolderPath = string.Empty;

        private SystemDbFileHandlerCollection _systemDbFiles;
        private UserDbFileHandlerCollection _userDbFiles;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs an instance of Storage Manager. If the specified folder does not exist, it will create it
        /// </summary>
        /// <param name="storageFolder">The folder where databases are held</param>
        /// <param name="hostDbExtension">The file extension for host dbs</param>
        /// <param name="partialDbExtension">The file extension for partial dbs</param>
        /// <param name="logFileExtension">The file extension for a db log file</param>
        internal StorageManager(string storageFolder, string hostDbExtension, string partialDbExtension, string logFileExtension, string systemDbExtension, string contractFolder)
        {
            _storageFolder = storageFolder;
            _hostDbExtension = hostDbExtension;
            _partialDbExtension = partialDbExtension;
            _logFileExtension = logFileExtension;
            _systemDbExtension = systemDbExtension;
            _contractFolder = contractFolder;

            _userDbFiles = new UserDbFileHandlerCollection();

            if (!Directory.Exists(_storageFolder))
            {
                Directory.CreateDirectory(_storageFolder);
            }

            string contractFolderPath = Path.Combine(_storageFolder, _contractFolder);

            if (!Directory.Exists(contractFolderPath))
            {
                Directory.CreateDirectory(contractFolderPath);
            }

            _contractFolderPath = contractFolderPath;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the needed database structures on disk for a system database
        /// </summary>
        /// <param name="dbName">The database name</param>
        /// <param name="pages">The inital set of pages to save to disk</param>
        /// <param name="type">The type of data file (an enum)</param>
        /// <param name="auth">A reference to the authentication manager</param>
        /// <param name="version">The database version</param>
        public void CreateSystemDatabase(string dbName, List<IPage> pages, DataFileType type, int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    switch (type)
                    {
                        case DataFileType.System:
                            string dataFileName = Path.Combine(_storageFolder, dbName + _systemDbExtension);
                            string logFileName = Path.Combine(_storageFolder, dbName + _logFileExtension);

                            DbSystemDataFile dataFile =
                                DbDataFileFactory.GetSystemDbDataFileVersion(version, dataFileName, pages, dbName);
                            var logfile = DbLogFileFactory.GetDbLogFileVersion(version, logFileName);

                            Guid dbId = Guid.Empty;
                            IPage page = pages.Where(p => p is ISystemPage).FirstOrDefault();

                            if (page is ISystemPage)
                            {
                                dbId = (page as SystemPage).DatabaseId;
                            }

                            SystemDbFileHandler databaseFileHandler = DbFileHandlerFactory.GetSystemDbFileHandlerForVersion(dbName, _storageFolder, _hostDbExtension, _logFileExtension, dataFile, logfile, dbId, version);

                            if (_systemDbFiles is null)
                            {
                                _systemDbFiles = new SystemDbFileHandlerCollection(Constants.SYSTEM_DATABASE_COUNT);
                            }

                            _systemDbFiles.Add(databaseFileHandler);
                            break;
                        default:
                            throw new ArgumentException("Unknown data file type");
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown or no database version supplied");
            }
        }

        /// <summary>
        /// Creates the needed database structures on disk for a user database
        /// </summary>
        /// <param name="dbName">The database name</param>
        /// <param name="pages">The inital set of pages to save to disk</param>
        /// <param name="type">The type of data file (an enum)</param>
        /// <param name="auth">A reference to the authentication manager</param>
        /// <param name="version">The database version</param>
        public void CreateUserDatabase(string dbName, List<IPage> pages, DataFileType type, int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    switch (type)
                    {
                        case DataFileType.Host:
                            string dataFileName = Path.Combine(_storageFolder, dbName + _hostDbExtension);
                            string logFileName = Path.Combine(_storageFolder, dbName + _logFileExtension);

                            IDbDataFile dataFile =
                                DbDataFileFactory.GetHostDbDataFileVersion(version, dataFileName, pages, dbName);

                            IDbLogFile logfile = DbLogFileFactory.GetDbLogFileVersion(version, logFileName);

                            Guid dbId = Guid.Empty;
                            IPage page = pages.Where(p => p is ISystemPage).FirstOrDefault();

                            if (page is ISystemPage)
                            {
                                dbId = (page as SystemPage).DatabaseId;
                            }

                            var databaseFileHandler = DbFileHandlerFactory.GetUserDbFileHandlerForVersion(dbName, _storageFolder, _hostDbExtension, _logFileExtension, dataFile, logfile, dbId, version);
                            _userDbFiles.Add(databaseFileHandler);
                            break;
                        case DataFileType.Partial:
                            break;
                        default:
                            throw new ArgumentException("Unknown data file type");
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown or no database version supplied");
            }
        }

        /// <summary>
        /// Removes the specified db from the <see cref="UserDbFileHandlerCollection"/> and deletes the db files (log and system) from disk
        /// </summary>
        /// <param name="dbName">The name of the db to remove</param>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool DeleteUserDatabase(string dbName)
        {
            if (_userDbFiles.Contains(dbName))
            {
                var db = _userDbFiles.Get(dbName);
                db.DeleteFromDisk();
                _userDbFiles.Remove(dbName);
                return true;
            }

            return false;
        }

        public List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema)
        {
            UserDbFileHandler file = _userDbFiles.Get(address.DatabaseId);

            if (file is not null)
            {
                return file.GetAllUserDataPages(address, schema);
            }

            return null;
        }

        public IBaseDataPage GetAnyDataPage(int[] pagesInMemory, TreeAddress address, PageType type)
        {
            throw new NotImplementedException();
        }

        public UserDataPage GetAnySystemDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema, int tableId)
        {
            SystemDbFileHandler file = _systemDbFiles.Get(address.DatabaseId);

            if (file is not null)
            {
                PageAddress[] pages = new PageAddress[pagesInMemory.Length];
                int i = 0;
                foreach (var item in pagesInMemory)
                {
                    pages[i] = new PageAddress(address.DatabaseId, address.TableId, item, address.SchemaId);
                    i++;
                }
                return file.GetAnyUserDataPage(pages, schema, tableId);
            }

            return null;
        }

        public UserDataPage GetAnyUserDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema, int tableId)
        {
            UserDbFileHandler file = _userDbFiles.Get(address.DatabaseId);

            if (file is not null)
            {
                PageAddress[] pages = new PageAddress[pagesInMemory.Length];
                int i = 0;
                foreach (var item in pagesInMemory)
                {
                    pages[i] = new PageAddress(address.DatabaseId, address.TableId, item, address.SchemaId);
                    i++;
                }
                return file.GetAnyUserDataPage(pages, schema, tableId);
            }

            return null;
        }

        public int GetMaxPageId(TreeAddress address)
        {
            int result = 0;
            var file = _userDbFiles.Get(address.DatabaseId);
            if (file is not null)
            {
                result = file.GetMaxPageId(address);
            }

            return result;
        }

        /// <summary>
        /// Scans thru the <see cref="_storageFolder"/> and returns all the file names without the extension of any <see cref="_systemDbExtension"/> files
        /// </summary>
        /// <returns>A list of system database names from the <see cref="_storageFolder"/></returns>
        public List<string> GetSystemDatabaseNames()
        {
            var files = Directory.GetFiles(_storageFolder, "*" + _systemDbExtension);
            var result = new List<string>(files.Length);

            foreach (var file in files)
            {
                result.Add(Path.GetFileNameWithoutExtension(Path.GetFileName(file)));
            }

            return result;
        }

        public ISystemPage GetSystemPage(Guid dbId)
        {
            ISystemPage page = null;
            var file = _userDbFiles.Get(dbId);
            if (file is not null)
            {
                page = file.GetSystemPage();
            }
            else
            {
                var foo = _systemDbFiles.Get(dbId);
                if (foo is not null)
                {
                    page = foo.GetSystemPage();
                }
            }

            return page;
        }

        /// <summary>
        /// Returns the <see cref="ISystemPage"/> for the specified system database name from disk. If <seealso cref="_systemDbFiles"/> is not initalized or does not contain the specified file, it will attempt to load into memory.
        /// </summary>
        /// <param name="dbName">The system database name to get the system page from</param>
        /// <returns>The <see cref="ISystemPage"/> for the system db name specified, or <c>NULL</c> if not found</returns>
        public ISystemPage GetSystemPageForSystemDatabase(string dbName)
        {
            SystemDbFileHandler db = null;

            if (_systemDbFiles is null)
            {
                _systemDbFiles = new SystemDbFileHandlerCollection();
            }

            if (_systemDbFiles.Count > 0)
            {
                if (_systemDbFiles.Contains(dbName))
                {
                    db = _systemDbFiles.Get(dbName);
                }
                else
                {
                    // we have not loaded the db file from disk, or it does not exist
                    string fileName = Path.Combine(_storageFolder, dbName, _systemDbExtension);
                    if (File.Exists(fileName))
                    {
                        db = LoadSystemFileIntoMemory(fileName);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                // we need to load system databases into memory
                LoadSystemDatabaseFilesIntoMemory();

                if (_systemDbFiles.Contains(dbName))
                {
                    db = _systemDbFiles.Get(dbName);
                }

            }

            if (db is not null)
            {
                return db.GetSystemPage();
            }
            else
            {
                return null;
            }
        }

        public int GetTotalPages(TreeAddress address)
        {
            int result = 0;
            var file = _userDbFiles.Get(address.DatabaseId);
            if (file is not null)
            {
                result = file.GetTotalPages(address);
            }
            else
            {
                var sysFile = _systemDbFiles.Get(address.DatabaseId);
                if (sysFile is not null)
                {
                    result = sysFile.GetTotalPages(address);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks the <see cref="_userDbFiles"/> collection for the specified DbName and returns if it exists, otherwise <c>NULL</c>
        /// </summary>
        /// <param name="dbName">The dbName to check</param>
        /// <returns>A reference to the specified <see cref="UserDbFileHandler"/>, or <c>NULL</c> if it is not found</returns>
        public UserDbFileHandler GetUserDatabaseFile(string dbName)
        {
            return _userDbFiles.Get(dbName);
        }

        public List<string> GetUserDatabaseNames()
        {
            var files = Directory.GetFiles(_storageFolder, "*" + _hostDbExtension);
            var result = new List<string>(files.Length);

            foreach (var file in files)
            {
                result.Add(Path.GetFileNameWithoutExtension(Path.GetFileName(file)));
            }

            return result;
        }

        public List<UserDatabaseInformation> GetUserDatabasesInformation()
        {
            var infos = new List<UserDatabaseInformation>(_userDbFiles.Count);

            foreach (var db in _userDbFiles)
            {
                var info = new UserDatabaseInformation { DatabaseId = db.DbId, DatabaseVersion = db.Version, DatabaseName = db.DatabaseName };
                infos.Add(info);
            }

            return infos;
        }

        public bool IsSystemDatabase(Guid databaseId)
        {
            return _systemDbFiles.Any(db => db.DbId == databaseId);
        }

        public bool IsUserDatabase(Guid databaseId)
        {
            return _userDbFiles.Any(db => db.DbId == databaseId);
        }
        public void LoadSystemDatabaseFilesIntoMemory()
        {
            var files = GetSystemDatabaseNames();

            if (_systemDbFiles is null)
            {
                _systemDbFiles = new SystemDbFileHandlerCollection(files.Count);
            }

            foreach (var db in files)
            {
                LoadSystemFileIntoMemory(db);
            }
        }

        public void LoadUserDatabaseFilesIntoMemory()
        {
            var files = GetUserDatabaseNames();

            if (_userDbFiles is null)
            {
                _userDbFiles = new UserDbFileHandlerCollection(files.Count);
            }

            foreach (var file in files)
            {
                string fileName = Path.Combine(_storageFolder, file + _hostDbExtension);
                LoadUserFileIntoMemory(fileName);
            }
        }

        public void LogCloseTransaction(Guid databaseId, TransactionEntry transaction)
        {
            UserDbFileHandler sysFile = _userDbFiles.Get(databaseId);

            if (sysFile is not null)
            {
                sysFile.LogCloseOpenTransactionToDisk(transaction);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public bool LogFileHasOpenTransaction(Guid databaseId, TransactionEntryKey key)
        {
            UserDbFileHandler userFile = _userDbFiles.Get(databaseId);

            if (userFile is not null)
            {
                return userFile.OpenTransactionIsOnDisk(key);
            }
            else
            {
                return false;
            }
        }

        public void LogOpenTransaction(Guid databaseId, TransactionEntry transaction)
        {
            UserDbFileHandler userFile = _userDbFiles.Get(databaseId);

            if (userFile is not null)
            {
                userFile.LogOpenTransactionToDisk(transaction);
            }
            else
            {
                var systemFile = _systemDbFiles.Get(databaseId);
                if (systemFile is not null)
                {
                    systemFile.LogOpenTransactionToDisk(transaction);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void RemoveOpenTransaction(Guid databaseId, TransactionEntry entry)
        {
            UserDbFileHandler sysFile = _userDbFiles.Get(databaseId);

            if (sysFile is not null)
            {
                sysFile.RemoveOpenTransactionOnDisk(entry);
            }
            else
            {
                SystemDbFileHandler systemDbFile = _systemDbFiles.Get(databaseId);
                if (systemDbFile is not null)
                {
                    systemDbFile.RemoveOpenTransactionOnDisk(entry);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown Database Id {databaseId.ToString()}");
                }
            }
        }
        public void SavePageDataToDisk(PageAddress address, byte[] data, PageType type, DataPageType dataPageType, bool isDeleted)
        {
            UserDbFileHandler file = _userDbFiles.Get(address.DatabaseId);

            if (file is not null)
            {
                file.WritePageToDisk(data, address, type, dataPageType, isDeleted);
            }
            else
            {
                SystemDbFileHandler system = _systemDbFiles.Get(address.DatabaseId);

                if (system is not null)
                {
                    system.WritePageToDisk(data, address, type, dataPageType, isDeleted);
                }
                else
                {
                    throw new InvalidOperationException($"Address {address.DatabaseId.ToString()} | {address.TableId.ToString()} | {address.PageId.ToString()} was not found on disk");
                }
            }
        }

        public int TotalSystemDatabasesOnDisk()
        {
            return Directory.GetFiles(_storageFolder, "*" + _systemDbExtension).Length;
        }

        public int TotalUserDatabasesOnDisk()
        {
            return Directory.GetFiles(_storageFolder, "*" + _hostDbExtension).Length;
        }

        public bool SaveContractToDisk(Contract contract)
        {
            try
            {
                string fileName = contract.DatabaseName;
                string fullPath = Path.Combine(_contractFolderPath, fileName);

                var contractJson = JsonSerializer.Serialize(contract);
                File.WriteAllText(fullPath, contractJson);

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the specified data file and log file into memory and returns it to caller. Also adds the file to the <see cref="_systemDbFiles"/> collection.
        /// </summary>
        /// <param name="fileName">The data file to add</param>
        /// <returns>An instance of the initalized <see cref="SystemDbFileHandler"/>, or <c>NULL</c></returns>
        private SystemDbFileHandler LoadSystemFileIntoMemory(string dbName)
        {
            string dataFile = dbName + _systemDbExtension;
            string dataFileName = Path.Combine(_storageFolder, dataFile);

            string logFile = dbName + _logFileExtension;
            string logFileName = Path.Combine(_storageFolder, logFile);

            SystemDbFileHandler db;

            if (!File.Exists(dataFileName))
            {
                return null;
            }

            int version = DbBasicDataFileReader.GetDatabaseVersion(dataFileName);
            Guid dbId = DbBasicDataFileReader.GetDatabaseId(dataFileName);

            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    DbSystemDataFile systemDb = DbDataFileFactory.GetSystemDbDataFileVersion(version, dataFileName, dbId);
                    IDbLogFile logDbFile = DbLogFileFactory.GetDbLogFileVersion(version, logFileName);
                    var file = new SystemDbFileHandler100(dataFileName, _storageFolder, _hostDbExtension, _logFileExtension, systemDb, logDbFile, dbId);

                    if (!_systemDbFiles.Contains(file.DatabaseName))
                    {
                        _systemDbFiles.Add(file);
                    }

                    db = file;
                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return db;
        }

        private UserDbFileHandler LoadUserFileIntoMemory(string fileName)
        {
            UserDbFileHandler db;

            if (!File.Exists(fileName))
            {
                return null;
            }

            int version = DbBasicDataFileReader.GetDatabaseVersion(fileName);
            Guid dbId = DbBasicDataFileReader.GetDatabaseId(fileName);

            switch (version)
            {
                case Constants.DatabaseVersions.V100:

                    IDbDataFile hostDb = DbDataFileFactory.GetHostDbDataFileVersion(version, fileName, dbId);
                    string dbFileName = Path.GetFileNameWithoutExtension(fileName);
                    IDbLogFile logDbFile = DbLogFileFactory.GetDbLogFileVersion(version, Path.Combine(_storageFolder, dbFileName + _logFileExtension));
                    var file = new UserDbFileHandler100(fileName, _storageFolder, _hostDbExtension, _logFileExtension, hostDb, logDbFile, dbId);

                    if (!_userDbFiles.Contains(file.DatabaseName))
                    {
                        _userDbFiles.Add(file);
                    }

                    db = file;

                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return db;
        }
        #endregion

    }
}
