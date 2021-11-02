using Drummersoft.DrummerDB.Core.Communication.Interface;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using System;

namespace Drummersoft.DrummerDB.Core.Communication
{
    /// <summary>
    /// Provides network services for the database system. For more information, see INetworkManager.md
    /// </summary>
    internal class NetworkManager : INetworkManager
    {
        #region Private Fields
        // managers
        private IAuthenticationManager _authManager;
        private IQueryManager _queryManager;
        private IDbManager _dbManager;
        private SQLServiceServer _sqlServiceServer;
        private InfoServiceServer _infoServiceServer;
        private DatabaseServiceServer _databaseServiceServer;
        private LogService _logService;

        // internal objects
        private SQLServiceHandler _sqlServiceHandler;
        private InfoServiceHandler _infoServiceHandler;
        private DatabaseServiceHandler _databaseServiceHandler;
        private PortSettings _sqlServicePort;
        private PortSettings _databaseServicePort;
        private PortSettings _infoServicePort;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public NetworkManager(PortSettings databaseServiceSettings, PortSettings sqlServiceSettings, PortSettings infoServiceSettings, IQueryManager queryManager, IDbManager dbManager, LogService logService)
        {
            _queryManager = queryManager;
            _dbManager = dbManager;

            _sqlServicePort = sqlServiceSettings;
            _databaseServicePort = databaseServiceSettings;
            _infoServicePort = infoServiceSettings;
            _logService = logService;
        }
        #endregion

        #region Public Methods
        public void StartServerForSQLService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, int portNumber, IQueryManager queryManager)
        {
            _sqlServicePort.PortNumber = portNumber;
            StartServerForSQLService(useHttps, authenticationManager, dbManager, queryManager);
        }

        public void StopServerForSQLService()
        {
            if (_sqlServiceServer is not null)
            {
                _sqlServiceServer.StopAsync();
            }
        }

        public void StartServerForSQLService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, IQueryManager queryManager)
        {
            string clientUrl;

            if (_sqlServiceHandler is null)
            {
                _sqlServiceHandler = new SQLServiceHandler();
                _sqlServiceHandler.SetQueryManager(queryManager);
                _sqlServiceHandler.SetDbManager(dbManager);
                _sqlServiceHandler.SetAuthentication(authenticationManager);

                if (_logService is not null)
                {
                    _sqlServiceHandler.SetLogService(_logService);
                }
            }

            if (useHttps)
            {
                clientUrl = $"https://{_sqlServicePort.IPAddress}:{_sqlServicePort.PortNumber.ToString()}";
            }
            else
            {
                clientUrl = $"http://{_sqlServicePort.IPAddress}:{_sqlServicePort.PortNumber.ToString()}";
            }

            string[] urls = new string[1];
            urls[0] = clientUrl;

            if (_sqlServiceServer is null)
            {
                _sqlServiceServer = new SQLServiceServer();
            }

            _logService.Info($"SQLService endpoint started at: {clientUrl}");

            _sqlServiceServer.RunAsync(null, urls, _sqlServiceHandler, _sqlServicePort);
        }

        public void StartServerForInfoService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, int portNumber)
        {
            _infoServicePort.PortNumber = portNumber;
            StartServerForInfoService(useHttps, authenticationManager, dbManager);
        }

        public void StopServerForInfoService()
        {
            if (_infoServiceServer is not null)
            {
                _infoServiceServer.StopAsync();
            }
        }

        public void StartServerForInfoService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager)
        {
            string clientUrl;

            if (_infoServiceHandler is null)
            {
                _infoServiceHandler = new InfoServiceHandler();
            }

            if (useHttps)
            {
                clientUrl = $"https://{_infoServicePort.IPAddress}:{_infoServicePort.PortNumber.ToString()}";
            }
            else
            {
                clientUrl = $"http://{_infoServicePort.IPAddress}:{_infoServicePort.PortNumber.ToString()}";
            }

            string[] urls = new string[1];
            urls[0] = clientUrl;

            if (_infoServiceServer is null)
            {
                _infoServiceServer = new InfoServiceServer();
            }

            _logService.Info($"InfoService endpoint started at: {clientUrl}");

            _infoServiceServer.RunAsync(null, urls, _infoServiceHandler, _infoServicePort);
        }

        /// <summary>
        /// Starts the Database Service with the supplied parameters
        /// </summary>
        /// <param name="useHttps">If the connection should use HTTPS or not</param>
        /// <param name="authenticationManager">An instance of an auth manager</param>
        /// <param name="dbManager">An instance of a db manager</param>
        /// <param name="cache">An instance of a cache manager</param>
        /// <param name="crypt">An instance of a crypt manager</param>
        /// <remarks>The managers passed in are normally used in the creation of a new database</remarks>
        public void StartServerForDatabaseService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager)
        {
            string clientUrl;

            if (_databaseServiceHandler is null)
            {
                // this is a hack to try and avoid registering every single type with dependency injection
                // will need to research this again later. This is the result of the call from DrummerDB.Core.Service
                //_databaseServiceHandler = new DatabaseServiceHandler(authenticationManager, dbManager);

                /*
                     Message=Some services are not able to be constructed (Error while validating the service descriptor 'ServiceType: Drummersoft.DrummerDB.Core.Communication.DatabaseServiceHandler Lifetime: 
                     Singleton ImplementationType: Drummersoft.DrummerDB.Core.Communication.DatabaseServiceHandler': 
                     Unable to resolve service for type 'Drummersoft.DrummerDB.Core.IdentityAccess.Interface.IAuthenticationManager' while 
                     attempting to activate 'Drummersoft.DrummerDB.Core.Communication.DatabaseServiceHandler'.)
                     Source=Microsoft.Extensions.DependencyInjection

                     Inner Exception 1:
                     InvalidOperationException: Error while validating the service descriptor 
                     'ServiceType: Drummersoft.DrummerDB.Core.Communication.DatabaseServiceHandler Lifetime: Singleton ImplementationType: 
                     Drummersoft.DrummerDB.Core.Communication.DatabaseServiceHandler': Unable to resolve service for 
                     type 'Drummersoft.DrummerDB.Core.IdentityAccess.Interface.IAuthenticationManager' while attempting to activate 
                     'Drummersoft.DrummerDB.Core.Communication.DatabaseServiceHandler'.

                     Inner Exception 2:
                     InvalidOperationException: Unable to resolve service for type 'Drummersoft.DrummerDB.Core.IdentityAccess.Interface.IAuthenticationManager' 
                     while attempting to activate 'Drummersoft.DrummerDB.Core.Communication.DatabaseServiceHandler'.

                 */

                _databaseServiceHandler = new DatabaseServiceHandler();
                _databaseServiceHandler.SetAuth(authenticationManager);
                _databaseServiceHandler.SetDatabase(dbManager);
            }

            if (useHttps)
            {
                clientUrl = $"https://{_databaseServicePort.IPAddress}:{_databaseServicePort.PortNumber.ToString()}";
            }
            else
            {
                clientUrl = $"http://{_databaseServicePort.IPAddress}:{_databaseServicePort.PortNumber.ToString()}";
            }

            string[] urls = new string[1];
            urls[0] = clientUrl;

            if (_databaseServiceServer is null)
            {
                _databaseServiceServer = new DatabaseServiceServer();
            }


            _logService.Info($"DatabaseService endpoint started at: {clientUrl}");
            _databaseServiceServer.RunAsync(null, urls, _databaseServiceHandler, _databaseServicePort);
        }

        public void StopServerForDatabaseService()
        {
            if (_databaseServiceServer is not null)
            {
                _databaseServiceServer.StopAsync();
            }
        }

        /// <summary>
        /// Starts the Database Service with the supplied parameters. Overrides the port number from what was loaded in settings.
        /// </summary>
        /// <param name="useHttps">If the connection should use HTTPS or not</param>
        /// <param name="authenticationManager">An instnace of an auth manager</param>
        /// <param name="dbManager">An instnace of a db manager</param>
        /// <param name="cache">An instance of a cache manager</param>
        /// <param name="crypt">An instance of a crypt manager</param>
        /// <param name="portNumber">The port number the server should listen on. This value overrides what is in the settings file.</param>
        public void StartServerForDatabaseService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, int portNumber)
        {
            _databaseServicePort.PortNumber = portNumber;
            StartServerForDatabaseService(useHttps, authenticationManager, dbManager);
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
