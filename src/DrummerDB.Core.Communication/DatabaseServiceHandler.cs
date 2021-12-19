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

        public bool InsertRowIntoTable(Row row, Guid dbId, string dbName, string tableName)
        {
            throw new NotImplementedException();
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

            stringBuilder.Append($"DrummerDB.Core.Communication - DatabaseServieHandler: Received Message {type}. ");
            stringBuilder.Append($"Message Id {id}");
            stringBuilder.Append($"Message UTC Generated: {messageGeneratedTimeUTC}");
            stringBuilder.Append($"IsLittleEndian: {isLittleEndian}");
            foreach (var address in ipAddresses)
            {
                stringBuilder.Append($"Message address: {address}");
            }

            _logger.Info(stringBuilder.ToString());
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
