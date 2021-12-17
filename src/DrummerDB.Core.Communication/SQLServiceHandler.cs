using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Diagnostics;

namespace Drummersoft.DrummerDB.Core.Communication
{
    /// <summary>
    /// Provides network actions for handling SQL statements to be executed against the database system. For more information, see INetworkManager.md
    /// </summary>
    internal class SQLServiceHandler
    {
        #region Private Fields
        private IAuthenticationManager _auth;
        private IDbManager _db;
        private QueryManager _query;
        private LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        /// <summary>
        /// Writes method call to <see cref="Debug.WriteLine(string?)"/>
        /// </summary>
        public void HandleTest()
        {
            Debug.WriteLine("SQL Service Handler Handles Test");
        }

        internal bool UserHasRights(string userName, string pw)
        {
            return _auth.ValidateLogin(userName, pw);
        }

        internal bool IsValidQuery(string sqlStatement, string userName, string pw, DatabaseType type, out string errorMessage)
        {
            return _query.IsStatementValid(sqlStatement, type, out errorMessage);
        }

        internal bool IsValidQuery(string sqlStatement, string userName, string pw, string databaseName, DatabaseType type, out string errorMessage)
        {
            return _query.IsStatementValid(sqlStatement, databaseName, type, out errorMessage);
        }

        internal Resultset ExecuteQuery(string sqlStatement, string userName, string pw, string dbName, Guid userSessionId, DatabaseType type)
        {
            return _query.ExecuteValidatedStatement(sqlStatement, dbName, userName, pw, userSessionId, type);
        }

        public void SetAuthentication(IAuthenticationManager authentication)
        {
            _auth = authentication;
        }

        public void SetQueryManager(IQueryManager query)
        {
            _query = query as QueryManager;
        }

        public void SetDbManager(IDbManager db)
        {
            _db = db;
        }

        public void SetLogService(LogService logService)
        {
            _log = logService;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
