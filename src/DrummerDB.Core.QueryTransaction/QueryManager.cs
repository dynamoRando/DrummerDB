using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Diagnostics;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class QueryManager : IQueryManager
    {
        #region Private Fields
        // managers
        private DbManager _dbManager;
        private IAuthenticationManager _authManager;
        private StatementHandler _statementHandler;

        // internal objects
        private IQueryExecutor _queryExecutor;
        private IQueryParser _queryParser;
        private IQueryPlanGenerator _queryPlanGenerator;
        private LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public QueryManager(DbManager dbManager, IAuthenticationManager authManager, ITransactionEntryManager entryManager)
        {
            _dbManager = dbManager;
            _authManager = authManager;

            _statementHandler = new StatementHandler(_dbManager);
            _queryParser = new QueryParser(_statementHandler);
            _queryPlanGenerator = new QueryPlanGenerator(_statementHandler);
            _queryExecutor = new QueryExecutor(_authManager, _dbManager, entryManager);
        }

        public QueryManager(DbManager dbManager, IAuthenticationManager authManager, ITransactionEntryManager entryManager, LogService log)
        {
            _dbManager = dbManager;
            _authManager = authManager;

            _statementHandler = new StatementHandler(_dbManager, log);
            _queryParser = new QueryParser(_statementHandler);
            _queryPlanGenerator = new QueryPlanGenerator(_statementHandler);
            _queryExecutor = new QueryExecutor(_authManager, _dbManager, entryManager, log);

            _log = log;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Validates the sql statement is both syntatically and logically correct (that specified columns/tables exist, etc.) If it cannot,
        /// the out variable errorMessage will contain a description of errors
        /// </summary>
        /// <param name="sqlStatement">The sql statement to verify</param>
        /// <param name="errorMessage">A description of an error attempting to parse the query, if any</param>
        /// <returns><c>TRUE</c> if the SQL query can be executed, otherwise <c>FALSE</c></returns>
        public bool IsStatementValid(string sqlStatement, out string errorMessage)
        {
            if (_log is not null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var result = _queryParser.IsStatementValid(sqlStatement, _dbManager, out errorMessage);
                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
                return result;
            }
            else
            {
                return _queryParser.IsStatementValid(sqlStatement, _dbManager, out errorMessage);
            }

        }

        public bool IsStatementValid(string sqlStatement, string dbName, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (_log is not null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var result = _queryParser.IsStatementValid(sqlStatement, dbName, _dbManager, out errorMessage);
                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.Elapsed.TotalMilliseconds);
                return result;
            }
            else
            {
                return _queryParser.IsStatementValid(sqlStatement, dbName, _dbManager, out errorMessage);
            }
        }

        /// <summary>
        /// Executes a previously validated sql statement and returns it's results
        /// </summary>
        /// <param name="sqlStatement">The sql statement to execute</param>
        /// <param name="dbName">The name of the database to execute the statement against</param>
        /// <param name="un">The user requesting to execute the query</param>
        /// <param name="pw">The user's pw</param>
        /// <param name="userSessionId">The session id of the user (used for open transactions)</param>
        /// <returns>The results of the sql statement</returns>
        public Resultset ExecuteValidatedStatement(string sqlStatement, string dbName, string un, string pw, Guid userSessionId)
        {
            QueryPlan plan = GetQueryPlan(sqlStatement, dbName);

            if (plan is null)
            {
                throw new InvalidOperationException("Unable to generate query plan");
            }

            return ExecutePlan(plan, un, pw, userSessionId);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Executes the supplied <see cref="QueryPlan"/> in a synchronous manner
        /// </summary>
        /// <param name="queryPlan">The plan to execute</param>
        /// <param name="userName">The user executing the plan</param>
        /// <param name="pw">The user's pw</param>
        /// <returns>The <see cref="Resultset"/> for the supplied plan</returns>
        private Resultset ExecutePlan(QueryPlan queryPlan, string userName, string pw, Guid userSessionId)
        {
            return _queryExecutor.ExecutePlanAsync(queryPlan, userName, pw, userSessionId).Result;
        }

        private QueryPlan GetQueryPlan(string sqlStatement, string dbName)
        {
            IDatabase db = null;
            if (!string.IsNullOrWhiteSpace(dbName))
            {
                if (_dbManager.HasDatabase(dbName))
                {
                    db = _dbManager.GetDatabase(dbName);
                    return _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                }
            }
            else
            {
                // check to see if the db name was specified in the statement, if not, then default system db
                string parsedDbName = GetDatabaseName(sqlStatement);
                if (_dbManager.HasDatabase(parsedDbName))
                {
                    db = _dbManager.GetDatabase(parsedDbName);

                    // for the parser to work correctly, we need to remove the USE {dbName} statement
                    sqlStatement = RemoveUsingStatement(sqlStatement, parsedDbName);


                    if (_log is not null)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        var result = _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                        sw.Stop();
                        _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
                        return result;

                    }
                    return _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);

                }
                else
                {
                    // default to system database if the user database was not supplied. This usually happens when the user is creating a new database
                    db = _dbManager.GetSystemDatabase();

                    if (_log is not null)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        var result = _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                        sw.Stop();
                        _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
                        return result;

                    }

                    return _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to parse the database name from the statement, looking for the keyword "USE [DatabaseName]"
        /// </summary>
        /// <param name="input">The SQL statement to parse</param>
        /// <returns>The database name</returns>
        private string GetDatabaseName(string input)
        {
            string databaseName = string.Empty;

            if (input.Contains($"{SQLGeneralKeywords.USE} "))
            {
                var items = input.Split(";");
                var words = items[0].Trim().Split(" ");
                databaseName = words[1];
            }

            /*
            if (input.Contains($"{DDLKeywords.CREATE} {SQLGeneralKeywords.DATABASE} "))
            {
                databaseName = input.Replace($"{DDLKeywords.CREATE} {SQLGeneralKeywords.DATABASE} ", string.Empty).Trim();
            }
            */

            return databaseName;
        }

        private string RemoveUsingStatement(string statement, string dbName)
        {
            return statement.Replace(SQLGeneralKeywords.USE + $" {dbName};", string.Empty).Trim();
        }
        #endregion

    }
}
