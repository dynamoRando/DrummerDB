using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100;

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
        private IDbManager _dbManager;
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
        public bool IsStatementValid(string statement, IDbManager dbManager, DatabaseType type, ICoopActionPlanOption[] options, out string errorMessage)
        {
            string dbName = GetDatabaseName(statement);

            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentException("Unable to parse database name in statement");
            }

            return IsStatementValid(statement, dbName, dbManager, type, options, out errorMessage);
        }

        public bool IsStatementValid(string statement, string dbName, IDbManager dbManager, DatabaseType type, ICoopActionPlanOption[] options, out string errorMessage)
        {
            if (!dbManager.HasDatabase(dbName, type))
            {
                errorMessage = $"Database {dbName} was not found";
                return false;
            }
            else
            {
                // default host type
                IDatabase database = dbManager.GetDatabase(dbName, type);
                if (database is not null)
                {
                    _db = database;
                    _dbManager = dbManager;
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

                    // --- new commands

                    // will generate network communication
                    if (HasRequestParticipantKeyword(statement))
                    {
                        return ParseForRequestParticipant(statement, out errorMessage);
                    }

                    // will generate network communication
                    if (HasRequestHostKeyword(statement))
                    {
                        return ParseForRequestHostNotifyAcceptedContract(statement, out errorMessage);
                    }

                    if (HasAddParticipantKeyword(statement))
                    {
                        return ParseForAddParticipant(statement, out errorMessage);
                    }

                    if (HasReviewPendingContractsKeyword(statement))
                    {
                        return ParseForReviewPendingContracts(statement, out errorMessage);
                    }

                    if (HasAcceptContractKeyword(statement))
                    {
                        return ParseForAcceptContract(statement, out errorMessage);
                    }

                    if (HasRejectContractByKeyword(statement))
                    {
                        return ParseForRejectContract(statement, out errorMessage);
                    }

                    if (HasReviewAcceptedContractKeyword(statement))
                    {
                        return ParseForReviewAcceptedContract(statement, out errorMessage);
                    }

                    if (HasReviewHostInfoKeyword(statement))
                    {
                        return ParseForReviewHostInfo(statement, out errorMessage);
                    }

                    if (HasGenerateHostInfoKeyword(statement))
                    {
                        return ParseForGenerateHostInfo(statement, out errorMessage);
                    }
                }
            }

            errorMessage = "Unknown DrummerDB keywords.";
            return false;
        }
        #endregion

        #region Private Methods
        private bool ParseForRejectContract(string statement, out string errorMessage)
        {
            //REJECT CONTRACT BY AuthorName;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REJECT_CONTRACT_BY))
                {
                    throw new NotImplementedException();
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasRejectContractByKeyword(string statement)
        {
            //REJECT CONTRACT BY AuthorName;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REJECT_CONTRACT_BY))
                {
                    throw new NotImplementedException();

                }
            }

            return false;
        }

        private bool ParseForReviewAcceptedContract(string statement, out string errorMessage)
        {
            //REVIEW ACCEPTED CONTRACTS;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REVIEW_ACCEPTED_CONTRACTS))
                {
                    // ?
                    errorMessage = string.Empty;
                    return true;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasReviewAcceptedContractKeyword(string statement)
        {
            //REVIEW ACCEPTED CONTRACTS;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REVIEW_ACCEPTED_CONTRACTS))
                {
                    // this becomes a SELECT * FROM coop.Contracts WHERE Status = 'Accepted'
                    return true;
                }
            }

            return false;
        }

        private bool HasRequestHostKeyword(string statement)
        {
            //REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {company.Alias};
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REQUEST_HOST))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ParseForRequestHostNotifyAcceptedContract(string statement, out string errorMessage)
        {
            //REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {company.Alias};
            Guid hostGuid = Guid.Empty;

            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REQUEST_HOST_NOTIFY_ACCEPTED_CONTRACT_BY))
                {
                    // need to ensure that we actually have a host and contract with the 
                    // provided name

                    var hostName = trimmedLine.Replace(DrummerKeywords.REQUEST_HOST_NOTIFY_ACCEPTED_CONTRACT_BY + " ", string.Empty).Trim();

                    var sysDb = _dbManager.GetSystemDatabase();
                    var hostTable = sysDb.GetTable(Tables.Hosts.TABLE_NAME);

                    var hostNameValue = RowValueMaker.Create(hostTable, Tables.Hosts.Columns.HostName, hostName);
                    int resultCount = hostTable.CountOfRowsWithValue(hostNameValue);

                    if (resultCount != 1)
                    {
                        errorMessage = $"No host or multiple hosts found for Host Name {hostName}";
                        return false;
                    }
                    else
                    {
                        var hostsResults = hostTable.GetRowsWithValue(hostNameValue);

                        if (hostsResults.Count != 1)
                        {
                            errorMessage = $"No host or multiple hosts found for Host Name {hostName}";
                            return false;
                        }

                        foreach (var result in hostsResults)
                        {
                            hostGuid = Guid.Parse(result.GetValueInString(Tables.Hosts.Columns.HostGUID));
                            break;
                        }

                        var contractsTable = sysDb.GetTable(Tables.CooperativeContracts.TABLE_NAME);

                        var searchHostGuid = RowValueMaker.Create(contractsTable, Tables.CooperativeContracts.Columns.HostGuid, hostGuid.ToString());
                        var acceptedContract = RowValueMaker.Create(contractsTable, Tables.CooperativeContracts.Columns.Status, Convert.ToInt32(ContractStatus.Accepted).ToString());

                        var searchItems = new IRowValue[2];
                        searchItems[0] = searchHostGuid;
                        searchItems[1] = acceptedContract;

                        var searchResults = contractsTable.GetRowsWithAllValues(searchItems);

                        if (searchResults.Count() != 1)
                        {
                            errorMessage = $"No host or multiple hosts found for Host Name {hostName}";
                            return false;
                        }
                        else
                        {
                            errorMessage = string.Empty;
                            return true;
                        }
                    }
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool ParseForAcceptContract(string statement, out string errorMessage)
        {
            //ACCEPT CONTRACT BY AuthorName;
            //need to validate that we have a contract with that author that is pending

            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.ACCEPT_CONTRACT_BY))
                {
                    // AuthorName;
                    string author = trimmedLine.Replace(DrummerKeywords.ACCEPT_CONTRACT_BY + " ", string.Empty);

                    var systemDb = _dbManager.GetSystemDatabase();
                    // we need a table in the system database of pending contracts
                    // not just contracts that we have saved to disk as pending

                    var hostsTable = systemDb.GetTable(Tables.Hosts.TABLE_NAME);
                    var hostValue = RowValueMaker.Create(hostsTable, Tables.Hosts.Columns.HostName, author);
                    var totalValues = hostsTable.CountOfRowsWithValue(hostValue);

                    Guid hostGuid = Guid.Empty;

                    if (totalValues == 0)
                    {
                        errorMessage = $"A contract with host with name {author} was not found";
                        return false;
                    }

                    var rows = hostsTable.GetRowsWithValue(hostValue);

                    if (rows.Count != 1)
                    {
                        errorMessage = $"More than 1 or no rows found for author {author}";
                        return false;
                    }

                    foreach (var row in rows)
                    {
                        hostGuid = Guid.Parse(row.AsValueGroup().GetValueInString(Tables.Hosts.Columns.HostGUID));
                    }

                    if (hostGuid != Guid.Empty)
                    {
                        // need to lookup contracts with the host guid
                        var coopContracts = systemDb.GetTable(Tables.CooperativeContracts.TABLE_NAME);

                        // make the search items, WHERE HostGuid == <hostGuid> AND ContractStatus == Pending
                        var hostGuidValue = RowValueMaker.Create(coopContracts, Tables.CooperativeContracts.Columns.HostGuid, hostGuid.ToString());
                        var pendingContract = RowValueMaker.Create(coopContracts, Tables.CooperativeContracts.Columns.Status, Convert.ToInt32(ContractStatus.Pending).ToString());

                        var searchValues = new RowValue[2];
                        searchValues[0] = hostGuidValue;
                        searchValues[1] = pendingContract;

                        var searchResults = coopContracts.GetRowsWithAllValues(searchValues.ToArray());

                        if (searchResults.Count() == 0)
                        {
                            errorMessage = $"No pending contracts found for author {author}";
                            return false;
                        }

                        if (searchResults.Count() > 1)
                        {
                            errorMessage = $"Multiple contracts found for author {author}";
                            return false;
                        }
                    }
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasAcceptContractKeyword(string statement)
        {
            //ACCEPT CONTRACT BY AuthorName;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.ACCEPT_CONTRACT_BY))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ParseForReviewPendingContracts(string statement, out string errorMessage)
        {
            //REVIEW PENDING CONTRACTS;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REVIEW_PENDING_CONTRACTS))
                {
                    // ??
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasReviewPendingContractsKeyword(string statement)
        {
            //REVIEW PENDING CONTRACTS;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REVIEW_PENDING_CONTRACTS))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ParseForRequestParticipant(string statement, out string errorMessage)
        {
            //REQUEST PARTICIPANT ParticipantAlias SAVE CONTRACT;
            //should generate a network communication item via databases => remote database
            //we need to validate that we actually have a participant with the specified alias

            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REQUEST_PARTICIPANT))
                {
                    //ParticipantAlias SAVE CONTRACT
                    string participant = trimmedLine.Replace(DrummerKeywords.REQUEST_PARTICIPANT + " ", string.Empty).Trim();
                    //ParticipantAlias
                    string participantAlias = participant.Replace(DrummerKeywords.SAVE_CONTRACT, string.Empty).Trim();
                    // need to check to see if database actually has a participant with this alias
                    if (_db is HostDb)
                    {
                        var hostDb = _db as HostDb;
                        if (!hostDb.HasParticipantAlias(participantAlias))
                        {
                            errorMessage = $"Participant alias {participantAlias} was not found in db {hostDb.Name}";
                            return false;
                        }
                    }
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasRequestParticipantKeyword(string statement)
        {
            //REQUEST PARTICIPANT ParticipantAlias SAVE CONTRACT;
            //should generate a network communication item via databases => remote database
            // we need to validate that we actually have a participant with the specified alias

            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REQUEST_PARTICIPANT))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ParseForAddParticipant(string statement, out string errorMessage)
        {
            //ADD PARTICIPANT ParticipantAlias AT 127.0.0.1:5000;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.ADD_PARTICIPANT))
                {
                    //ParticipantAlias AT 127.0.0.1:5000
                    string participant = trimmedLine.Replace(DrummerKeywords.ADD_PARTICIPANT + " ", string.Empty).Trim();
                    var items = participant.Split(" ");

                    if (items.Count() != 3)
                    {
                        errorMessage = "Unable to parse participant alias";
                        return false;
                    }

                    string participantAlias = items[0];
                    string participantIPPort = items[2].Trim();

                    var ipItems = participantIPPort.Split(":");
                    if (ipItems.Count() != 2)
                    {
                        errorMessage = "Unable to parse participant ip address and port";
                        return false;
                    }

                    string ipAddress = ipItems[0];
                    string portNumber = ipItems[1];

                    if (!int.TryParse(portNumber, out _))
                    {
                        errorMessage = "Unable to parse participant port number";
                        return false;
                    }

                    if (!IPAddress.TryParse(ipAddress, out _))
                    {
                        errorMessage = "Unable to parse participant ip address";
                        return false;
                    }

                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool ParseForReviewHostInfo(string statement, out string errorMessage)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.GENERATE_HOST_INFO_AS_HOSTNAME))
                {
                    errorMessage = string.Empty;
                    return true;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasReviewHostInfoKeyword(string statement)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimLined = line.Trim();
                if (trimLined.StartsWith(DrummerKeywords.REVIEW_HOST_INFO))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasAddParticipantKeyword(string statement)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimLined = line.Trim();
                if (trimLined.StartsWith(DrummerKeywords.ADD_PARTICIPANT))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ParseForGenerateHostInfo(string statement, out string errorMessage)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.GENERATE_HOST_INFO_AS_HOSTNAME))
                {
                    // we need to make sure that we haven't already generated host information

                    // GENERATE HOST INFO AS HOSTNAME {contractAuthorName};

                    string hostName = trimmedLine.Replace(DrummerKeywords.GENERATE_HOST_INFO_AS_HOSTNAME, string.Empty).Trim();
                    var sysDb = _dbManager.GetSystemDatabase();
                    var hostInfo = sysDb.GetTable(Tables.HostInfo.TABLE_NAME);
                    var searchItem = RowValueMaker.Create(hostInfo, Tables.HostInfo.Columns.HostName, hostName);

                    if (hostInfo.CountOfRowsWithValue(searchItem) > 0)
                    {
                        errorMessage = $"There is already a generated host name with {hostName}";
                        return false;
                    }
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasGenerateHostInfoKeyword(string statement)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimLined = line.Trim();
                if (trimLined.StartsWith(DrummerKeywords.GENERATE_HOST_INFO_AS_HOSTNAME))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasGenerateContractKeyword(string statement)
        {
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimLined = line.Trim();
                if (trimLined.StartsWith(DrummerKeywords.GENERATE_CONTRACT_WITH_DESCRIPTION))
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
            if (_db is HostDb)
            {
                var hostDb = _db as HostDb;
                if (!hostDb.IsReadyForCooperation())
                {
                    errorMessage = $"Database {_db.Name} does not have all tables set with a logical storage policy";
                    return false;
                }
            }

            // example: GENERATE CONTRACT AS AUTHOR RetailerCorporation DESCRIPTION IntroductionMessageGoesHere;
            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.GENERATE_CONTRACT_WITH_DESCRIPTION))
                {
                    string lineAnalysis = trimmedLine;
                    string keywords = DrummerKeywords.GENERATE_CONTRACT_WITH_DESCRIPTION + " ";

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
