using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Used to evaluate a <see cref="IStatement"/> and provide needed changes to a <see cref="QueryPlan"/>
    /// </summary>
    static class StatementPlanEvaluator
    {
        #region Public Methods
        static public void EvaluateQueryPlanForInsert(InsertStatement statement, QueryPlan plan, IDatabase db, IDbManager dbManager)
        {
            if (plan is not null)
            {
                var insert = new InsertQueryPlanPart();
                var insertTable = new InsertTableOperator(dbManager);
                insertTable.Rows = statement.Rows;
                insertTable.TableName = statement.TableName;
                insertTable.Columns = statement.Columns;
                insertTable.DatabaseName = db.Name;
                insert.Operations.Add(insertTable);
                plan.Parts.Add(insert);
            }
        }

        static public void EvaluateQueryPlanForSearchConditions(IStatement statement, QueryPlan plan, IDatabase db, IDbManager dbManager, LogService log)
        {
            if (plan is not null)
            {
                if (statement is IWhereClause)
                {
                    var whereClause = statement as IWhereClause;
                    if (whereClause.WhereClause.Parts.Count > 1)
                    {
                        var maxPart = whereClause.WhereClause.GetMaxPredicate();

                        // we have a boolean chain that we need to iterate over
                        if (maxPart is BoolPredicate)
                        {
                            EvaluateBoolSearchChain(statement, plan, db, maxPart as BoolPredicate);
                        }
                    }
                    else
                    {
                        EvaluateForWhereClause(statement, plan, db, dbManager, log);
                    }
                }
            }
        }

        static public void EvaluateQueryPlanForCreateDatabase(CreateHostDbStatement statement, QueryPlan plan, IDbManager dbManager)
        {
            if (plan is not null)
            {
                var cdb = new CreateHostDbQueryPlanPart();
                var chdb = new CreateHostDbOperator(statement.DatabaseName, dbManager);
                cdb.Operations.Add(chdb);
                plan.Parts.Add(cdb);
            }
        }

        static public void EvaluateQueryPlanForDropDatabase(DropDbStatement statement, QueryPlan plan, IDbManager dbManager)
        {
            if (plan is not null)
            {
                var ddb = new DropDbQueryPlanPart();
                var dhdb = new DropHostDbOperator(statement.DatabaseName, dbManager);
                ddb.Operations.Add(dhdb);
                plan.Parts.Add(ddb);
            }
        }

        static public void EvaluateQueryPlanForCreateSchema(CreateSchemaStatement statement, QueryPlan plan, IDbManager dbManager, string databaseName)
        {
            if (plan is not null)
            {
                var createSchemaPlan = new CreateSchemaPlanPart();

                // default create in host database; may need to change later
                var createSchema = new CreateSchemaOperator(statement.Name, databaseName, dbManager, DatabaseType.Host);
                createSchemaPlan.Operations.Add(createSchema);
                plan.Parts.Add(createSchemaPlan);
            }
        }

        static public void EvaluateQueryPlanForCreateTable(CreateTableStatement statement, QueryPlan plan, IDbManager dbManager, string databaseName)
        {
            if (plan is not null)
            {
                var createTable = new CreateTableQueryPlanPart();
                var ct = new CreateTableOperator(databaseName, dbManager, statement.TableName);

                foreach (var column in statement.Columns)
                {
                    // done this way instead of passing directly to ensure that the length is set properly for fixed types
                    ct.Columns.Add(new ColumnSchema(column.Name, column.DataType, column.Ordinal, column.IsNullable));
                }

                createTable.Operations.Add(ct);
                plan.Parts.Add(createTable);
            }
        }
        static public void EvaluateQueryPlanForUpdate(UpdateStatement statement, QueryPlan plan, IDbManager dbManager, IDatabase db, LogService log)
        {
            if (plan is not null)
            {
                // if we have a WHERE clause, we need to use the TableReadOperator to determine ahead of time
                // the rows/values we will be updating
                foreach (var part in plan.Parts)
                {
                    if (part is UpdateQueryPlanPart)
                    {
                        var update = part as UpdateQueryPlanPart;

                        // check to see if we have a WHERE clause filter
                        if (update.Operations.Count == 1 && statement.HasWhereClause)
                        {
                            // we need to add an UPDATE operation, and then make sure that the previous
                            // operation is pointed to it
                            // this will signal when executing the plan that our update target is only for
                            // the specified rows of the previous operator.
                            var address = db.GetTable(statement.TableName).Address;

                            var updateOp = new UpdateOperator(dbManager, address, statement.Values);
                            updateOp.DatabaseName = db.Name;
                            var singleOp = update.Operations.First();
                            if (singleOp is TableReadOperator)
                            {
                                var readOp = singleOp as TableReadOperator;
                                updateOp.PreviousOperation = readOp;
                                readOp.NextOperation = updateOp;
                            }

                            update.Operations.Add(updateOp);
                        }
                        else
                        {
                            // we need to get a table read operator that returns the entire table (since we're going to update every row in the table with the values)
                            var address = db.GetTable(statement.TableName).Address;
                            var updateOp = new UpdateOperator(dbManager, address, statement.Values);
                            updateOp.DatabaseName = db.Name;

                            var readOp = new TableReadOperator(dbManager, address, statement.Columns.Select(column => column.ColumnName).ToArray(), log);
                            updateOp.PreviousOperation = readOp;
                            readOp.NextOperation = updateOp;
                            update.Operations.Add(readOp);
                            update.Operations.Add(updateOp);
                        }
                    }
                }
            }
        }

        static public void EvalutateQueryPlanForDelete(DeleteStatement statement, QueryPlan plan, IDbManager dbManager, IDatabase db, LogService log)
        {
            if (plan is not null)
            {
                // if we have a WHERE clause, we need to use the TableReadOperator to determine ahead of time
                // the rows/values we will be updating
                foreach (var part in plan.Parts)
                {
                    if (part is DeleteQueryPlanPart)
                    {
                        var delete = part as DeleteQueryPlanPart;

                        // check to see if we have a WHERE clause filter
                        if (delete.Operations.Count == 1 && statement.HasWhereClause)
                        {
                            // we need to add an DELETE operation, and then make sure that the previous
                            // operation is pointed to it
                            // this will signal when executing the plan that our update target is only for
                            // the specified rows of the previous operator.
                            var address = db.GetTable(statement.TableName).Address;

                            var updateOp = new DeleteOperator(dbManager, address);
                            updateOp.DatabaseName = db.Name;
                            var singleOp = delete.Operations.First();
                            if (singleOp is TableReadOperator)
                            {
                                var readOp = singleOp as TableReadOperator;
                                updateOp.PreviousOperation = readOp;
                                readOp.NextOperation = updateOp;
                            }

                            delete.Operations.Add(updateOp);
                        }
                        else
                        {
                            // we need to get a table read operator that returns the entire table (since we're going to delete every row in the table)
                            var table = db.GetTable(statement.TableName);
                            var address = table.Address;
                            var updateOp = new DeleteOperator(dbManager, address);
                            updateOp.DatabaseName = db.Name;

                            var readOp = new TableReadOperator(dbManager, address, table.Schema().Columns.Select(column => column.Name).ToArray(), log);
                            updateOp.PreviousOperation = readOp;
                            readOp.NextOperation = updateOp;
                            delete.Operations.Add(readOp);
                            delete.Operations.Add(updateOp);
                        }
                    }
                }
            }
        }
        #endregion

        #region Private Methods

        private static void EvaluateBoolSearchChain(IStatement statement, QueryPlan plan, IDatabase db, BoolPredicate predicate)
        {
            if (plan is not null)
            {
                TableReadOperator tableReadOperation = null;

                // find the read table operation where we will add filters to
                foreach (var op in plan.Parts)
                {
                    if (op is SelectQueryPlanPart)
                    {
                        var selectPart = op as SelectQueryPlanPart;
                        foreach (var operation in selectPart.Operations)
                        {
                            if (operation is TableReadOperator)
                            {
                                tableReadOperation = operation as TableReadOperator;
                            }
                        }
                    }
                }

                // need to iterate the bool chain
                // start with the topmost bool itself
                var topBoolStep = new TableReadBooleanFilter();
                topBoolStep.ComparisonOperator = predicate.ComparisonOperator;

                if (predicate.Right is Predicate)
                {
                    var rightPredicate = predicate.Right as Predicate;
                    var table = db.GetTable(rightPredicate.TableName);
                    var rowValue = RowValueMaker.Create(table, rightPredicate.ColumnName, rightPredicate.Value);
                    var tableRowValue = new TableRowValue(rowValue, table.Schema().Id, db.Id, table.Schema().Schema.SchemaGUID);

                    var rightFilter = new TableReadFilter(tableRowValue, rightPredicate.GetValueOperator(), rightPredicate.Id);
                    topBoolStep.RightFilter = rightFilter;
                }

                if (predicate.Left is BoolPredicate)
                {
                    var leftPredicate = predicate.Left as BoolPredicate;

                    // recurse thru the bool chain
                    EvaluateBoolPredicate(leftPredicate, statement, plan, db, topBoolStep);
                }

                // once we've iterated thru the chain, add it to the query plan
                List<ITableReadFilter> filters = new List<ITableReadFilter>(1);
                filters.Add(topBoolStep);
                tableReadOperation.SetFilters(filters);

            }
        }

        private static void EvaluateBoolPredicate(BoolPredicate predicate, IStatement statement, QueryPlan plan, IDatabase db, TableReadBooleanFilter previousStep)
        {
            var childBoolStep = new TableReadBooleanFilter();
            childBoolStep.ComparisonOperator = predicate.ComparisonOperator;

            if (predicate.Right is Predicate)
            {
                var rightPredicate = predicate.Right as Predicate;
                var table = db.GetTable(rightPredicate.TableName);
                var rowValue = RowValueMaker.Create(table, rightPredicate.ColumnName, rightPredicate.Value);
                var tableRowValue = new TableRowValue(rowValue, table.Schema().Id, db.Id, table.Schema().Schema.SchemaGUID);

                var rightFilter = new TableReadFilter(tableRowValue, rightPredicate.GetValueOperator(), rightPredicate.Id);
                childBoolStep.RightFilter = rightFilter;
            }

            if (predicate.Left is Predicate)
            {
                var leftPredicate = predicate.Left as Predicate;
                var table = db.GetTable(leftPredicate.TableName);
                var rowValue = RowValueMaker.Create(table, leftPredicate.ColumnName, leftPredicate.Value);
                var tableRowValue = new TableRowValue(rowValue, table.Schema().Id, db.Id, table.Schema().Schema.SchemaGUID);

                var leftFilter = new TableReadFilter(tableRowValue, leftPredicate.GetValueOperator(), leftPredicate.Id);
                childBoolStep.LeftFilter = leftFilter;
            }

            if (predicate.Left is BoolPredicate)
            {
                EvaluateBoolPredicate(predicate.Left as BoolPredicate, statement, plan, db, previousStep);
            }

            if (predicate.Right is BoolPredicate)
            {
                EvaluateBoolPredicate(predicate.Right as BoolPredicate, statement, plan, db, previousStep);
            }

            if (previousStep.RightFilter is null)
            {
                previousStep.RightFilter = childBoolStep;
            }

            if (previousStep.LeftFilter is null)
            {
                previousStep.LeftFilter = childBoolStep;
            }
        }

        private static void EvaluateForWhereClause(IStatement statement, QueryPlan plan, IDatabase db, IDbManager dbManager, LogService log)
        {
            if (plan is not null)
            {
                if (statement is IWhereClause)
                {
                    var searchableStatement = statement as IWhereClause;

                    if (searchableStatement.HasWhereClause)
                    {
                        TableReadOperator tableReadOperation = null;

                        foreach (var op in plan.Parts)
                        {
                            if (op is SelectQueryPlanPart)
                            {
                                var selectPart = op as SelectQueryPlanPart;
                                foreach (var operation in selectPart.Operations)
                                {
                                    if (operation is TableReadOperator)
                                    {
                                        tableReadOperation = operation as TableReadOperator;
                                    }
                                }
                            }

                            if (op is UpdateQueryPlanPart)
                            {
                                if (statement is UpdateStatement)
                                {
                                    var update = statement as UpdateStatement;
                                    bool hasReadOperation = op.Operations.Any(o => o is TableReadOperator);
                                    if (!hasReadOperation)
                                    {
                                        var physicalTable = db.GetTable(update.TableName);
                                        var tableAddress = physicalTable.Address;
                                        var readOp = new TableReadOperator(dbManager, tableAddress, update.Columns.Select(c => c.ColumnName).ToArray(), log);

                                        op.Operations.Add(readOp);
                                        tableReadOperation = readOp;
                                    }
                                }
                            }

                            if (op is DeleteQueryPlanPart)
                            {
                                var delete = statement as DeleteStatement;
                                bool hasReadOperation = op.Operations.Any(o => o is TableReadOperator);
                                if (!hasReadOperation)
                                {
                                    var physicalTable = db.GetTable(delete.TableName);
                                    var tableAddress = physicalTable.Address;
                                    var readOp = new TableReadOperator(dbManager, tableAddress, physicalTable.Schema().Columns.Select(col => col.Name).ToArray(), log);

                                    op.Operations.Add(readOp);
                                    tableReadOperation = readOp;
                                }
                            }
                        }

                        List<ITableReadFilter> filters = new List<ITableReadFilter>(searchableStatement.WhereClause.Parts.Count);
                        Table table = null;

                        foreach (var searchPart in searchableStatement.WhereClause.Parts)
                        {
                            if (statement is IDMLStatement)
                            {
                                var dmlStatement = statement as IDMLStatement;
                                table = db.GetTable(dmlStatement.TableName);
                            }

                            if (searchPart is Predicate)
                            {
                                var part = searchPart as Predicate;
                                var rowValue = RowValueMaker.Create(table, part.ColumnName, part.Value);

                                ValueComparisonOperator comparer = ValueComparisonOperator.Unknown;

                                if (string.Equals("=", part.Operator, StringComparison.OrdinalIgnoreCase))
                                {
                                    comparer = ValueComparisonOperator.Equals;
                                }

                                if (string.Equals(">", part.Operator, StringComparison.OrdinalIgnoreCase))
                                {
                                    comparer = ValueComparisonOperator.GreaterThan;
                                }

                                if (string.Equals("<", part.Operator, StringComparison.OrdinalIgnoreCase))
                                {
                                    comparer = ValueComparisonOperator.LessThan;
                                }

                                if (string.Equals(">=", part.Operator, StringComparison.OrdinalIgnoreCase))
                                {
                                    comparer = ValueComparisonOperator.GreaterThanOrEqualTo;
                                }

                                if (string.Equals("<=", part.Operator, StringComparison.OrdinalIgnoreCase))
                                {
                                    comparer = ValueComparisonOperator.LessThanOrEqualTo;
                                }

                                var tableRowValue = new TableRowValue(rowValue, table.Address.TableId, db.Id, table.Address.SchemaId);
                                var filter = new TableReadFilter(tableRowValue, comparer, searchPart.Id);

                                filters.Add(filter);
                            }


                        }

                        tableReadOperation.SetFilters(filters);
                    }

                }
            }
        }

        #endregion
    }
}
