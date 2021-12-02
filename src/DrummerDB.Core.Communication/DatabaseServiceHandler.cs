using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Diagnostics;

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
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public DatabaseServiceHandler() { }

        public DatabaseServiceHandler(IAuthenticationManager authenticationManager, IDbManager dbManager, IStorageManager storageManager)
        {
            _authenticationManager = authenticationManager;
            _dbManager = dbManager;
            _storageManager = storageManager;
        }
        #endregion

        #region Public Methods
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

        public bool UserHasSystemPermission(string userName, SystemPermission permission)
        {
            return _authenticationManager.UserHasSystemPermission(userName, permission);
        }

        public bool CreateUserDatabase(string databaseName, out Guid databaseId)
        {
            var manager = _dbManager as DbManager;
            return manager.TryCreateNewHostDatabase(databaseName, out databaseId);
        }

        public bool SaveContract(Contract contract)
        {
            return _storageManager.SaveContractToDisk(contract);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
