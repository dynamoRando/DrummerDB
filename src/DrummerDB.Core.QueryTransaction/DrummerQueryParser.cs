using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Validates statements related to Drummer specific keywords (i.e. DRUMMER BEGIN/DRUMMER END)
    /// </summary>
    internal class DrummerQueryParser : IQueryParser
    {
        #region Private Fields
        private LogService _log;
        private IDatabase _db;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public DrummerQueryParser()
        {
        }

        public DrummerQueryParser(LogService log)
        {
            _log = log;
        }
        #endregion

        #region Public Methods
        public bool IsStatementValid(string statement, IDbManager dbManager, out string errorMessage)
        {
            string dbName = GetDatabaseName(statement);

            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentException("Unable to parse database name in statement"); 
            }

            return IsStatementValid(statement, dbName, dbManager, out errorMessage);
        }

        public bool IsStatementValid(string statement, string dbName, IDbManager dbManager, out string errorMessage)
        {
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
                    _db = database;
                    if (HasLogicalStoragePolicyKeyword(statement))
                    {
                        return ParseForLogicalStoragePolicy(statement, out errorMessage);
                    }
                }
            }

            errorMessage = string.Empty;
            return false;
        }
        #endregion

        #region Private Methods
        private bool HasLogicalStoragePolicyKeyword(string statement)
        {
            return statement.Contains(DrummerKeywords.SET_LOGICAL_STORAGE);
        }

        private bool ParseForLogicalStoragePolicy(string statement, out string errorMesssage)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                if (line.StartsWith(DrummerKeywords.SET_LOGICAL_STORAGE))
                {
                    string keywords = DrummerKeywords.SET_LOGICAL_STORAGE + " " + DrummerKeywords.FOR;
                    var tablePolicy = line.Replace(keywords, string.Empty).Trim();
                    var items = tablePolicy.Split(" ");
                    string tableName = items[0];
                    string policy = items[1];

                    if (_db.HasTable(tableName))
                    {
                        if (DrummerKeywords.
                            LogicalStoragePolicyKeywords.
                            StoragePolicies.
                            Any(item => string.Equals(item, policy, StringComparison.OrdinalIgnoreCase)))
                        {
                        }
                        else
                        {
                            errorMesssage = $"Storage Policy {policy} is not known";
                            return false;
                        }
                    }
                    else
                    {
                        errorMesssage = $"Table {tableName} was not found";
                        return false;
                    }
                }
            }

            errorMesssage = string.Empty;
            return true;
        }

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
        #endregion

    }
}
