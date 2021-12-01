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

                    if (HasReviewLogicalStoryagePolicyKeyword(statement))
                    {
                        return ParseForReviewLogicalStoragePolicy(statement, out errorMessage);
                    }

                    if (HasGenerateContractKeyword(statement))
                    {
                        return ParseForGenerateContract(statement, out errorMessage);
                    }
                }
            }

            errorMessage = string.Empty;
            return false;
        }
        #endregion

        #region Private Methods
        private bool HasGenerateContractKeyword(string statement)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimLined = line.Trim();
                if (trimLined.StartsWith(DrummerKeywords.GENERATE_CONTRACT_AS_AUTHOR))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ParseForGenerateContract(string statement, out string errorMessage)
        {
            // validation rule:
            // all tables in the database should have a logical storage policy
            // other than that, we just need to validate the syntax

            if (!_db.IsReadyForCooperation())
            {
                errorMessage = $"Database {_db.Name} does not have all tables set with a logical storage policy";
                return false;
            }

            // example: GENERATE CONTRACT AS AUTHOR RetailerCorporation DESCRIPTION IntroductionMessageGoesHere;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.GENERATE_CONTRACT_AS_AUTHOR))
                {
                    string lineAnalysis = trimmedLine;
                    string keywords = DrummerKeywords.GENERATE_CONTRACT_AS_AUTHOR + " ";

                    // AuthorName DESCRIPTION IntroductionMessageGoesHere
                    string authorName = lineAnalysis.Replace(keywords, string.Empty).Trim();

                    if (authorName.Contains(DrummerKeywords.DESCRIPTION))
                    {
                        // need to remove the description keyword and parse the description
                        int indexOfDescriptionKeyword = authorName.IndexOf(DrummerKeywords.DESCRIPTION + " ");
                        int lengthOfAuthorName = authorName.Length;
                        int remainingLength = lengthOfAuthorName - indexOfDescriptionKeyword;

                        // DESCRIPTION IntroductionMessageGoesHere
                        string descriptionData = authorName.Substring(indexOfDescriptionKeyword, remainingLength).Trim();

                        // AuthorName
                        authorName = authorName.Replace(descriptionData, string.Empty).Trim();

                        // IntroductionMessageGoesHere
                        descriptionData = descriptionData.Replace(DrummerKeywords.DESCRIPTION, string.Empty).Trim();

                        // ???
                        errorMessage = string.Empty;
                        return true;
                    }
                }
            }

            // ???

            errorMessage = string.Empty;
            return true;
        }

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
                    tablePolicy = tablePolicy.Replace(";", string.Empty);
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

        private bool ParseForReviewLogicalStoragePolicy(string statement, out string errorMesssage)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                if (line.StartsWith(DrummerKeywords.REVIEW_LOGICAL_STORAGE))
                {
                    string keywords = DrummerKeywords.REVIEW_LOGICAL_STORAGE + " " + DrummerKeywords.FOR;
                    var tableName = line.Replace(keywords, string.Empty).Trim();
                    tableName = tableName.Replace(";", string.Empty);

                    if (_db.HasTable(tableName))
                    {
                        errorMesssage = string.Empty;
                        return true;
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

        private bool HasReviewLogicalStoryagePolicyKeyword(string statement)
        {
            return statement.Contains(DrummerKeywords.REVIEW_LOGICAL_STORAGE);
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
