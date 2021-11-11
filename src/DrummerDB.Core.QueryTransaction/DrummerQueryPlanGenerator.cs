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
