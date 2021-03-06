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
using NLog.Config;
using NLog.Targets;
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
        private SystemNotifications _notifications;

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
            _notifications = new SystemNotifications();
        }

        /// <summary>
        /// Constructs a Process with the specified storage folder path. This is used for testing purposes only and will override the default setting in <see cref="Settings.DatabaseFolder"/>
        /// </summary>
        /// <param name="storageFolderPath">The path to the appropriate storage folder</param>
        public Process(string storageFolderPath)
        {
            _storageFolder = storageFolderPath;
            _notifications = new SystemNotifications();
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
            _notifications = new SystemNotifications();
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
            _notifications = new SystemNotifications();
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
            RegisterForHostInfoChanges();
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
            _dbManager = new DbManager(_xEntryManager, _logService, _notifications);
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
            _network = new NetworkManager(databasePort, sqlPort, infoPort, _queries, _dbManager, _logService, _hostInfo, _notifications);
        }

        private void SetupAuth()
        {
            _auth = new IdentityAccess.AuthenticationManager(_dbManager);
        }

        private void SetupCrypt()
        {
            _crypt = new CryptoManager();
        }

        private void RegisterForHostInfoChanges()
        {
            _notifications.HostInfoUpdated += HandleUpdatedHostInfo;
        }

        private void HandleUpdatedHostInfo(object sender, EventArgs e)
        {
            if (e is HostUpdatedEventArgs)
            {
                var args = e as HostUpdatedEventArgs;
                _hostInfo = args.HostInfo;
            }
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
            string fullPath = string.Empty;

            if (!string.IsNullOrEmpty(_storageFolder))
            {
                fullPath = Path.Combine(_storageFolder, Settings.LogFileName);
            }
            else
            {
                fullPath = Path.Combine(Settings.DatabaseFolder, Settings.LogFileName);
            }

            var logger = CreateCustomLogger(Guid.NewGuid().ToString(), fullPath);

            _logService = new LogService(logger, Settings.EnableLogging, Settings.LogPerformanceMetrics);
            _logService.Info("DrummerDB started");

            DateTimeOffset localTime = DateTimeOffset.Now;
            DateTimeOffset utcTime = DateTimeOffset.UtcNow;

            string currentMessage = $"Local Time: {localTime.ToString("T")}";
            string offsetMessage = $"Difference from UTC: {localTime.Offset.ToString()}";

            _logService.Info(currentMessage);
            _logService.Info(offsetMessage);

            if (!string.IsNullOrEmpty(_storageFolder))
            {
                _logService.Info($"Database Folder: {_storageFolder}");
            }
            else
            {
                _logService.Info($"Database Folder: {Settings.DatabaseFolder}");
            }
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

        // kept here as an example only
        // https://stackoverflow.com/questions/20352325/logging-in-multiple-files-using-nlog
        /// <summary>
        /// Create Custom Logger using parameters passed.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="LogEntryLayout">Give "" if you want just message. If omited will switch to full log paramaters.</param>
        /// <param name="logFileLayout">Filename only. No extension or file paths accepted.</param>
        /// <param name="absoluteFilePath">If you want to save the log file to different path thatn application default log path, specify the path here.</param>
        /// <returns>New instance of NLog logger completly isolated from default instance if any</returns>
        public static Logger CreateCustomLogger(string name = "CustomLog",
            string LogEntryLayout = "${ date:format=dd.MM.yyyy HH\\:mm\\:ss.fff} thread[${threadid}] ${logger} (${level:uppercase=true}): ${message}. ${exception:format=ToString}",
            string logFileLayout = "logs/{0}.${{shortdate}}.log",
            string absoluteFilePath = "")
        {
            var factory = new LogFactory();
            var target = new FileTarget();
            target.Name = name;
            if (absoluteFilePath == "")
                target.FileName = string.Format(logFileLayout, name);
            else
                target.FileName = string.Format(absoluteFilePath + "//" + logFileLayout, name);
            if (LogEntryLayout == "") //if user specifes "" then use default layout.
                target.Layout = "${message}. ${exception:format=ToString}";
            else
                target.Layout = LogEntryLayout;
            var defaultconfig = LogManager.Configuration;
            var config = new LoggingConfiguration();
            config.AddTarget(name, target);

            var ruleInfo = new LoggingRule("*", NLog.LogLevel.Trace, target);

            config.LoggingRules.Add(ruleInfo);

            factory.Configuration = config;

            return factory.GetCurrentClassLogger();
        }

        public Logger CreateCustomLogger(string loggerName, string filePath)
        {
            var factory = new LogFactory();
            var target = new FileTarget();
            target.FileName = filePath;
            target.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} - ${message} ${exception:format=ToString}";

            var defaultconfig = LogManager.Configuration;
            var config = new LoggingConfiguration();
            config.AddTarget(loggerName, target);

            var ruleInfo = new LoggingRule("*", NLog.LogLevel.Trace, target);

            config.LoggingRules.Add(ruleInfo);

            factory.Configuration = config;

            return factory.GetCurrentClassLogger();
        }

        #endregion

    }
}

