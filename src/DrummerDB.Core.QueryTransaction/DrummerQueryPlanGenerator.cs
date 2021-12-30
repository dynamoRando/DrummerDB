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
using Drummersoft.DrummerDB.Core.Structures.Interface;

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
            EvaluateForRequestHostNotifyAcceptedContract(line, database, dbManager, ref plan);
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
                    sourceTableColumnContractGuid.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.ContractGUID).Id;
                    sourceTableColumnContractGuid.Order = 2;
                    layout.AddSource(sourceTableColumnContractGuid);

                    ResultsetSourceTable sourceTableColumnDatabaseName = new ResultsetSourceTable();
                    sourceTableColumnDatabaseName.Table = table.Address;
                    sourceTableColumnDatabaseName.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.DatabaseName).Id;
                    sourceTableColumnDatabaseName.Order = 3;
                    layout.AddSource(sourceTableColumnDatabaseName);

                    ResultsetSourceTable sourceTableColumnDatabaseId = new ResultsetSourceTable();
                    sourceTableColumnDatabaseId.Table = table.Address;
                    sourceTableColumnDatabaseId.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.DatabaseId).Id;
                    sourceTableColumnDatabaseId.Order = 4;
                    layout.AddSource(sourceTableColumnDatabaseId);

                    ResultsetSourceTable sourceTableColumnDescription = new ResultsetSourceTable();
                    sourceTableColumnDescription.Table = table.Address;
                    sourceTableColumnDescription.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.Description).Id;
                    sourceTableColumnDescription.Order = 5;
                    layout.AddSource(sourceTableColumnDescription);

                    ResultsetSourceTable sourceTableColumnVersion = new ResultsetSourceTable();
                    sourceTableColumnVersion.Table = table.Address;
                    sourceTableColumnVersion.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.Version).Id;
                    sourceTableColumnVersion.Order = 6;
                    layout.AddSource(sourceTableColumnVersion);

                    ResultsetSourceTable sourceTableColumnGeneratedDate = new ResultsetSourceTable();
                    sourceTableColumnGeneratedDate.Table = table.Address;
                    sourceTableColumnGeneratedDate.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.GeneratedDate).Id;
                    sourceTableColumnGeneratedDate.Order = 7;
                    layout.AddSource(sourceTableColumnGeneratedDate);

                    ResultsetSourceTable sourceTableColumnStatus = new ResultsetSourceTable();
                    sourceTableColumnStatus.Table = table.Address;
                    sourceTableColumnStatus.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.Status).Id;
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
                    var value = RowValueMaker.Create(table, Tables.CooperativeContracts.Columns.Status, Convert.ToInt32(acceptedStatus).ToString(), false);
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
            string errorMessage = string.Empty;
            SystemDatabase systemDb = null;

            //ACCEPT CONTRACT BY AuthorName;
            if (line.StartsWith(DrummerKeywords.ACCEPT_CONTRACT_BY))
            {
                // AuthorName;
                string author = line.Replace(DrummerKeywords.ACCEPT_CONTRACT_BY + " ", string.Empty);

                systemDb = dbManager.GetSystemDatabase();
                // we need a table in the system database of pending contracts
                // not just contracts that we have saved to disk as pending

                var hostsTable = systemDb.GetTable(Tables.Hosts.TABLE_NAME);
                var hostValue = RowValueMaker.Create(hostsTable, Tables.Hosts.Columns.HostName, author);
                var totalValues = hostsTable.CountOfRowsWithValue(hostValue);

                Guid hostGuid = Guid.Empty;

                if (totalValues == 0)
                {
                    errorMessage = $"A contract with host with name {author} was not found";
                    throw new InvalidOperationException(errorMessage);
                }

                var rows = hostsTable.GetRowsWithValue(hostValue);

                if (rows.Count != 1)
                {
                    errorMessage = $"More than 1 or no rows found for author {author}";
                    throw new InvalidOperationException(errorMessage);
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

                    var searchResults = coopContracts.GetLocalRowsWithAllValues(searchValues.ToArray());

                    if (searchResults.Count() == 0)
                    {
                        errorMessage = $"No pending contracts found for author {author}";
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (searchResults.Count() > 1)
                    {
                        errorMessage = $"Multiple contracts found for author {author}";
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (searchResults.Count() == 1)
                    {
                        // get the host guid we're going to update to accepted
                        // this is the host that we will be filtering on
                        var rowValueHostGuid = searchResults.First().Values.Where(v => string.Equals(v.Column.Name, Tables.CooperativeContracts.Columns.HostGuid)).FirstOrDefault();
                        var rowValueContractGuid = searchResults.First().Values.Where(v => string.Equals(v.Column.Name, Tables.CooperativeContracts.Columns.ContractGUID)).FirstOrDefault();

                        // need to generate an update statement to mark the contract as accepted

                        if (!plan.HasPart(PlanPartType.Update))
                        {
                            plan.AddPart(new UpdateQueryPlanPart());
                        }

                        var part = plan.GetPart(PlanPartType.Update);
                        if (part is UpdateQueryPlanPart)
                        {
                            TreeAddress address;

                            if (database is null)
                            {
                                address = new TreeAddress { DatabaseId = systemDb.Id, TableId = Tables.CooperativeContracts.TABLE_ID, SchemaId = Guid.Parse(Constants.COOP_SCHEMA_GUID) };
                            }
                            else
                            {
                                address = new TreeAddress { DatabaseId = database.Id, TableId = Tables.CooperativeContracts.TABLE_ID, SchemaId = Guid.Parse(Constants.COOP_SCHEMA_GUID) };
                            }


                            // need to create update column sources

                            var columns = new List<IUpdateColumnSource>();

                            // create value object that we're going to update the contract guid to
                            var column = new UpdateTableValue();
                            var tableColumn = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.Status);

                            column.Column = new StatementColumn(tableColumn.Id, tableColumn.Name);
                            column.Value = Convert.ToInt32(ContractStatus.Accepted).ToString();

                            columns.Add(column);
                            var updateOp = new UpdateOperator(dbManager, address, columns);

                            // we need to create a read table operator to specify to update only the column with the specific host guid that is pending
                            // and set it as the previous operation

                            // columns that we need to read to identify what to update
                            string[] colNames = new string[3]
                            {
                                Tables.CooperativeContracts.Columns.HostGuid,
                                Tables.CooperativeContracts.Columns.ContractGUID,
                                Tables.CooperativeContracts.Columns.Status
                            };

                            var readTableOp = new TableReadOperator(dbManager, address, colNames, _log);
                            updateOp.PreviousOperation = readTableOp;

                            TableRowValue tableRowValueHostGuid = null;
                            if (database is null)
                            {
                                tableRowValueHostGuid = new TableRowValue(rowValueHostGuid as RowValue, Tables.CooperativeContracts.TABLE_ID, systemDb.Id, Guid.Parse(Constants.COOP_SCHEMA_GUID));
                            }
                            else
                            {
                                tableRowValueHostGuid = new TableRowValue(rowValueHostGuid as RowValue, Tables.CooperativeContracts.TABLE_ID, database.Id, Guid.Parse(Constants.COOP_SCHEMA_GUID));
                            }


                            var filterHostGuid = new TableReadFilter(tableRowValueHostGuid, ValueComparisonOperator.Equals, 1);

                            var filters = new List<ITableReadFilter>(2);
                            filters.Add(filterHostGuid);

                            // get the contract guid we will be updating to accepted from pending
                            // this will be the second item we're filtering on

                            TableRowValue tableRowValueContractGuid;

                            if (database is null)
                            {
                                tableRowValueContractGuid = new TableRowValue(rowValueContractGuid as RowValue, Tables.CooperativeContracts.TABLE_ID, systemDb.Id, Guid.Parse(Constants.COOP_SCHEMA_GUID));
                            }
                            else
                            {
                                tableRowValueContractGuid = new TableRowValue(rowValueContractGuid as RowValue, Tables.CooperativeContracts.TABLE_ID, database.Id, Guid.Parse(Constants.COOP_SCHEMA_GUID));
                            }


                            var filterContractGuid = new TableReadFilter(tableRowValueContractGuid, ValueComparisonOperator.Equals, 2);

                            filters.Add(filterContractGuid);

                            readTableOp.SetFilters(filters);

                            part.AddOperation(readTableOp);
                            part.AddOperation(updateOp);
                        }

                    }
                }
                else
                {
                    errorMessage = $"Could not find host!";
                    throw new InvalidOperationException(errorMessage);
                }
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
                    sourceTableColumnContractGuid.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.ContractGUID).Id;
                    sourceTableColumnContractGuid.Order = 2;
                    layout.AddSource(sourceTableColumnContractGuid);

                    ResultsetSourceTable sourceTableColumnDatabaseName = new ResultsetSourceTable();
                    sourceTableColumnDatabaseName.Table = table.Address;
                    sourceTableColumnDatabaseName.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.DatabaseName).Id;
                    sourceTableColumnDatabaseName.Order = 3;
                    layout.AddSource(sourceTableColumnDatabaseName);

                    ResultsetSourceTable sourceTableColumnDatabaseId = new ResultsetSourceTable();
                    sourceTableColumnDatabaseId.Table = table.Address;
                    sourceTableColumnDatabaseId.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.DatabaseId).Id;
                    sourceTableColumnDatabaseId.Order = 4;
                    layout.AddSource(sourceTableColumnDatabaseId);

                    ResultsetSourceTable sourceTableColumnDescription = new ResultsetSourceTable();
                    sourceTableColumnDescription.Table = table.Address;
                    sourceTableColumnDescription.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.Description).Id;
                    sourceTableColumnDescription.Order = 5;
                    layout.AddSource(sourceTableColumnDescription);

                    ResultsetSourceTable sourceTableColumnVersion = new ResultsetSourceTable();
                    sourceTableColumnVersion.Table = table.Address;
                    sourceTableColumnVersion.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.Version).Id;
                    sourceTableColumnVersion.Order = 6;
                    layout.AddSource(sourceTableColumnVersion);

                    ResultsetSourceTable sourceTableColumnGeneratedDate = new ResultsetSourceTable();
                    sourceTableColumnGeneratedDate.Table = table.Address;
                    sourceTableColumnGeneratedDate.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.GeneratedDate).Id;
                    sourceTableColumnGeneratedDate.Order = 7;
                    layout.AddSource(sourceTableColumnGeneratedDate);

                    ResultsetSourceTable sourceTableColumnStatus = new ResultsetSourceTable();
                    sourceTableColumnStatus.Table = table.Address;
                    sourceTableColumnStatus.ColumnId = Tables.CooperativeContracts.GetColumn(Tables.CooperativeContracts.Columns.Status).Id;
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
                    var value = RowValueMaker.Create(table, Tables.CooperativeContracts.Columns.Status, Convert.ToInt32(acceptedStatus).ToString(), false);
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
                    var colAcceptedContract = Participants.GetColumn(Participants.Columns.Status);
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
                    var valueAcceptedContract = new InsertValue(7, colAcceptedContract.Name, Convert.ToInt32(ContractStatus.NotSent).ToString());
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

        private void EvaluateForRequestHostNotifyAcceptedContract(string line, HostDb database, IDbManager dbManager, ref QueryPlan plan)
        {
            Guid hostGuid = Guid.Empty;
            string errorMessage = string.Empty;
            Contract acceptedContractItem;

            //REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {company.Alias};
            if (line.StartsWith(DrummerKeywords.REQUEST_HOST_NOTIFY_ACCEPTED_CONTRACT_BY))
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(DrummerKeywords.REQUEST_HOST_NOTIFY_ACCEPTED_CONTRACT_BY))
                {
                    // need to ensure that we actually have a host and contract with the 
                    // provided name

                    var hostName = trimmedLine.Replace(DrummerKeywords.REQUEST_HOST_NOTIFY_ACCEPTED_CONTRACT_BY + " ", string.Empty).Trim();

                    var sysDb = dbManager.GetSystemDatabase();
                    var hostTable = sysDb.GetTable(Tables.Hosts.TABLE_NAME);

                    var hostNameValue = RowValueMaker.Create(hostTable, Tables.Hosts.Columns.HostName, hostName);
                    uint resultCount = hostTable.CountOfRowsWithValue(hostNameValue);

                    if (resultCount != 1)
                    {
                        errorMessage = $"No host or multiple hosts found for Host Name {hostName}";
                        throw new InvalidOperationException(errorMessage);
                    }
                    else
                    {
                        var hostsResults = hostTable.GetLocalRowsWithValue(hostNameValue);

                        if (hostsResults.Count != 1)
                        {
                            errorMessage = $"No host or multiple hosts found for Host Name {hostName}";
                            throw new InvalidOperationException(errorMessage);
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
                            throw new InvalidOperationException(errorMessage);
                        }
                        else
                        {
                            // we need to generate a network communication object back to the host that we're accepting the above contract
                            // we also need to generate a partial database with the save contract schema
                            // we are basically agreeing to cooperate with the host for our data

                            // TO DO: need to populate acceptedContractItem with data from the coop tables
                            acceptedContractItem = sysDb.GetLatestAcceptedContractFromHostsTable(hostGuid);

                            if (!plan.HasPart(PlanPartType.CreatePartDb))
                            {
                                plan.AddPart(new CreatePartialDbQueryPlanPart());

                                var part = plan.GetPart(PlanPartType.CreatePartDb);
                                var op = new CreatePartDbOperator(dbManager, acceptedContractItem);

                                part.Operations.Add(op);
                            }

                            if (!plan.HasPart(PlanPartType.RemoteHostNotifyAcceptContract))
                            {
                                plan.AddPart(new RemoteHostAcceptContractPlanPart());

                                var part = plan.GetPart(PlanPartType.RemoteHostNotifyAcceptContract);
                                var op = new RemoteHostNotifyAcceptContractOperator(acceptedContractItem, dbManager as DbManager);

                                part.Operations.Add(op);
                            }

                            // need to generate an update statement to update the value for the last communication time with the host
                            if (!plan.HasPart(PlanPartType.Update))
                            {
                                plan.AddPart(new UpdateQueryPlanPart());
                                var part = plan.GetPart(PlanPartType.Update);

                                var columns = new List<IUpdateColumnSource>();

                                // create value object that we're going to update the last com value to
                                var column = new UpdateTableValue();
                                var tableColumn = Tables.Hosts.GetColumn(Tables.Hosts.Columns.LastCommunicationUTC);

                                column.Column = new StatementColumn(tableColumn.Id, tableColumn.Name);
                                column.Value = DateTime.UtcNow.ToString();

                                columns.Add(column);
                                var updateOp = new UpdateOperator(dbManager, hostTable.Address, columns);

                                // we need to create a read table operator to specify to update the specific host name

                                // specify the column that we're interested in reading + updating
                                string[] colNames = new string[2] { Tables.Hosts.Columns.LastCommunicationUTC, Tables.Hosts.Columns.HostName };

                                // filter by the host name
                                var value = RowValueMaker.Create(hostTable, Tables.Hosts.Columns.HostName, hostName, true);
                                var trv = new TableRowValue(value, hostTable.Address.TableId, hostTable.Address.DatabaseId, hostTable.Address.SchemaId);
                                TableReadFilter filter = new TableReadFilter(trv, ValueComparisonOperator.Equals, 1);

                                var readTableOp = new TableReadOperator(dbManager, hostTable.Address, colNames, filter, _log);
                                updateOp.PreviousOperation = readTableOp;

                                part.AddOperation(readTableOp);
                                part.AddOperation(updateOp);
                            }
                        }
                    }
                }
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
