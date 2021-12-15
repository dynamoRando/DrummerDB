using Drummersoft.DrummerDB.Core.Communication;
using Drummersoft.DrummerDB.Core.Communication.Interface;
using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Cryptography.Interface;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Drummersoft.DrummerDB.Core.Systems
{
    /// <summary>
    /// An instance of DrummerDB
    /// </summary>
    public sealed class Process
    {
        #region Private Fields
        private Settings _settings;
        private DbManager _dbManager;
        private ICacheManager _cache;
        private IStorageManager _storage;
        private INetworkManager _network;
        private IQueryManager _queries;
        private IAuthenticationManager _auth;
        private ICryptoManager _crypt;
        private ITransactionEntryManager _xEntryManager;
        private LogService _logService;

        // test variables
        private string _storageFolder;
        private bool _loadSystemDatabases = true;
        private bool _loadUserDatabases = true;

        private HostInfo _hostInfo;
        #endregion

        #region Public Properties
        /// <summary>
        /// The settings for this process
        /// </summary>
        internal Settings Settings => _settings;

        // the following should only be used for Testing purposes, not called from any external assembly.
        // this is testing for all up integration
        internal DbManager DbManager => _dbManager;
        internal CryptoManager CryptoManager => _crypt as CryptoManager;
        internal StorageManager StorageManager => _storage as StorageManager;
        internal CacheManager CacheManager => _cache as CacheManager;
        internal IdentityAccess.AuthenticationManager AuthenticationManager => _auth as IdentityAccess.AuthenticationManager;
        #endregion

        #region Constructors
        public Process()
        {
            // default consturctor
        }

        /// <summary>
        /// Constructs a Process with the specified storage folder path. This is used for testing purposes only and will override the default setting in <see cref="Settings.DatabaseFolder"/>
        /// </summary>
        /// <param name="storageFolderPath">The path to the appropriate storage folder</param>
        public Process(string storageFolderPath)
        {
            _storageFolder = storageFolderPath;
        }

        /// <summary>
        /// Constructs a Process with the specified storage folder path and loads system databases. This is used for testing purposes only and will override the default setting in <see cref="Settings.DatabaseFolder"/>
        /// </summary>
        /// <param name="storageFolderPath">the path to the appropriate storage folder</param>
        /// <param name="loadSystemDatabases">Default false. Determines if on startup we should load the system databases</param>
        public Process(string storageFolderPath, bool loadSystemDatabases)
        {
            _storageFolder = storageFolderPath;
            _loadSystemDatabases = loadSystemDatabases;
        }

        /// <summary>
        /// Constructs a Process with the specified storage folder path and loads system databases. This is used for testing purposes only and will override the default setting in <see cref="Settings.DatabaseFolder"/>
        /// </summary>
        /// <param name="loadSystemDatabases">Default false. Determines if on startup we should load the system databases</param>
        /// <param name="loadUserDatabases">Default false. Determines if on startup we should load the user databases</param>
        public Process(bool loadSystemDatabases, bool loadUserDatabases)
        {
            _loadSystemDatabases = loadSystemDatabases;
            _loadUserDatabases = loadUserDatabases;
        }

        /// <summary>
        /// Constructs a Process with the specified storage folder path and loads system databases and user databases. 
        /// This is used for testing purposes only and will override the default setting in <see cref="Settings.DatabaseFolder"/>
        /// </summary>
        /// <param name="storageFolderPath">the path to the appropriate storage folder</param>
        /// <param name="loadSystemDatabases">Default false. Determines if on startup we should load the system databases</param>
        /// <param name="loadUserDatabases">Default false. Determines if on startup we should load the user databases</param>
        public Process(string storageFolderPath, bool loadSystemDatabases, bool loadUserDatabases) : this(loadSystemDatabases, loadUserDatabases)
        {
            _storageFolder = storageFolderPath;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the drummerDB instance. Loads databases, brings online networking, etc.
        /// </summary>
        public void Start()
        {
            LoadConfiguration();


            if (Settings.EnableLogging)
            {
                ConfigureLogService();
            }

            SetupCrypt();
            SetupStorage();
            SetupMemory();
            SetupTransactionEntryManager();
            SetupDatabases();
            SetupAuth();
            SetupQueries();
            SetupNetwork();
            LoadDatabases();
            CheckForAdminSetup();
        }

        public void Stop()
        {
            _network.StopServerForSQLService();
            _network.StopServerForDatabaseService();
            _network.StopServerForInfoService();
        }

        /// <summary>
        /// Starts the SQL Service for this Process and overrides settings in the appsettings.json
        /// </summary>
        /// <param name="overrideSettingsPortNumber">The port number to use if you wish to override the setting in the Process' appsettings.json</param>
        /// <param name="overrideSettingsUseHttps">Pass to override the setting to use HTTPS in the Process' appsettings.json</param>
        public void StartSQLServer(int overrideSettingsPortNumber, bool overrideSettingsUseHttps)
        {
            _network.StartServerForSQLService(overrideSettingsUseHttps, _auth, _dbManager, overrideSettingsPortNumber, _queries);
        }

        public void StartSQLServer()
        {
            _network.StartServerForSQLService(Settings.UseHttpsForConnections, _auth, _dbManager, _queries);
        }

        public void StartInfoServer()
        {
            _network.StartServerForInfoService(Settings.UseHttpsForConnections, _auth, _dbManager);
        }

        public void StartInfoServer(int overrideSettingsPortNumber, bool overrideSettingsUseHttps)
        {
            _network.StartServerForInfoService(overrideSettingsUseHttps, _auth, _dbManager, overrideSettingsPortNumber);
        }

        /// <summary>
        /// Starts the database service for the Process with the default settings
        /// </summary>
        public void StartDbServer()
        {
            _network.StartServerForDatabaseService(Settings.UseHttpsForConnections, _auth, _dbManager, _storage);
        }

        /// <summary>
        /// Starts the database service for the Process with the specified port number. This value will override what is in the appsettings.json file
        /// </summary>
        /// <param name="overrideSettingsPortNumber">The port number to start the database service on</param>
        /// <param name="overrideSettingsUseHttps">Overrides the settings file to default true/false for using HTTPS. Used for testing purposes.</param>
        public void StartDbServer(int overrideSettingsPortNumber, bool overrideSettingsUseHttps)
        {
            _network.StartServerForDatabaseService(overrideSettingsUseHttps, _auth, _dbManager, overrideSettingsPortNumber, _storage);
            _hostInfo.DatabasePortNumber = overrideSettingsPortNumber;
            _dbManager.UpdateHostInfoInDatabases(_hostInfo);
        }

        /// <summary>
        /// This function is deprecated and should be removed. It is only for testing purposes.
        /// </summary>
        /// <param name="userName">The login to create</param>
        /// <param name="pw">The pw</param>
        /// <param name="guid">The login guid</param>
        public void Test_SetupLogin(string userName, string pw, Guid guid)
        {
            _dbManager.CreateLogin(userName, pw, guid);
        }

        /// <summary>
        /// This function is deprecated and should be removed. It is only for testing purposes. Creates an admin login for the rpcoess.
        /// </summary>
        /// <param name="userName">The login to creat</param>
        /// <param name="pw">The pw</param>
        /// <param name="guid">The login guid</param>
        public void Test_SetupAdminLogin(string userName, string pw, Guid guid)
        {
            _dbManager.CreateAdminLogin(userName, pw, guid);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads configuration settings for the Process
        /// </summary>
        private void LoadConfiguration()
        {
            _settings = new Settings();
            _settings = new Configurator().Load();
        }

        private void SetupStorage()
        {
            if (string.IsNullOrEmpty(_storageFolder))
            {
                _storage = new StorageManager(
                    Settings.DatabaseFolder,
                    Settings.HostDbExtension,
                    Settings.PartialDbExtension,
                    Settings.HostDatabaseLogExtension,
                    Settings.PartDatabaseLogExtension,
                    Settings.SystemDbExtension,
                    Settings.ContractFolderName,
                    Settings.ContractFileExtension
                );
            }
            else
            {
                _storage = new StorageManager(
                    _storageFolder,
                    Settings.HostDbExtension,
                    Settings.PartialDbExtension,
                    Settings.HostDatabaseLogExtension,
                    Settings.PartDatabaseLogExtension,
                    Settings.SystemDbExtension,
                    Settings.ContractFolderName,
                    Settings.ContractFileExtension
                );
            }
        }

        private void SetupDatabases()
        {
            _dbManager = new DbManager(_xEntryManager);
        }

        private void LoadDatabases()
        {
            if (_loadSystemDatabases)
            {
                _dbManager.LoadSystemDatabases(_cache, _storage, _crypt, _hostInfo);
                ConfigureHostInfo();
            }

            if (_loadUserDatabases)
            {
                _dbManager.LoadUserDatabases(_cache, _storage, _crypt, _hostInfo);
            }

            if (_loadUserDatabases && _loadSystemDatabases)
            {
                _dbManager.LoadSystemDatabaseTableWithActiveDbs();
            }
        }

        private void SetupMemory()
        {
            if (_logService is not null)
            {
                _cache = new CacheManager(_logService);
            }
            else
            {
                _cache = new CacheManager();
            }

        }

        private void SetupTransactionEntryManager()
        {
            _xEntryManager = new TransactionEntryManager();
        }

        private void SetupQueries()
        {
            if (Settings.EnableLogging)
            {
                _queries = new QueryManager(_dbManager, _auth, _xEntryManager, _logService);
            }
            else
            {
                _queries = new QueryManager(_dbManager, _auth, _xEntryManager);
            }

        }

        private void SetupNetwork()
        {
            var sqlPort = new PortSettings { IPAddress = Settings.IP4Adress, PortNumber = Settings.SQLServicePort };
            var databasePort = new PortSettings { IPAddress = Settings.IP4Adress, PortNumber = Settings.DatabaseServicePort };
            var infoPort = new PortSettings { IPAddress = Settings.IP4Adress, PortNumber = Settings.InfoServicePort };
            _network = new NetworkManager(databasePort, sqlPort, infoPort, _queries, _dbManager, _logService, _hostInfo);
        }

        private void SetupAuth()
        {
            _auth = new IdentityAccess.AuthenticationManager(_dbManager);
        }

        private void SetupCrypt()
        {
            _crypt = new CryptoManager();
        }

        private void CheckForAdminSetup()
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directoryInfo = new DirectoryInfo(rootPath);
            if (directoryInfo.GetFiles().Any(file => file.Name == Constants.ADMIN_SETUP))
            {
                var text = File.ReadAllText(Path.Combine(rootPath, Constants.ADMIN_SETUP));
                var items = text.Split(",");
                var userName = items[0];
                var password = items[1];
                Guid userId = Guid.Parse(items[2]);

                Debug.WriteLine($"Creating admin {userName}");

                Test_SetupAdminLogin(userName, password, userId);
            }
        }

        private void ConfigureLogService()
        {
            var config = new NLog.Config.LoggingConfiguration();
            string fullPath = string.Empty;

            if (!string.IsNullOrEmpty(_storageFolder))
            {
                fullPath = Path.Combine(_storageFolder, Settings.LogFileName);
            }
            else
            {
                fullPath = Path.Combine(Settings.DatabaseFolder, Settings.LogFileName);
            }


            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fullPath };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;

            var logger = NLog.LogManager.GetCurrentClassLogger();

            _logService = new LogService(logger, Settings.EnableLogging, Settings.LogPerformanceMetrics);
            _logService.Info("DrummerDB started");

            DateTimeOffset localTime = DateTimeOffset.Now;
            DateTimeOffset utcTime = DateTimeOffset.UtcNow;

            string currentMessage = $"Local Time: {localTime.ToString("T")}";
            string offsetMessage = $"Difference from UTC: {localTime.Offset.ToString()}";

            _logService.Info(currentMessage);
            _logService.Info(offsetMessage);
        }

        private void ConfigureHostInfo()
        {
            var sysDb = _dbManager.GetSystemDatabase();
            _hostInfo.HostGUID = sysDb.HostGUID();
            _hostInfo.HostName = sysDb.HostName();
            _hostInfo.Token = sysDb.HostToken();
            _hostInfo.DatabasePortNumber = Settings.DatabaseServicePort;

            IPAddress address;

            if (IPAddress.TryParse(Settings.IP4Adress, out address))
            {
                _hostInfo.IP4Address = address.MapToIPv4().ToString();
                _hostInfo.IP6Address = address.MapToIPv6().ToString();
            }
            else
            {
                _hostInfo.IP4Address = Settings.IP4Adress;
                _hostInfo.IP6Address = string.Empty;
            }
        }

        #endregion

    }
}

