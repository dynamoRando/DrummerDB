using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
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
        private IQueryParser _drummerQueryParser;
        private IQueryPlanGenerator _queryPlanGenerator;
        private DrummerQueryPlanGenerator _drummerPlanGenerator;
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

            _drummerQueryParser = new DrummerQueryParser();
            _drummerPlanGenerator = new DrummerQueryPlanGenerator();
        }

        public QueryManager(DbManager dbManager, IAuthenticationManager authManager, ITransactionEntryManager entryManager, LogService log)
        {
            _dbManager = dbManager;
            _authManager = authManager;

            _statementHandler = new StatementHandler(_dbManager, log);
            _queryParser = new QueryParser(_statementHandler, log);
            _queryPlanGenerator = new QueryPlanGenerator(_statementHandler);
            _queryExecutor = new QueryExecutor(_authManager, _dbManager, entryManager, log);

            _drummerQueryParser = new DrummerQueryParser(log);
            _drummerPlanGenerator = new DrummerQueryPlanGenerator(log);

            _log = log;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Validates the sql statement is both syntatically and logically correct (that specified columns/tables exist, etc.) If it cannot,
        /// the out variable errorMessage will contain a description of errors. 
        /// </summary>
        /// <param name="sqlStatement">The sql statement to verify</param>
        /// <param name="errorMessage">A description of an error attempting to parse the query, if any</param>
        /// <returns><c>TRUE</c> if the SQL query can be executed, otherwise <c>FALSE</c></returns>
        /// <remarks>This function works on SQL statements and DrummerDB SQL statements.</remarks>
        public bool IsStatementValid(string sqlStatement, DatabaseType type, out string errorMessage)
        {
            bool isStatementValid = false;

            if (_log is not null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (ContainsDrummerKeywords(sqlStatement))
                {
                    isStatementValid = _drummerQueryParser.IsStatementValid(sqlStatement, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                }
                else
                {
                    if (ContainsCooperativeKeywords(sqlStatement))
                    {
                        var options = ParseStatementForCooperativeOptions(sqlStatement);
                        isStatementValid = _queryParser.IsStatementValid(sqlStatement, _dbManager, type, options, out errorMessage);
                    }
                    else
                    {
                        isStatementValid = _queryParser.IsStatementValid(sqlStatement, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                    }
                }
                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);

                return isStatementValid;
            }
            else
            {
                if (ContainsDrummerKeywords(sqlStatement))
                {
                    return _drummerQueryParser.IsStatementValid(sqlStatement, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                }
                else
                {
                    if (ContainsCooperativeKeywords(sqlStatement))
                    {
                        var options = ParseStatementForCooperativeOptions(sqlStatement);
                        return _drummerQueryParser.IsStatementValid(sqlStatement, _dbManager, type, options, out errorMessage);
                    }
                    else
                    {
                        return _queryParser.IsStatementValid(sqlStatement, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                    }
                }
            }
        }

        public bool IsStatementValid(string sqlStatement, out string errorMessage)
        {
            return IsStatementValid(sqlStatement, DatabaseType.Host, out errorMessage);
        }

        public bool IsStatementValid(string sqlStatement, string dbName, out string errorMessage)
        {
            return IsStatementValid(sqlStatement, dbName, DatabaseType.Host, out errorMessage);
        }

        public bool IsStatementValid(string sqlStatement, string dbName, DatabaseType type, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool isStatementValid = false;

            if (_log is not null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (ContainsDrummerKeywords(sqlStatement))
                {
                    isStatementValid = _drummerQueryParser.IsStatementValid(sqlStatement, dbName, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                }
                else
                {
                    if (ContainsCooperativeKeywords(sqlStatement))
                    {
                        var options = ParseStatementForCooperativeOptions(sqlStatement);
                        isStatementValid = _queryParser.IsStatementValid(sqlStatement, dbName, _dbManager, type, options, out errorMessage);
                    }
                    else
                    {
                        isStatementValid = _queryParser.IsStatementValid(sqlStatement, dbName, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                    }
                }

                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.Elapsed.TotalMilliseconds);
                return isStatementValid;
            }
            else
            {
                if (ContainsDrummerKeywords(sqlStatement))
                {
                    return _drummerQueryParser.IsStatementValid(sqlStatement, dbName, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                }
                else
                {
                    if (ContainsCooperativeKeywords(sqlStatement))
                    {
                        var options = ParseStatementForCooperativeOptions(sqlStatement);
                        return _queryParser.IsStatementValid(sqlStatement, dbName, _dbManager, type, options, out errorMessage);
                    }
                    else
                    {
                        return _queryParser.IsStatementValid(sqlStatement, dbName, _dbManager, type, new ICoopActionPlanOption[0], out errorMessage);
                    }
                }
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
        /// <remarks>This function works on SQL statements and DrummerDB SQL statements.</remarks>
        public Resultset ExecuteValidatedStatement(string sqlStatement, string dbName, string un, string pw, Guid userSessionId, DatabaseType type)
        {
            QueryPlan plan = GetQueryPlan(sqlStatement, dbName, type);

            if (plan is null)
            {
                throw new InvalidOperationException("Unable to generate query plan");
            }

            return ExecutePlan(plan, un, pw, userSessionId);
        }

        public Resultset ExecuteValidatedStatement(string sqlStatement, string dbName, string un, string pw, Guid userSessionId)
        {
            return ExecuteValidatedStatement(sqlStatement, dbName, un, pw, userSessionId, DatabaseType.Host);
        }

        public bool ExecuteDatabaseServiceAction(IDatabaseServiceAction action, out string errorMessage)
        {
            return _queryExecutor.ExecuteDatabaseServiceAction(action, out errorMessage);
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

        private QueryPlan GetQueryPlan(string sqlStatement, string dbName, DatabaseType type)
        {
            IDatabase db = null;
            if (!string.IsNullOrWhiteSpace(dbName))
            {
                if (_dbManager.HasDatabase(dbName, type))
                {
                    db = _dbManager.GetDatabase(dbName, type);

                    // need to see if this has drummer keywords
                    if (ContainsDrummerKeywords(sqlStatement))
                    {
                        var database = _dbManager.GetHostDatabase(dbName);
                        return _drummerPlanGenerator.GetQueryPlan(sqlStatement, database, _dbManager);
                    }
                    else
                    {
                        if (ContainsCooperativeKeywords(sqlStatement))
                        {
                            var options = ParseStatementForCooperativeOptions(sqlStatement);
                            return _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager, options);
                        }
                        else
                        {
                            return _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                        }

                    }
                }
            }
            else
            {
                // check to see if the db name was specified in the statement, if not, then default system db
                string parsedDbName = GetDatabaseName(sqlStatement);
                if (_dbManager.HasDatabase(parsedDbName, type))
                {
                    db = _dbManager.GetDatabase(parsedDbName, type);

                    // for the parser to work correctly, we need to remove the USE {dbName} statement
                    sqlStatement = RemoveUsingStatement(sqlStatement, parsedDbName);
                    QueryPlan result = null;

                    if (_log is not null)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        if (ContainsCooperativeKeywords(sqlStatement))
                        {
                            var coopOptions = ParseStatementForCooperativeOptions(sqlStatement);
                            result = _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager, coopOptions);
                        }
                        else
                        {
                            result = _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                        }

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
                    QueryPlan result = null;

                    if (_log is not null)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        // need to see if this has drummer keywords
                        if (ContainsDrummerKeywords(sqlStatement))
                        {
                            var database = _dbManager.GetHostDatabase(dbName);
                            result = _drummerPlanGenerator.GetQueryPlan(sqlStatement, database, _dbManager);
                        }
                        else
                        {
                            if (ContainsCooperativeKeywords(sqlStatement))
                            {
                                var coopOptions = ParseStatementForCooperativeOptions(sqlStatement);
                                result = _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager, coopOptions);
                            }
                            else
                            {
                                result = _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                            }
                        }

                        sw.Stop();
                        _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
                        return result;
                    }

                    if (ContainsDrummerKeywords(sqlStatement))
                    {
                        var database = _dbManager.GetHostDatabase(dbName);
                        return _drummerPlanGenerator.GetQueryPlan(sqlStatement, database, _dbManager);
                    }
                    else
                    {
                        if (ContainsCooperativeKeywords(sqlStatement))
                        {
                            var coopOptions = ParseStatementForCooperativeOptions(sqlStatement);
                            return _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager, coopOptions);
                        }
                        else
                        {
                            return _queryPlanGenerator.GetQueryPlan(sqlStatement, db, _dbManager);
                        }
                    }
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

            return databaseName;
        }

        private string RemoveUsingStatement(string statement, string dbName)
        {
            return statement.Replace(SQLGeneralKeywords.USE + $" {dbName};", string.Empty).Trim();
        }

        private bool ContainsDrummerKeywords(string statement)
        {
            return statement.Contains(DrummerKeywords.DRUMMER_BEGIN);
        }

        private bool ContainsCooperativeKeywords(string statement)
        {
            return statement.Contains(CooperativeKeywords.COOP_ACTION_FOR_PARTICIPANT);
        }

        private ICoopActionPlanOption[] ParseStatementForCooperativeOptions(string statement)
        {
            var options = new List<ICoopActionPlanOption>();

            var lines = statement.Split(";");

            foreach (var line in lines)
            {
                if (line.StartsWith(CooperativeKeywords.COOP_ACTION_FOR_PARTICIPANT))
                {
                    var trimmedLine = line.Trim();
                    var participantAlias = trimmedLine.Replace(CooperativeKeywords.COOP_ACTION_FOR_PARTICIPANT + " ", string.Empty).Trim();
                    var alias = new CoopActionOptionParticipant();
                    alias.ParticipantAlias = participantAlias;
                    alias.Text = trimmedLine;
                    options.Add(alias);
                }
            }

            return options.ToArray();
        }
        #endregion

    }
}
