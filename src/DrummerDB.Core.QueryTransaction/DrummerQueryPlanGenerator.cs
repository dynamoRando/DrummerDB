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
using Drummersoft.DrummerDB.Core.Databases.Version;
using System.Net;
using static Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100;
using Drummersoft.DrummerDB.Core.Cryptography.Interface;

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
            EvaluateForGenerateContract(line, database, dbManager, ref plan);
            EvaluateForRequestParticipant(line, database, dbManager, ref plan);
            EvaluateForRequestHost(line, database, dbManager, ref plan);
            EvaluateForAddParticipant(line, database, dbManager, ref plan);
            EvaluteForReviewPendingContract(line, database, dbManager, ref plan);
            EvaluteForAcceptContract(line, database, dbManager, ref plan);
            EvaluteForRejectContract(line, database, dbManager, ref plan);
            EvaluteForReviewAcceptedContract(line, database, dbManager, ref plan);
            EvaluateForGenerateHostInfo(line, database, dbManager, ref plan);
            EvaluateForReviewHostInfo(line, database, dbManager, ref plan);
        }

        private void EvaluateForReviewHostInfo(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            // this needs to translate to a SELECT * FROM coop.HostInfo 
            // from the SystemDb
            if (line.StartsWith(DrummerKeywords.REVIEW_HOST_INFO))
            {
                var sysDb = dbManager.GetSystemDatabase();
                var hostInfoTable = sysDb.GetTable(Tables.HostInfo.TABLE_NAME);

                // need a new set policy operator in the plan
                if (!plan.HasPart(PlanPartType.Select))
                {
                    plan.AddPart(new SelectQueryPlanPart());
                }

                var part = plan.GetPart(PlanPartType.Select);
                if (part is SelectQueryPlanPart)
                {
                    var selectPart = part as SelectQueryPlanPart;
                    var layout = new ResultsetLayout();

                    // add the columns, starting with HostGuid
                    ResultsetSourceTable sourceTableColumnGuid = new ResultsetSourceTable();
                    sourceTableColumnGuid.Table = hostInfoTable.Address;
                    sourceTableColumnGuid.ColumnId = Tables.HostInfo.GetColumn(Tables.HostInfo.Columns.HostGUID).Id;
                    sourceTableColumnGuid.Order = 1;
                    layout.AddSource(sourceTableColumnGuid);

                    ResultsetSourceTable sourceTableColumnName = new ResultsetSourceTable();
                    sourceTableColumnName.Table = hostInfoTable.Address;
                    sourceTableColumnName.ColumnId = Tables.HostInfo.GetColumn(Tables.HostInfo.Columns.HostName).Id;
                    sourceTableColumnName.Order = 2;
                    layout.AddSource(sourceTableColumnName);

                    selectPart.Layout = layout;
                    var columns = new string[] { Tables.HostInfo.Columns.HostGUID, Tables.HostInfo.Columns.HostName };

                    TableReadOperator readTable = new TableReadOperator(dbManager, hostInfoTable.Address, columns, _log);
                    selectPart.AddOperation(readTable);
                }

                if (plan.TransactionPlan is null)
                {
                    var xplan = new TransactionPlan();
                    xplan.Behavior = TransactionBehavior.Normal;
                    plan.TransactionPlan = xplan;
                }
            }
        }

        private void EvaluateForGenerateHostInfo(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //GENERATE HOST INFO AS HOSTNAME {hostName};
            if (line.StartsWith(DrummerKeywords.GENERATE_HOST_INFO_AS_HOSTNAME))
            {
                string hostName = line.Replace(DrummerKeywords.GENERATE_HOST_INFO_AS_HOSTNAME, string.Empty).Trim();
                var sysDb = dbManager.GetSystemDatabase();

                // create an insert table operation for coop.HostInfo
                if (!plan.HasPart(PlanPartType.Insert))
                {
                    plan.AddPart(new InsertQueryPlanPart());
                }

                var part = plan.GetPart(PlanPartType.Insert);
                if (part is InsertQueryPlanPart)
                {
                    var insertHostOp = new InsertTableOperator(dbManager);
                    insertHostOp.TableName = Tables.HostInfo.TABLE_NAME;
                    insertHostOp.DatabaseName = sysDb.Name;
                    insertHostOp.TableSchemaName = Constants.COOP_SCHEMA;

                    var hostGuid = Guid.NewGuid();
                    var token = CryptoManager.GenerateToken();

                    var hostGuidColumn = Tables.HostInfo.GetColumn(Tables.HostInfo.Columns.HostGUID);
                    var hostNameColumn = Tables.HostInfo.GetColumn(Tables.HostInfo.Columns.HostName);
                    var hostToken = Tables.HostInfo.GetColumn(Tables.HostInfo.Columns.Token);

                    // need to create a row to insert
                    var insertRow = new InsertRow(1);

                    var insertValueHostGuid = new InsertValue(1, hostGuidColumn.Name, hostGuid.ToString());
                    var insertValueHostName = new InsertValue(2, hostNameColumn.Name, hostName);
                    var insertValueToken = new InsertValue(3, hostToken.Name, token.ToString());

                    insertRow.Values.Add(insertValueHostGuid);
                    insertRow.Values.Add(insertValueHostName);
                    insertRow.Values.Add(insertValueToken);

                    insertHostOp.Rows.Add(insertRow);

                    part.AddOperation(insertHostOp);
                }

                if (!plan.HasPart(PlanPartType.GenerateHostInfo))
                {
                    plan.AddPart(new GenerateHostInfoPlanPart());
                }

                part = plan.GetPart(PlanPartType.GenerateHostInfo);
                if (part is GenerateHostInfoPlanPart)
                {
                    var op = new GenerateHostInfoOperator(dbManager as IDbManager);
                    part.Operations.Add(op);
                }
            }
        }

        private void EvaluteForReviewAcceptedContract(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //REVIEW ACCEPTED CONTRACTS;
            if (line.StartsWith(DrummerKeywords.REVIEW_ACCEPTED_CONTRACTS))
            {
                var sysDb = dbManager.GetSystemDatabase();
                var table = sysDb.GetTable(Tables.CooperativeContracts.TABLE_NAME);

                // need a new set policy operator in the plan
                if (!plan.HasPart(PlanPartType.Select))
                {
                    plan.AddPart(new SelectQueryPlanPart());
                }

                var part = plan.GetPart(PlanPartType.Select);
                if (part is SelectQueryPlanPart)
                {
                    var selectPart = part as SelectQueryPlanPart;
                    var layout = new ResultsetLayout();

                    // add the columns, starting with HostGuid
                    ResultsetSourceTable sourceTableColumnHostGuid = new ResultsetSourceTable();
                    sourceTableColumnHostGuid.Table = table.Address;
                    sourceTableColumnHostGuid.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.HostGuid).Id;
                    sourceTableColumnHostGuid.Order = 1;
                    layout.AddSource(sourceTableColumnHostGuid);

                    ResultsetSourceTable sourceTableColumnContractGuid = new ResultsetSourceTable();
                    sourceTableColumnContractGuid.Table = table.Address;
                    sourceTableColumnContractGuid.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.ContractGUID).Id;
                    sourceTableColumnContractGuid.Order = 2;
                    layout.AddSource(sourceTableColumnContractGuid);

                    ResultsetSourceTable sourceTableColumnDatabaseName = new ResultsetSourceTable();
                    sourceTableColumnDatabaseName.Table = table.Address;
                    sourceTableColumnDatabaseName.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.DatabaseName).Id;
                    sourceTableColumnDatabaseName.Order = 3;
                    layout.AddSource(sourceTableColumnDatabaseName);

                    ResultsetSourceTable sourceTableColumnDatabaseId = new ResultsetSourceTable();
                    sourceTableColumnDatabaseId.Table = table.Address;
                    sourceTableColumnDatabaseId.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.DatabaseId).Id;
                    sourceTableColumnDatabaseId.Order = 4;
                    layout.AddSource(sourceTableColumnDatabaseId);

                    ResultsetSourceTable sourceTableColumnDescription = new ResultsetSourceTable();
                    sourceTableColumnDescription.Table = table.Address;
                    sourceTableColumnDescription.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.Description).Id;
                    sourceTableColumnDescription.Order = 5;
                    layout.AddSource(sourceTableColumnDescription);

                    ResultsetSourceTable sourceTableColumnVersion = new ResultsetSourceTable();
                    sourceTableColumnVersion.Table = table.Address;
                    sourceTableColumnVersion.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.Version).Id;
                    sourceTableColumnVersion.Order = 6;
                    layout.AddSource(sourceTableColumnVersion);

                    ResultsetSourceTable sourceTableColumnGeneratedDate = new ResultsetSourceTable();
                    sourceTableColumnGeneratedDate.Table = table.Address;
                    sourceTableColumnGeneratedDate.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.GeneratedDate).Id;
                    sourceTableColumnGeneratedDate.Order = 7;
                    layout.AddSource(sourceTableColumnGeneratedDate);

                    ResultsetSourceTable sourceTableColumnStatus = new ResultsetSourceTable();
                    sourceTableColumnStatus.Table = table.Address;
                    sourceTableColumnStatus.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.Status).Id;
                    sourceTableColumnStatus.Order = 8;
                    layout.AddSource(sourceTableColumnStatus);

                    selectPart.Layout = layout;
                    var columns = new string[]
                    {
                        Tables.CooperativeContracts.Columns.HostGuid,
                        Tables.CooperativeContracts.Columns.ContractGUID,
                        Tables.CooperativeContracts.Columns.DatabaseName,
                        Tables.CooperativeContracts.Columns.DatabaseId,
                        Tables.CooperativeContracts.Columns.Description,
                        Tables.CooperativeContracts.Columns.Version,
                        Tables.CooperativeContracts.Columns.GeneratedDate,
                        Tables.CooperativeContracts.Columns.Status
                    };

                    // filter by accepted contracts
                    var acceptedStatus = ContractStatus.Accepted;
                    var value = RowValueMaker.Create(table, UserTable.Columns.TableName, Convert.ToInt32(acceptedStatus).ToString(), false);
                    var trv = new TableRowValue(value, table.Address.TableId, table.Address.DatabaseId, table.Address.SchemaId);
                    TableReadFilter filter = new TableReadFilter(trv, ValueComparisonOperator.Equals, 1);

                    TableReadOperator readTable = new TableReadOperator(dbManager, table.Address, columns, filter, _log);
                    selectPart.AddOperation(readTable);
                }

                if (plan.TransactionPlan is null)
                {
                    var xplan = new TransactionPlan();
                    xplan.Behavior = TransactionBehavior.Normal;
                    plan.TransactionPlan = xplan;
                }
            }
        }

        private void EvaluteForRejectContract(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //REJECT CONTRACT BY AuthorName;
            if (line.StartsWith(DrummerKeywords.REJECT_CONTRACT_BY))
            {
                throw new NotImplementedException();
            }
        }

        private void EvaluteForAcceptContract(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //ACCEPT CONTRACT BY AuthorName;
            if (line.StartsWith(DrummerKeywords.ACCEPT_CONTRACT_BY))
            {
                throw new NotImplementedException();
            }
        }

        private void EvaluteForReviewPendingContract(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //REVIEW PENDING CONTRACTS;
            if (line.StartsWith(DrummerKeywords.REVIEW_PENDING_CONTRACTS))
            {
                var sysDb = dbManager.GetSystemDatabase();
                var table = sysDb.GetTable(Tables.CooperativeContracts.TABLE_NAME);

                // need a new set policy operator in the plan
                if (!plan.HasPart(PlanPartType.Select))
                {
                    plan.AddPart(new SelectQueryPlanPart());
                }

                var part = plan.GetPart(PlanPartType.Select);
                if (part is SelectQueryPlanPart)
                {
                    var selectPart = part as SelectQueryPlanPart;
                    var layout = new ResultsetLayout();

                    // add the columns, starting with HostGuid
                    ResultsetSourceTable sourceTableColumnHostGuid = new ResultsetSourceTable();
                    sourceTableColumnHostGuid.Table = table.Address;
                    sourceTableColumnHostGuid.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.HostGuid).Id;
                    sourceTableColumnHostGuid.Order = 1;
                    layout.AddSource(sourceTableColumnHostGuid);

                    ResultsetSourceTable sourceTableColumnContractGuid = new ResultsetSourceTable();
                    sourceTableColumnContractGuid.Table = table.Address;
                    sourceTableColumnContractGuid.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.ContractGUID).Id;
                    sourceTableColumnContractGuid.Order = 2;
                    layout.AddSource(sourceTableColumnContractGuid);

                    ResultsetSourceTable sourceTableColumnDatabaseName = new ResultsetSourceTable();
                    sourceTableColumnDatabaseName.Table = table.Address;
                    sourceTableColumnDatabaseName.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.DatabaseName).Id;
                    sourceTableColumnDatabaseName.Order = 3;
                    layout.AddSource(sourceTableColumnDatabaseName);

                    ResultsetSourceTable sourceTableColumnDatabaseId = new ResultsetSourceTable();
                    sourceTableColumnDatabaseId.Table = table.Address;
                    sourceTableColumnDatabaseId.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.DatabaseId).Id;
                    sourceTableColumnDatabaseId.Order = 4;
                    layout.AddSource(sourceTableColumnDatabaseId);

                    ResultsetSourceTable sourceTableColumnDescription = new ResultsetSourceTable();
                    sourceTableColumnDescription.Table = table.Address;
                    sourceTableColumnDescription.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.Description).Id;
                    sourceTableColumnDescription.Order = 5;
                    layout.AddSource(sourceTableColumnDescription);

                    ResultsetSourceTable sourceTableColumnVersion = new ResultsetSourceTable();
                    sourceTableColumnVersion.Table = table.Address;
                    sourceTableColumnVersion.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.Version).Id;
                    sourceTableColumnVersion.Order = 6;
                    layout.AddSource(sourceTableColumnVersion);

                    ResultsetSourceTable sourceTableColumnGeneratedDate = new ResultsetSourceTable();
                    sourceTableColumnGeneratedDate.Table = table.Address;
                    sourceTableColumnGeneratedDate.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.GeneratedDate).Id;
                    sourceTableColumnGeneratedDate.Order = 7;
                    layout.AddSource(sourceTableColumnGeneratedDate);

                    ResultsetSourceTable sourceTableColumnStatus = new ResultsetSourceTable();
                    sourceTableColumnStatus.Table = table.Address;
                    sourceTableColumnStatus.ColumnId = Tables.HostInfo.GetColumn(Tables.CooperativeContracts.Columns.Status).Id;
                    sourceTableColumnStatus.Order = 8;
                    layout.AddSource(sourceTableColumnStatus);

                    selectPart.Layout = layout;
                    var columns = new string[]
                    {
                        Tables.CooperativeContracts.Columns.HostGuid,
                        Tables.CooperativeContracts.Columns.ContractGUID,
                        Tables.CooperativeContracts.Columns.DatabaseName,
                        Tables.CooperativeContracts.Columns.DatabaseId,
                        Tables.CooperativeContracts.Columns.Description,
                        Tables.CooperativeContracts.Columns.Version,
                        Tables.CooperativeContracts.Columns.GeneratedDate,
                        Tables.CooperativeContracts.Columns.Status
                    };

                    // filter by pending contracts
                    var acceptedStatus = ContractStatus.Pending;
                    var value = RowValueMaker.Create(table, UserTable.Columns.TableName, Convert.ToInt32(acceptedStatus).ToString(), false);
                    var trv = new TableRowValue(value, table.Address.TableId, table.Address.DatabaseId, table.Address.SchemaId);
                    TableReadFilter filter = new TableReadFilter(trv, ValueComparisonOperator.Equals, 1);

                    TableReadOperator readTable = new TableReadOperator(dbManager, table.Address, columns, filter, _log);
                    selectPart.AddOperation(readTable);
                }

                if (plan.TransactionPlan is null)
                {
                    var xplan = new TransactionPlan();
                    xplan.Behavior = TransactionBehavior.Normal;
                    plan.TransactionPlan = xplan;
                }
            }
        }

        private void EvaluateForAddParticipant(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //ADD PARTICIPANT ParticipantAlias AT 127.0.0.1:5000;
            if (line.StartsWith(DrummerKeywords.ADD_PARTICIPANT))
            {
                var trimmedLine = line.Trim();

                //ParticipantAlias AT 127.0.0.1:5000
                string participant = trimmedLine.Replace(DrummerKeywords.ADD_PARTICIPANT + " ", string.Empty).Trim();
                var items = participant.Split(" ");

                string participantAlias = items[0].Trim();
                string participantIPPort = items[2].Trim();

                var ipItems = participantIPPort.Split(":");

                string ipAddress = ipItems[0];
                string portNumber = ipItems[1];

                // need to generate an INSERT operator in the query plan to save this to the sys.Participants table in the provided
                // user table

                if (!plan.HasPart(PlanPartType.Insert))
                {
                    plan.AddPart(new InsertQueryPlanPart());
                }

                var part = plan.GetPart(PlanPartType.Insert);
                if (part is InsertQueryPlanPart)
                {
                    IPAddress parsedAddress;
                    IPAddress.TryParse(ipAddress, out parsedAddress);

                    string stringIP6 = parsedAddress.MapToIPv6().ToString();
                    string stringIP4 = parsedAddress.MapToIPv4().ToString();

                    var insertOp = new InsertTableOperator(dbManager);
                    insertOp.TableName = Participants.TABLE_NAME;
                    insertOp.DatabaseName = database.Name;
                    insertOp.TableSchemaName = Constants.SYS_SCHEMA;

                    // get columns to insert values into
                    var colParticipantGuid = Participants.GetColumn(Participants.Columns.ParticpantGUID);
                    var colAlias = Participants.GetColumn(Participants.Columns.Alias);
                    var colIp4 = Participants.GetColumn(Participants.Columns.IP4Address);
                    var colIp6 = Participants.GetColumn(Participants.Columns.IP6Address);
                    var colPortNumber = Participants.GetColumn(Participants.Columns.PortNumber);
                    var colLastCom = Participants.GetColumn(Participants.Columns.LastCommunicationUTC);
                    var colAcceptedContract = Participants.GetColumn(Participants.Columns.HasAcceptedContract);
                    var colContractVersion = Participants.GetColumn(Participants.Columns.AcceptedContractVersion);
                    var colContractVersionDate = Participants.GetColumn(Participants.Columns.AcceptedContractDateTimeUTC);
                    var colToken = Participants.GetColumn(Participants.Columns.Token);

                    var participantId = Guid.NewGuid();

                    // need to create a row to insert
                    var insertRow = new InsertRow(1);

                    var valueParticipantId = new InsertValue(1, colParticipantGuid.Name, participantId.ToString());
                    var valueAlias = new InsertValue(2, colAlias.Name, participantAlias);
                    var valueIp4 = new InsertValue(3, colIp4.Name, stringIP4);
                    var valueIp6 = new InsertValue(4, colIp6.Name, stringIP6);
                    var valuePortNumber = new InsertValue(5, colPortNumber.Name, portNumber);
                    var valueLastCom = new InsertValue(6, colLastCom.Name);
                    var valueAcceptedContract = new InsertValue(7, colAcceptedContract.Name, "false");
                    var valueContractVersion = new InsertValue(8, colContractVersion.Name);
                    var valueContractVersionDate = new InsertValue(9, colContractVersionDate.Name);
                    var valueToken = new InsertValue(10, colToken.Name);

                    insertRow.AddValue(valueParticipantId);
                    insertRow.AddValue(valueAlias);
                    insertRow.AddValue(valueIp4);
                    insertRow.AddValue(valueIp6);
                    insertRow.AddValue(valuePortNumber);
                    insertRow.AddValue(valueLastCom);
                    insertRow.AddValue(valueAcceptedContract);
                    insertRow.AddValue(valueContractVersion);
                    insertRow.AddValue(valueContractVersionDate);
                    insertRow.AddValue(valueToken);

                    insertOp.Rows.Add(insertRow);

                    part.AddOperation(insertOp);
                }
            }
        }

        private void EvaluateForRequestHost(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {company.Alias};
            if (line.StartsWith(DrummerKeywords.REQUEST_HOST))
            {
                throw new NotImplementedException();
            }
        }

        private void EvaluateForRequestParticipant(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            //REQUEST PARTICIPANT ParticipantAlias SAVE CONTRACT;
            //should generate a network communication item via databases => remote database
            if (line.StartsWith(DrummerKeywords.REQUEST_PARTICIPANT))
            {
                var trimmedLine = line.Trim();

                //ParticipantAlias SAVE CONTRACT;
                string participantAliasCommand = trimmedLine.Replace(DrummerKeywords.REQUEST_PARTICIPANT + " ", string.Empty);

                if (participantAliasCommand.Contains(DrummerKeywords.SAVE_CONTRACT))
                {
                    string participantAlias = participantAliasCommand.Replace(DrummerKeywords.SAVE_CONTRACT, string.Empty).Trim();
                    if (database.HasParticipantAlias(participantAlias))
                    {
                        var participant = database.GetParticipant(participantAlias);
                        // generate a query plan to request the participant to save the latest contract
                        if (!plan.HasPart(PlanPartType.RemoteSaveContract))
                        {
                            plan.AddPart(new RemoteSaveContractPlanPart());

                            var part = plan.GetPart(PlanPartType.RemoteSaveContract);
                            var op = new RemoteSaveContractOperator(database, participant);

                            part.Operations.Add(op);
                        }
                    }
                }
            }
        }

        private void EvaluateForGenerateContract(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            // example:
            // GENERATE CONTRACT WITH DESCRIPTION IntroductionMessageGoesHere;
            // this data should be inserted into the sys.DatabaseContracts table in the database
            // once the contract has been generated, all the records in sys.UserTables should be updated with the new
            // contract GUID
            string description = string.Empty;
            Guid contractGuid = Guid.Empty;

            if (line.StartsWith(DrummerKeywords.GENERATE_CONTRACT_WITH_DESCRIPTION))
            {
                if (database.IsReadyForCooperation() && dbManager.HasHostInfo())
                {
                    string lineAnalysis = line;
                    string keywords = DrummerKeywords.GENERATE_CONTRACT_WITH_DESCRIPTION + " ";

                    // IntroductionMessageGoesHere
                    description = lineAnalysis.Replace(keywords, string.Empty).Trim();

                    // create an insert table operation for sys.DatabaseContracts
                    // and then an update table operation for sys.UserTables
                    if (!plan.HasPart(PlanPartType.Insert))
                    {
                        plan.AddPart(new InsertQueryPlanPart());
                    }

                    var part = plan.GetPart(PlanPartType.Insert);
                    if (part is InsertQueryPlanPart)
                    {
                        var insertDatabaseContractsOp = new InsertTableOperator(dbManager);
                        insertDatabaseContractsOp.TableName = DatabaseContracts.TABLE_NAME;
                        insertDatabaseContractsOp.DatabaseName = database.Name;
                        insertDatabaseContractsOp.TableSchemaName = Constants.SYS_SCHEMA;

                        contractGuid = Guid.NewGuid();
                        var contractVersion = Guid.NewGuid();

                        var contractGuidColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.ContractGUID);
                        var generatedDateColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.GeneratedDate);
                        var descriptionColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.Description);
                        var retiredColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.RetiredDate);
                        var versionColumn = DatabaseContracts.GetColumn(DatabaseContracts.Columns.Version);

                        // need to create a row to insert
                        var insertRow = new InsertRow(1);

                        var insertValueContractGuid = new InsertValue(1, contractGuidColumn.Name, contractGuid.ToString());
                        var insertValueGeneratedDate = new InsertValue(2, generatedDateColumn.Name, DateTime.Now.ToString());
                        var insertValueDescription = new InsertValue(3, descriptionColumn.Name, description);
                        var insertValueRetiredDate = new InsertValue(4, retiredColumn.Name, DateTime.MinValue.ToString());
                        var insertValueVersion = new InsertValue(5, versionColumn.Name, contractVersion.ToString());

                        insertRow.Values.Add(insertValueContractGuid);
                        insertRow.Values.Add(insertValueGeneratedDate);
                        insertRow.Values.Add(insertValueDescription);
                        insertRow.Values.Add(insertValueRetiredDate);
                        insertRow.Values.Add(insertValueVersion);

                        insertDatabaseContractsOp.Rows.Add(insertRow);

                        part.AddOperation(insertDatabaseContractsOp);
                    }


                    // need to update all rows in the table with the generated contract value
                    if (!plan.HasPart(PlanPartType.Update))
                    {
                        plan.AddPart(new UpdateQueryPlanPart());
                    }

                    part = plan.GetPart(PlanPartType.Update);
                    if (part is UpdateQueryPlanPart)
                    {
                        var address = new TreeAddress { DatabaseId = database.Id, TableId = UserTable.TABLE_ID, SchemaId = Guid.Parse(Constants.SYS_SCHEMA_GUID) };
                        // need to create update column sources

                        var columns = new List<IUpdateColumnSource>();

                        // create value object that we're going to update the contract guid to
                        var column = new UpdateTableValue();
                        var tableColumn = UserTable.GetColumn(UserTable.Columns.ContractGUID);

                        column.Column = new StatementColumn(tableColumn.Id, tableColumn.Name);
                        column.Value = contractGuid.ToString();

                        columns.Add(column);
                        var updateOp = new UpdateOperator(dbManager, address, columns);

                        // we need to create a read table operator to specify to update all the columns in the user table with the contract
                        // and set it as the previous operation

                        // only reading 1 column from the table that we want to update, the contract GUID column in sys.UserTables
                        string[] colNames = new string[1] { UserTable.Columns.ContractGUID };

                        var readTableOp = new TableReadOperator(dbManager, address, colNames, _log);
                        updateOp.PreviousOperation = readTableOp;

                        part.AddOperation(readTableOp);
                        part.AddOperation(updateOp);
                    }
                }

            }
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
                    if (!plan.HasPart(PlanPartType.Select))
                    {
                        plan.AddPart(new SelectQueryPlanPart());
                    }

                    var part = plan.GetPart(PlanPartType.Select);
                    if (part is SelectQueryPlanPart)
                    {
                        var selectPart = part as SelectQueryPlanPart;
                        var layout = new ResultsetLayout();
                        ResultsetSourceTable sourceTable = new ResultsetSourceTable();

                        Table userTable = database.GetTable(UserTable.TABLE_NAME, Constants.SYS_SCHEMA);

                        sourceTable.Table = userTable.Address;

                        // return the LogicalStoragePolicy column
                        sourceTable.ColumnId = UserTable.GetColumn(UserTable.Columns.LogicalStoragePolicy).Id;
                        sourceTable.Order = 1;

                        layout.AddSource(sourceTable);
                        selectPart.Layout = layout;
                        var columns = new string[] { UserTable.Columns.LogicalStoragePolicy };

                        // filter by the table name
                        var value = RowValueMaker.Create(userTable, UserTable.Columns.TableName, tableName, true);
                        var trv = new TableRowValue(value, userTable.Address.TableId, userTable.Address.DatabaseId, userTable.Address.SchemaId);
                        TableReadFilter filter = new TableReadFilter(trv, ValueComparisonOperator.Equals, 1);

                        TableReadOperator readTable = new TableReadOperator(dbManager, sourceTable.Table, columns, filter, _log);
                        selectPart.AddOperation(readTable);
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
                        if (!plan.HasPart(PlanPartType.LogicalStoragePolicy))
                        {
                            plan.AddPart(new LogicalStoragePolicyPlanPart());
                        }

                        var part = plan.GetPart(PlanPartType.LogicalStoragePolicy);
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
                            item.AddOperation(op);
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
