using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Tables;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class DrummerQueryPlanGenerator
    {
        #region Private Fields
        private LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public DrummerQueryPlanGenerator()
        {
        }

        public DrummerQueryPlanGenerator(LogService log)
        {
            _log = log;
        }
        #endregion

        #region Public Methods
        public QueryPlan GetQueryPlan(string statement, HostDb database, IDbManager dbManager)
        {
            QueryPlan plan = new QueryPlan(statement);

            var lines = statement.Split(";");
            foreach (var line in lines)
            {
                string trimLine = line.Trim();
                EvaluateLine(trimLine, database, dbManager, ref plan);
            }

            return plan;
        }
        #endregion

        #region Private Methods
        private void EvaluateLine(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            EvaluateForLogicalStoragePolicy(line, database, dbManager, ref plan);
            EvaluateForReviewLogicalStoragePolicy(line, database, dbManager, ref plan);
        }

        private void EvaluateForGenerateContract(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            // example:
            // GENERATE CONTRACT AS AUTHOR RetailerCorporation DESCRIPTION IntroductionMessageGoesHere;
            // this data should be inserted into the sys.DatabaseContracts table in the database
            // once the contract has been generated, all the records in sys.UserTables should be updated with the new
            // contract GUID
            string authorName = string.Empty;
            string descriptionData = string.Empty;
            Guid contractGuid = Guid.Empty;

            if (database.IsReadyForCooperation())
            {
                if (line.StartsWith(DrummerKeywords.GENERATE_CONTRACT_AS_AUTHOR))
                {
                    string lineAnalysis = line;
                    string keywords = DrummerKeywords.GENERATE_CONTRACT_AS_AUTHOR + " ";

                    // AuthorName DESCRIPTION IntroductionMessageGoesHere
                    authorName = lineAnalysis.Replace(keywords, string.Empty).Trim();

                    if (authorName.Contains(DrummerKeywords.DESCRIPTION))
                    {
                        // need to remove the description keyword and parse the description
                        int indexOfDescriptionKeyword = authorName.IndexOf(DrummerKeywords.DESCRIPTION + " ");
                        int lengthOfAuthorName = authorName.Length;
                        int remainingLength = lengthOfAuthorName - indexOfDescriptionKeyword;

                        // DESCRIPTION IntroductionMessageGoesHere
                        descriptionData = authorName.Substring(indexOfDescriptionKeyword, remainingLength).Trim();

                        // AuthorName
                        authorName = authorName.Replace(descriptionData, string.Empty).Trim();

                        // IntroductionMessageGoesHere
                        descriptionData = descriptionData.Replace(DrummerKeywords.DESCRIPTION, string.Empty).Trim();
                    }
                }

                // create an insert table operation for sys.DatabaseContracts
                // and then an update table operation for sys.UserTables
                if (!plan.Parts.Any(part => part is InsertQueryPlanPart))
                {
                    plan.Parts.Add(new InsertQueryPlanPart());
                }

                foreach (var part in plan.Parts)
                {
                    if (part is InsertQueryPlanPart)
                    {
                        var insertDatabaseContractsOp = new InsertTableOperator(dbManager);
                        insertDatabaseContractsOp.TableName = DatabaseContracts.TABLE_NAME;

                        contractGuid = Guid.NewGuid();

                        var contractGuidColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.ContractGUID);
                        var generatedDateColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.GeneratedDate);
                        var authorColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.Author);
                        var tokenColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.Token);
                        var descriptionColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.Description);

                        var contractGuidStatement = new StatementColumn(contractGuidColumn.Id, contractGuidColumn.Name);
                        insertDatabaseContractsOp.Columns.Add(contractGuidStatement);

                        // need to create a row to insert
                        var insertRow = new InsertRow(1);

                        var insertValueContractGuid = new InsertValue(1, contractGuidColumn.Name, contractGuid.ToString());
                        var insertValueGeneratedDate = new InsertValue(2, generatedDateColumn.Name, DateTime.Now.ToString());
                        var insertValueAuthor = new InsertValue(3, authorColumn.Name, authorName);

                        var tokenString = CryptoManager.GenerateTokenString();
                        var insertValueToken = new InsertValue(4, tokenColumn.Name, tokenString);

                        var insertValueDescription = new InsertValue(5, descriptionColumn.Name, descriptionData);

                        insertRow.Values.Add(insertValueContractGuid);
                        insertRow.Values.Add(insertValueGeneratedDate);
                        insertRow.Values.Add(insertValueAuthor);
                        insertRow.Values.Add(insertValueToken);
                        insertRow.Values.Add(insertValueDescription);

                        insertDatabaseContractsOp.Rows.Add(insertRow);

                        part.Operations.Add(insertDatabaseContractsOp);
                    }
                }

                // need to update all rows in the table with the generated contract value
                if (!plan.Parts.Any(part => part is UpdateQueryPlanPart))
                {
                    plan.Parts.Add(new UpdateQueryPlanPart());
                }

                foreach (var part in plan.Parts)
                {
                    if (part is UpdateQueryPlanPart)
                    {
                        var address = new TreeAddress { DatabaseId = database.Id, TableId = UserTable.TABLE_ID, SchemaId = Guid.Parse(Constants.SYS_SCHEMA_GUID) };
                        // need to create update column sources

                        var columns = new List<IUpdateColumnSource>();

                        // create value object that we're going to update the contract guid to
                        var column = new UpdateTableValue();
                        column.Column = new StatementColumn(UserTable.TABLE_ID, UserTable.TABLE_NAME);
                        column.Value = contractGuid.ToString();

                        columns.Add(column);
                        var updateOp = new UpdateOperator(dbManager, address, columns);

                        // we need to create a read table operator to specify to update all the columns in the user table with the contract
                        // and set it as the previous operation

                        // only reading 1 column from the table that we want to update, the contract GUID column in sys.UserTables
                        string[] colNames = new string[1] { UserTable.Columns.ContractGUID };

                        var readTableOp = new TableReadOperator(dbManager, address, colNames, _log);
                        updateOp.PreviousOperation = readTableOp;

                        part.Operations.Add(updateOp);
                    }
                }
            }

            throw new NotImplementedException();
        }

        private void EvaluateForReviewLogicalStoragePolicy(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            // example:
            //REVIEW LOGICAL STORAGE FOR Products;
            string keywords = DrummerKeywords.REVIEW_LOGICAL_STORAGE + " " + DrummerKeywords.FOR;
            if (line.StartsWith(keywords))
            {
                string tableName = line.Replace(keywords, string.Empty).Trim();
                tableName = tableName.Replace(";", string.Empty).Trim();

                if (database.HasTable(tableName))
                {
                    // need to get a logical policy operator


                    // this needs to translate to a SELECT LogicalStoragePolicy FROM sys.UserTables WHERE TableName = TableName
                    // in the target HostDb

                    // we need to make sure the query plan has a SelectQueryPlanPart
                    // and ensure that SELECT part has a Layout with the above schema

                    // need a new set policy operator in the plan
                    if (!plan.Parts.Any(part => part is SelectQueryPlanPart))
                    {
                        plan.Parts.Add(new SelectQueryPlanPart());
                    }

                    foreach (var part in plan.Parts)
                    {
                        if (part is SelectQueryPlanPart)
                        {
                            var selectPart = part as SelectQueryPlanPart;
                            var layout = new ResultsetLayout();
                            ResultsetSourceTable sourceTable = new ResultsetSourceTable();

                            Table userTable = database.GetTable(UserTable.TABLE_NAME, Constants.SYS_SCHEMA);

                            sourceTable.Table = userTable.Address;

                            // return the LogicalStoragePolicy column
                            sourceTable.ColumnId = UserTable.GetColumns().
                                Where(c => c.Name == UserTable.Columns.LogicalStoragePolicy).FirstOrDefault().Id;

                            sourceTable.Order = 1;

                            layout.Columns.Add(sourceTable);
                            selectPart.Layout = layout;
                            var columns = new string[] { UserTable.Columns.LogicalStoragePolicy };

                            // filter by the table name
                            var value = RowValueMaker.Create(userTable, UserTable.Columns.TableName, tableName, true);
                            var trv = new TableRowValue(value, userTable.Address.TableId, userTable.Address.DatabaseId, userTable.Address.SchemaId);
                            TableReadFilter filter = new TableReadFilter(trv, ValueComparisonOperator.Equals, 1);

                            TableReadOperator readTable = new TableReadOperator(dbManager, sourceTable.Table, columns, filter, _log);
                            selectPart.Operations.Add(readTable);
                        }
                    }
                }
            }

            if (plan.TransactionPlan is null)
            {
                var xplan = new TransactionPlan();
                xplan.Behavior = TransactionBehavior.Normal;
                plan.TransactionPlan = xplan;
            }
        }

        private void EvaluateForLogicalStoragePolicy(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            // example: 
            //SET LOGICAL STORAGE FOR Products Host_Only;
            string keywords = DrummerKeywords.SET_LOGICAL_STORAGE + " " + DrummerKeywords.FOR;

            if (line.StartsWith(keywords))
            {
                string tablePolicy = line.Replace(keywords, string.Empty).Trim();
                tablePolicy = tablePolicy.Replace(";", string.Empty);
                string[] items = tablePolicy.Split(" ");

                string tableName = items[0].Trim();
                string policy = items[1].Trim();

                if (database.HasTable(tableName))
                {
                    if (DrummerKeywords.
                        LogicalStoragePolicyKeywords.
                        StoragePolicies.Any(p => string.Equals(p, policy, StringComparison.OrdinalIgnoreCase)))
                    {
                        // need a new set policy operator in the plan
                        if (!plan.Parts.Any(part => part is LogicalStoragePolicyPlanPart))
                        {
                            plan.Parts.Add(new LogicalStoragePolicyPlanPart());
                        }

                        foreach (var part in plan.Parts)
                        {
                            if (part is LogicalStoragePolicyPlanPart)
                            {
                                var item = part as LogicalStoragePolicyPlanPart;

                                var table = database.GetTable(tableName);
                                LogicalStoragePolicy enumPolicy = LogicalStoragePolicy.None;

                                switch (policy)
                                {
                                    case DrummerKeywords.LogicalStoragePolicyKeywords.HOST_ONLY:
                                        enumPolicy = LogicalStoragePolicy.HostOnly;
                                        break;
                                    case DrummerKeywords.LogicalStoragePolicyKeywords.MIRROR:
                                        enumPolicy = LogicalStoragePolicy.Mirror;
                                        break;
                                    case DrummerKeywords.LogicalStoragePolicyKeywords.PARTICIPANT_OWNED:
                                        enumPolicy = LogicalStoragePolicy.ParticipantOwned;
                                        break;
                                    case DrummerKeywords.LogicalStoragePolicyKeywords.SHARED:
                                        enumPolicy = LogicalStoragePolicy.Shared;
                                        break;
                                    case DrummerKeywords.LogicalStoragePolicyKeywords.NONE:
                                        enumPolicy = LogicalStoragePolicy.None;
                                        break;
                                    default:
                                        throw new InvalidOperationException("Unknown storage policy");
                                }

                                var op = new LogicalStoragePolicyOperator(database, table, enumPolicy, database.Name);
                                item.Operations.Add(op);
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unknown policy {policy}");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Table {tableName} not found in database {database.Name}");
                }
            }
        }
        #endregion
    }
}
