using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Evaluates a SQL statement for validity - ensures the syntax is correct and that the listed objects exist in the target database
    /// </summary>
    internal class QueryParser : IQueryParser
    {
        #region Private Fields
        private StatementHandler _statementHandler;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public QueryParser(StatementHandler statementHandler)
        {
            _statementHandler = statementHandler;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Attempts to validate that the SQL statement is valid against database objects. Assumes the USE "DatabaseName" keyword is part of the statement.
        /// </summary>
        /// <param name="statement">The SQL statemnt to validate</param>
        /// <param name="dbManager">The db manager</param>
        /// <param name="errorMessage">Outputs any error messages occured in validating the statement</param>
        /// <returns><c>TRUE</c> if the database can execute the statement, otherise <c>FALSE</c></returns>
        /// <exception cref="ArgumentException">Thrown when the Database Name cannot be parsed from the statement</exception>
        public bool IsStatementValid(string statement, IDbManager dbManager, out string errorMessage)
        {
            string dbName = GetDatabaseName(statement);

            if (string.IsNullOrEmpty(dbName))
            {
                // throw new ArgumentException("Unable to parse database name in statement");
                dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;
            }

            if (!statement.Contains($"{DDLKeywords.CREATE} {SQLGeneralKeywords.DATABASE} "))
            {
                if (!dbManager.HasDatabase(dbName))
                {
                    errorMessage = $"Database {dbName} was not found";
                    return false;
                }
                else
                {
                    IDatabase database = dbManager.GetDatabase(dbName);
                    return WalkStatementForValidity(statement, database, out errorMessage);
                }
            }
            else
            {
                errorMessage = string.Empty;
                return true;
            }
        }

        public bool IsStatementValid(string statement, string dbName, IDbManager dbManager, out string errorMessage)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentException("Unable to parse database name in statement");
            }

            if (!dbManager.HasDatabase(dbName))
            {
                errorMessage = $"Database {dbName} was not found";
                return false;
            }
            else
            {
                IDatabase database = dbManager.GetDatabase(dbName);
                if (database is not null)
                {
                    return WalkStatementForValidity(statement, database, out errorMessage);
                }
            }

            errorMessage = string.Empty;
            return false;
        }

        /// <summary>
        /// Attempts to validate that the SQL statement is valid against database objects.
        /// </summary>
        /// <param name="statement">The SQL statemnt to validate</param>
        /// <param name="dbManager">The db manager</param>
        /// <param name="dbName">The name of the db to check against</param>
        /// <param name="errorMessage">Outputs any error messages occured in validating the statement</param>
        /// <returns><c>TRUE</c> if the database can execute the statement, otherise <c>FALSE</c></returns>
        public bool IsStatementValid(string statement, IDbManager dbManager, string dbName, out string errorMessage)
        {
            if (!dbManager.HasDatabase(dbName))
            {
                errorMessage = $"Database {dbName} was not found";
                return false;
            }
            else
            {
                return WalkStatementForValidity(statement, dbManager.GetDatabase(dbName), out errorMessage);
            }
        }

        /// <summary>
        /// Determines if the SQL statement is a DML statement (Data Manipulation Language) or DDL (Data Definition Langugage) statement by looking for keywords.
        /// </summary>
        /// <param name="statement">The statement to parse</param>
        /// <returns>The type of statement (DML or DDL)</returns>
        public static StatementType DetermineStatementType(string statement)
        {
            var ddlKeywords = DDLKeywords.Get();
            var statementItems = statement.Split(" ");

            foreach (var word in statementItems)
            {
                if (ddlKeywords.Any(keyword => string.Equals(keyword, word, StringComparison.OrdinalIgnoreCase)))
                {
                    return StatementType.DDL;
                }
            }

            return StatementType.DML;
        }
        #endregion

        #region Private Methods
        private bool WalkStatementForValidity(string statement, IDatabase database, out string errorMessage)
        {
            StatementType type;
            errorMessage = string.Empty;
            statement = RemoveUsingStatement(statement, database.Name);

            // the Antlr generated parser needs to take the SQL statement in uppercase
            //statement = statement.ToUpper();
            type = DetermineStatementType(statement);

            StatementReport report = _statementHandler.ParseStatementForValiditity(statement, database, type);
            foreach (var error in report.Errors)
            {
                errorMessage += error + Environment.NewLine;
            }

            return report.IsValid;
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

            if (input.Contains($"{DDLKeywords.CREATE} {SQLGeneralKeywords.DATABASE} "))
            {
                databaseName = input.Replace($"{DDLKeywords.CREATE} {SQLGeneralKeywords.DATABASE} ", string.Empty).Trim();
            }

            return databaseName;
        }

        private string RemoveUsingStatement(string statement, string dbName)
        {
            return statement.Replace(SQLGeneralKeywords.USE + $" {dbName};", string.Empty).Trim();
        }

        #endregion

    }
}
