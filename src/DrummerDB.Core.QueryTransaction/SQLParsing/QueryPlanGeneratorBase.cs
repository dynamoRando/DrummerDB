using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Diagnostics;
using System.Linq;
using a = Antlr4.Runtime.Misc;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.SQLParsing
{
    /// <summary>
    /// Extends the <see cref="TSqlParserBaseListener"/> class and overloads any method calls of interest for query plan generation
    /// </summary>
    /// <remarks>This is part of the Antlr generated code. See IQueryManager.md for more information.</remarks>
    internal class QueryPlanGeneratorBase : TSqlParserBaseListener
    {
        #region Private Fields
        ICharStream _charStream;
        private IQueryPlanPart _currentPart;
        private int _currentPartOperatorOrder = 0;
        private int _currentPartOrder = 0;
        private QueryPlan _plan;
        private SearchConditionParser _searchConditionParser;
        private IStatement _statement;
        private string _tableName = string.Empty;
        #endregion

        #region Public Fields
        public IDbManager _dbManager;
        public IDatabase Database;
        public LogService LogService;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the current assigned Query Plan. If setting, also resets the Operator counts to 0.
        /// </summary>
        public QueryPlan QueryPlan
        {
            get
            {
                return _plan;
            }

            set
            {
                SetQueryPlan(value);
            }
        }

        public CommonTokenStream TokenStream { get; set; }
        #endregion

        #region Constructors
        public QueryPlanGeneratorBase(IDbManager dbManager)
        {
            _dbManager = dbManager;
            _searchConditionParser = new SearchConditionParser();
        }
        #endregion

        #region Public Methods
        public override void EnterDrop_table([NotNull] TSqlParser.Drop_tableContext context)
        {
            base.EnterDrop_table(context);
            DebugContext(context);

            _statement = new DropTableStatement(GetWhiteSpaceFromCurrentContext(context), Database);
        }

        public override void ExitDrop_table([NotNull] TSqlParser.Drop_tableContext context)
        {
            base.ExitDrop_table(context);

            if (_statement is DropTableStatement)
            {
                var dropStatement = _statement as DropTableStatement;
                var dropPart = new DropTablePlanPart();
                var dropOp = new DropTableOperator(Database, dropStatement.TableName);
                dropPart.Operations.Add(dropOp);

                _plan.Parts.Add(dropPart);
            }
        }

        public override void EnterColumn_definition([NotNull] TSqlParser.Column_definitionContext context)
        {
            base.EnterColumn_definition(context);

            if (_statement is IContextColumnDefinition)
            {
                var statement = _statement as IContextColumnDefinition;
                statement.HandleEnterColumnDefinition(new ContextWrapper(context, _charStream));
            }
        }

        public override void EnterColumn_name_list(TSqlParser.Column_name_listContext context)
        {
            base.EnterColumn_name_list(context);
            DebugContext(context);
        }

        public override void EnterCreate_database([NotNull] TSqlParser.Create_databaseContext context)
        {
            base.EnterCreate_database(context);
            DebugContext(context);

            string fullText = GetWhiteSpaceFromCurrentContext(context);
            _statement = new CreateHostDbStatement(fullText);
        }

        public override void EnterCreate_schema([NotNull] TSqlParser.Create_schemaContext context)
        {
            base.EnterCreate_schema(context);
            DebugContext(context);

            _statement = new CreateSchemaStatement(new ContextWrapper(context, _charStream).FullText);
        }

        public override void EnterCreate_table([NotNull] TSqlParser.Create_tableContext context)
        {
            base.EnterCreate_table(context);
            DebugContext(context);

            _statement = new CreateTableStatement();
            var create = _statement as CreateTableStatement;
            create.HandleEnterTableNameOrCreateTable(new ContextWrapper(context, _charStream));

        }

        public override void EnterData_type([NotNull] TSqlParser.Data_typeContext context)
        {
            base.EnterData_type(context);
            DebugContext(context);

            if (_statement is IContextDataType)
            {
                var statement = _statement as IContextDataType;
                statement.HandleEnterDataType(new ContextWrapper(context, _charStream));
            }
        }

        public override void EnterDelete_statement([NotNull] TSqlParser.Delete_statementContext context)
        {
            base.EnterDelete_statement(context);
            DebugContext(context);

            string fullText = GetWhiteSpaceFromCurrentContext(context);

            _statement = new DeleteStatement();
            var delete = _statement as DeleteStatement;
            delete.FullText = fullText;

            _currentPartOrder++;
            _currentPart = new DeleteQueryPlanPart();
            _currentPart.Order = _currentPartOrder;

            _plan.Parts.Add(_currentPart);

        }

        public override void EnterDrop_database([NotNull] TSqlParser.Drop_databaseContext context)
        {
            base.EnterDrop_database(context);

            string fullText = GetWhiteSpaceFromCurrentContext(context);
            _statement = new DropDbStatement(fullText);
        }

        public override void EnterExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            base.EnterExpression(context);

            string debug = context.GetText();
            Debug.WriteLine("EnterExpression");
            Debug.WriteLine(debug);

            if (_statement is InsertStatement)
            {
                var iStatement = _statement as InsertStatement;
                var currentRow = iStatement.PendingRow;

                int idx = currentRow.GetMaxValueId() + 1;
                var col = iStatement.GetColumn(idx);
                string value = GetWhiteSpaceFromCurrentContext(context).Trim().Replace("'", string.Empty);
                var val = new InsertValue(idx, col.ColumnName, value);
                currentRow.Values.Add(val);
            }

            if (_statement is UpdateStatement)
            {
                var update = _statement as UpdateStatement;
                // if we're in the WHERE clause, skip
                if (!(context.Parent is TSqlParser.Search_conditionContext) && !(context.Parent is TSqlParser.PredicateContext))
                {
                    var value = GetWhiteSpaceFromCurrentContext(context).Trim().Replace("'", string.Empty);
                    if (update.CurrentValue is UpdateTableValue)
                    {
                        var val = update.CurrentValue as UpdateTableValue;
                        if (string.IsNullOrEmpty(val.Value))
                        {
                            val.Value = value;
                        }
                    }
                }

            }
        }

        public override void EnterExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
            base.EnterExpression_list(context);

            string debug = context.GetText();
            Debug.WriteLine("EnterExpression_list");
            Debug.WriteLine(debug);

            if (_statement is InsertStatement)
            {
                var iStatement = _statement as InsertStatement;
                var insertRow = new InsertRow(iStatement.GetMaxRowId() + 1);
                iStatement.PendingRow = insertRow;
            }
        }

        public override void EnterFull_column_name([NotNull] TSqlParser.Full_column_nameContext context)
        {
            base.EnterFull_column_name(context);

            string debug = context.GetText();
            Debug.WriteLine("EnterFull_column_name");
            Debug.WriteLine(debug);

            if (_statement is UpdateStatement)
            {
                // skip parsing the WHERE clause, we've already done that
                if (!(context.Parent is TSqlParser.PredicateContext))
                {
                    var update = _statement as UpdateStatement;
                    string colName = GetWhiteSpaceFromCurrentContext(context);
                    if (update.CurrentColumn is not null)
                    {
                        if (string.IsNullOrEmpty(update.CurrentColumn.ColumnName))
                        {
                            update.CurrentColumn.ColumnName = colName;
                            update.CurrentColumn.TableName = update.TableName;
                        }
                    }
                }
            }
        }

        public override void EnterFull_table_name([NotNull] TSqlParser.Full_table_nameContext context)
        {
            base.EnterFull_table_name(context);
            DebugContext(context);

            if (_statement is IContextFullTableName)
            {
                var statement = _statement as IContextFullTableName;
                statement.HandleEnterFullTableName(new ContextWrapper(context, _charStream));
            }
        }

        public override void EnterId_([NotNull] TSqlParser.Id_Context context)
        {
            base.EnterId_(context);
            DebugContext(context);

            if (_statement is IContextId)
            {
                var statement = _statement as IContextId;
                statement.HandleEnterId(new ContextWrapper(context, _charStream));
            }
        }

        public override void EnterInsert_column_id([NotNull] TSqlParser.Insert_column_idContext context)
        {
            base.EnterInsert_column_id(context);

            string debug = context.GetText();
            Debug.WriteLine("EnterInsert_column_id");
            Debug.WriteLine(debug);

            if (_statement is InsertStatement)
            {
                var iStatement = _statement as InsertStatement;
                var colName = GetWhiteSpaceFromCurrentContext(context).Trim();
                int colIndex = iStatement.GetMaxColumnId() + 1;
                var col = new StatementColumn(colIndex, colName);
                iStatement.Columns.Add(col);
            }
        }

        public override void EnterInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            base.EnterInsert_statement(context);
            DebugContext(context);

            string fullText = GetWhiteSpaceFromCurrentContext(context);

            _statement = new InsertStatement();
            var insertStatement = _statement as InsertStatement;
            insertStatement.FullText = fullText;

            if (_plan.HasCooperativeOptions)
            {
                insertStatement.Options = _plan.Options;
            }
        }

        public override void EnterNull_notnull([NotNull] TSqlParser.Null_notnullContext context)
        {
            base.EnterNull_notnull(context);
            DebugContext(context);

            if (_statement is IContextNullNotNull)
            {
                var statement = _statement as IContextNullNotNull;
                statement.HandleEnterNullNotNull(new ContextWrapper(context, _charStream));
            }
        }

        // will need to adjust this code to handle multiple predicates
        // and also booleans
        public override void EnterPredicate([NotNull] TSqlParser.PredicateContext context)
        {
            base.EnterPredicate(context);
        }

        public override void EnterSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.EnterSearch_condition(context);
            Debug.WriteLine("EnterSearch_condition");

            if (_statement is SelectStatement)
            {
                var selectStatement = _statement as SelectStatement;
                _searchConditionParser.EvaluateSearchCondition(context, selectStatement, TokenStream, _charStream, _plan, Database, selectStatement.TableName);
            }

            if (_statement is UpdateStatement)
            {
                var update = _statement as UpdateStatement;
                _searchConditionParser.EvaluateSearchCondition(context, update, TokenStream, _charStream, _plan, Database, update.TableName);
            }

            if (_statement is DeleteStatement)
            {
                var delete = _statement as DeleteStatement;
                _searchConditionParser.EvaluateSearchCondition(context, delete, TokenStream, _charStream, _plan, Database, delete.TableName);
            }
        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);
            DebugContext(context);

            /*
            if (_query is SelectQuery)
            {
                var query = (_query as SelectQuery);
                query.SelectListText = context.GetText().Split(',').ToList();
            }
            */
        }

        public override void EnterSelect_list_elem([NotNull] TSqlParser.Select_list_elemContext context)
        {
            base.EnterSelect_list_elem(context);
            DebugContext(context);

            if (_statement is IContextSelectListElement)
            {
                var statement = _statement as IContextSelectListElement;
                statement.HandleEnterSelectListElement(new ContextWrapper(context, _charStream));
            }
        }

        public override void EnterSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            base.EnterSelect_statement(context);
            DebugContext(context);

            _currentPartOrder++;

            _currentPart = new SelectQueryPlanPart();
            _currentPart.Order = _currentPartOrder;

            _statement = new SelectStatement();
        }

        public override void EnterSimple_name([NotNull] TSqlParser.Simple_nameContext context)
        {
            base.EnterSimple_name(context);
            DebugContext(context);
        }

        public override void EnterSql_clauses([NotNull] TSqlParser.Sql_clausesContext context)
        {
            base.EnterSql_clauses(context);
            DebugContext(context);
        }

        public override void EnterTable_alias([NotNull] TSqlParser.Table_aliasContext context)
        {
            base.EnterTable_alias(context);
            DebugContext(context);

            if (_statement is IContextTableAlias)
            {
                var statement = _statement as IContextTableAlias;
                statement.HandleEnterTableAlias(new ContextWrapper(context, _charStream));
            }
        }

        public override void EnterTable_name([NotNull] TSqlParser.Table_nameContext context)
        {
            base.EnterTable_name(context);
            DebugContext(context);

            if (_statement is IContextTableName)
            {
                var statement = _statement as IContextTableName;
                statement.HandleEnterTableNameOrCreateTable(new ContextWrapper(context, _charStream));
            }
        }

        public override void EnterUpdate_elem([NotNull] TSqlParser.Update_elemContext context)
        {
            base.EnterUpdate_elem(context);

            string debug = context.GetText();
            Debug.WriteLine("EnterUpdate_elem");
            Debug.WriteLine(debug);

            if (_statement is UpdateStatement)
            {
                var update = _statement as UpdateStatement;
                int id = update.GetMaxColumnId();
                update.CurrentColumn = new StatementColumn(id);
                var val = new UpdateTableValue();
                val.Column = update.CurrentColumn;
                update.CurrentValue = val;
            }
        }

        public override void EnterUpdate_statement([NotNull] TSqlParser.Update_statementContext context)
        {
            base.EnterUpdate_statement(context);
            DebugContext(context);

            string fullText = GetWhiteSpaceFromCurrentContext(context);

            _statement = new UpdateStatement();
            var update = _statement as UpdateStatement;
            update.FullText = fullText;

            var updatePart = new UpdateQueryPlanPart();
            _plan.Parts.Add(updatePart);
        }

        public override void ExitColumn_definition([NotNull] TSqlParser.Column_definitionContext context)
        {
            base.ExitColumn_definition(context);

            if (_statement is IContextColumnDefinition)
            {
                var statement = _statement as IContextColumnDefinition;
                statement.HandleExitColumnDefinition(new ContextWrapper(context, _charStream));
            }
        }

        public override void ExitCreate_database([NotNull] TSqlParser.Create_databaseContext context)
        {
            base.ExitCreate_database(context);

            var cdb = _statement as CreateHostDbStatement;
            StatementPlanEvaluator.EvaluateQueryPlanForCreateDatabase(cdb, _plan, _dbManager);

        }

        public override void ExitCreate_schema([NotNull] TSqlParser.Create_schemaContext context)
        {
            base.ExitCreate_schema(context);

            if (_statement is not null)
            {
                if (_statement is CreateSchemaStatement)
                {
                    var createSchema = _statement as CreateSchemaStatement;
                    StatementPlanEvaluator.EvaluateQueryPlanForCreateSchema(createSchema, _plan, _dbManager, Database.Name);
                    _statement = null;
                }
            }
        }

        public override void ExitCreate_table([NotNull] TSqlParser.Create_tableContext context)
        {
            base.ExitCreate_table(context);

            if (_statement is not null)
            {
                if (_statement is CreateTableStatement)
                {
                    var createTable = _statement as CreateTableStatement;
                    StatementPlanEvaluator.EvaluateQueryPlanForCreateTable(createTable, _plan, _dbManager, Database.Name);
                }
            }
        }

        public override void ExitDelete_statement([NotNull] TSqlParser.Delete_statementContext context)
        {
            base.ExitDelete_statement(context);
            DebugContext(context);

            StatementPlanEvaluator.EvalutateQueryPlanForDelete(_statement as DeleteStatement, _plan, _dbManager, Database, LogService);

        }

        public override void ExitDrop_database([NotNull] TSqlParser.Drop_databaseContext context)
        {
            base.ExitDrop_database(context);

            var ddb = _statement as DropDbStatement;
            StatementPlanEvaluator.EvaluateQueryPlanForDropDatabase(ddb, _plan, _dbManager);
        }

        public override void ExitExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            base.ExitExpression(context);

            string debug = context.GetText();
            Debug.WriteLine("EnterExpression");
            Debug.WriteLine(debug);

            if (_statement is UpdateStatement)
            {
                var update = _statement as UpdateStatement;
                {
                    if (update.CurrentValue is not null)
                    {
                        update.Values.Add(update.CurrentValue);
                    }
                }
            }
        }

        public override void ExitExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
            base.ExitExpression_list(context);

            string debug = context.GetText();
            Debug.WriteLine("ExitExpression_list");
            Debug.WriteLine(debug);

            if (_statement is InsertStatement)
            {
                var iStatement = _statement as InsertStatement;
                iStatement.Rows.Add(iStatement.PendingRow);
            }
        }

        public override void ExitInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            base.ExitInsert_statement(context);
            StatementPlanEvaluator.EvaluateQueryPlanForInsert(_statement as InsertStatement, _plan, Database, _dbManager);
        }

        public override void ExitSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.ExitSearch_condition(context);

            string debug = context.GetText();

            Debug.WriteLine("ExitSearch_condition");
            Debug.WriteLine(debug);

            StatementPlanEvaluator.EvaluateQueryPlanForSearchConditions(_statement, _plan, Database, _dbManager, LogService);
        }

        public override void ExitSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.ExitSelect_list(context);
            DebugContext(context);
        }

        public override void ExitSelect_list_elem([NotNull] TSqlParser.Select_list_elemContext context)
        {
            base.ExitSelect_list_elem(context);
            DebugContext(context);
        }

        public override void ExitSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            base.ExitSelect_statement(context);
            DebugContext(context);

            // need to examine _tablePlan and add that data to _currentPart
            // do we need to this?
            // QueryPlan.Parts.Add(_currentPart);
        }

        public override void ExitSql_clauses([NotNull] TSqlParser.Sql_clausesContext context)
        {
            base.ExitSql_clauses(context);
            DebugContext(context);
        }

        // this needs to be refactored and documented
        public override void ExitTable_name([NotNull] TSqlParser.Table_nameContext context)
        {
            base.ExitTable_name(context);
            DebugContext(context);
            Table physicalTable = null;
            TreeAddress tableAddress;

            if (_statement is IColumnList)
            {
                var statement = _statement as IColumnList;
                var dmlStatement = _statement as IDMLStatement;
                if (!string.IsNullOrEmpty(dmlStatement.TableName) && statement.Columns.Count > 0)
                {
                    if (Database is not null)
                    {
                        physicalTable = Database.GetTable(dmlStatement.TableName);
                        tableAddress = physicalTable.Address;

                        if (_currentPart is not null)
                        {
                            if (_currentPart is SelectQueryPlanPart)
                            {
                                var part = _currentPart as SelectQueryPlanPart;

                                TableReadOperator readTableOperation = null;

                                if (statement.Columns.Select(c => c.ColumnName).ToList().Any(x => string.Equals(x, SQLGeneralKeywords.WILDCARD, StringComparison.OrdinalIgnoreCase)))
                                {
                                    readTableOperation = new TableReadOperator(_dbManager, tableAddress, physicalTable.Schema().Columns.Select(c => c.Name).ToArray(), LogService);
                                }
                                else
                                {
                                    readTableOperation = new TableReadOperator(_dbManager, tableAddress, statement.Columns.Select(c => c.ColumnName).ToArray(), LogService);
                                }


                                part.Operations.Add(readTableOperation);
                                _plan.Parts.Add(_currentPart);

                                var lockRequest = new LockObjectRequest();
                                var lockAddress =
                                    new SQLAddress
                                    {
                                        DatabaseId = tableAddress.DatabaseId,
                                        TableId = tableAddress.TableId,
                                        PageId = 0,
                                        RowId = 0,
                                        RowOffset = 0
                                    };

                                lockRequest.Address = lockAddress;
                                lockRequest.LockType = LockType.Read;
                                lockRequest.LockOrder = _currentPartOrder;
                                lockRequest.ObjectName = physicalTable.Name;
                                lockRequest.ObjectId = physicalTable.Schema().ObjectId;
                                lockRequest.ObjectType = ObjectType.Table;

                                _plan.LockObjectRequests.Add(lockRequest);

                                bool onlySelectPart = false;
                                if (_plan.Parts.Count == 1)
                                {
                                    foreach (var p in _plan.Parts)
                                    {
                                        if (p is SelectQueryPlanPart)
                                        {
                                            onlySelectPart = true;
                                        }
                                    }
                                }

                                if (_plan.Parts.Count == 1 && onlySelectPart)
                                {
                                    var select = _plan.Parts.First() as SelectQueryPlanPart;
                                    var resultLayout = new ResultsetLayout();

                                    if (statement.Columns.Select(c => c.ColumnName).ToList().Any(col => string.Equals(col, SQLGeneralKeywords.WILDCARD, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        foreach (var c in physicalTable.Schema().Columns)
                                        {
                                            var columnSource = new ResultsetSourceTable();
                                            columnSource.ColumnId = physicalTable.GetColumn(c.Name).Id;
                                            columnSource.Table = physicalTable.Address;
                                            columnSource.Order = physicalTable.GetColumn(c.Name).Ordinal;
                                            resultLayout.Columns.Add(columnSource);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var column in statement.Columns)
                                        {
                                            var columnSource = new ResultsetSourceTable();
                                            columnSource.ColumnId = physicalTable.GetColumn(column.ColumnName).Id;
                                            columnSource.Table = physicalTable.Address;
                                            columnSource.Order = statement.Columns.IndexOf(column);
                                            resultLayout.Columns.Add(columnSource);
                                        }
                                    }

                                    select.Layout = resultLayout;
                                }
                            }
                        }
                    }
                    else
                    {
                        physicalTable = Database.GetTable(dmlStatement.TableName);
                        tableAddress = physicalTable.Address;

                        if (_currentPart is not null)
                        {
                            if (_currentPart is SelectQueryPlanPart)
                            {
                                var part = _currentPart as SelectQueryPlanPart;

                                TableReadOperator readTableOperation = null;

                                if (statement.Columns.Select(c => c.ColumnName).ToList().Any(x => string.Equals(x, SQLGeneralKeywords.WILDCARD, StringComparison.OrdinalIgnoreCase)))
                                {
                                    readTableOperation = new TableReadOperator(_dbManager, tableAddress, physicalTable.Schema().Columns.Select(c => c.Name).ToArray(), LogService);
                                }
                                else
                                {
                                    readTableOperation = new TableReadOperator(_dbManager, tableAddress, statement.Columns.Select(c => c.ColumnName).ToArray(), LogService);
                                }


                                part.Operations.Add(readTableOperation);
                                _plan.Parts.Add(_currentPart);

                                var lockRequest = new LockObjectRequest();
                                var lockAddress =
                                    new SQLAddress
                                    {
                                        DatabaseId = tableAddress.DatabaseId,
                                        TableId = tableAddress.TableId,
                                        PageId = 0,
                                        RowId = 0,
                                        RowOffset = 0
                                    };

                                lockRequest.Address = lockAddress;
                                lockRequest.LockType = LockType.Read;
                                lockRequest.LockOrder = _currentPartOrder;
                                lockRequest.ObjectName = physicalTable.Name;
                                lockRequest.ObjectId = physicalTable.Schema().ObjectId;
                                lockRequest.ObjectType = ObjectType.Table;

                                _plan.LockObjectRequests.Add(lockRequest);

                                bool onlySelectPart = false;
                                if (_plan.Parts.Count == 1)
                                {
                                    foreach (var p in _plan.Parts)
                                    {
                                        if (p is SelectQueryPlanPart)
                                        {
                                            onlySelectPart = true;
                                        }
                                    }
                                }

                                if (_plan.Parts.Count == 1 && onlySelectPart)
                                {
                                    var select = _plan.Parts.First() as SelectQueryPlanPart;
                                    var resultLayout = new ResultsetLayout();

                                    if (statement.Columns.Select(c => c.ColumnName).ToList().Any(col => string.Equals(col, SQLGeneralKeywords.WILDCARD, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        foreach (var c in physicalTable.Schema().Columns)
                                        {
                                            var columnSource = new ResultsetSourceTable();
                                            columnSource.ColumnId = physicalTable.GetColumn(c.Name).Id;
                                            columnSource.Table = physicalTable.Address;
                                            columnSource.Order = physicalTable.GetColumn(c.Name).Ordinal;
                                            resultLayout.Columns.Add(columnSource);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var column in statement.Columns)
                                        {
                                            var columnSource = new ResultsetSourceTable();
                                            columnSource.ColumnId = physicalTable.GetColumn(column.ColumnName).Id;
                                            columnSource.Table = physicalTable.Address;
                                            columnSource.Order = statement.Columns.IndexOf(column);
                                            resultLayout.Columns.Add(columnSource);
                                        }
                                    }

                                    select.Layout = resultLayout;
                                }
                            }
                        }
                    }
                }
            }


        }

        public override void ExitUpdate_elem([NotNull] TSqlParser.Update_elemContext context)
        {
            base.ExitUpdate_elem(context);

            if (_statement is UpdateStatement)
            {
                var update = _statement as UpdateStatement;
                update.Columns.Add(update.CurrentColumn);
                update.CurrentValue = null;
                update.CurrentColumn = null;
            }
        }

        public override void ExitUpdate_statement([NotNull] TSqlParser.Update_statementContext context)
        {
            base.ExitUpdate_statement(context);

            if (_statement is not null)
            {
                if (_plan is not null)
                {
                    StatementPlanEvaluator.EvaluateQueryPlanForUpdate(_statement as UpdateStatement, _plan, _dbManager, Database, LogService);
                }
            }
        }

        public IStatement GetStatement()
        {
            return _statement;
        }
        #endregion

        #region Private Methods
        [Conditional("DEBUG")]
        private void DebugContext(ParserRuleContext context)
        {
            string debug = context.GetText();
            string fullText = GetWhiteSpaceFromCurrentContext(context);
            string callingMethod = new StackFrame(1, true).GetMethod().Name;

            Debug.WriteLine(callingMethod);
            Debug.WriteLine(debug);
            Debug.WriteLine(fullText);
        }

        private string GetWhiteSpaceFromCurrentContext(ParserRuleContext context)
        {
            int a = context.Start.StartIndex;
            int b = context.Stop.StopIndex;
            a.Interval interval = new a.Interval(a, b);
            _charStream = context.Start.InputStream;
            return _charStream.GetText(interval);
        }

        private void SetQueryPlan(QueryPlan plan)
        {
            _plan = plan;

            _currentPartOrder = 0;
            _currentPartOperatorOrder = 0;
        }
        #endregion
    }
}
