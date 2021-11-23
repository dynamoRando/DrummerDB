using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using a = Antlr4.Runtime.Misc;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Extends the <see cref="TSqlParserBaseListener"/> class and overloads any method calls of interest for parsing
    /// </summary>
    /// <remarks>This is part of the Antlr generated code. See IQueryManager.md for more information.</remarks>
    internal class StatementValidator : TSqlParserBaseListener
    {
        #region Private Fields
        ICharStream _charStream;
        private IStatement _statement;
        private string _tableName = string.Empty;
        #endregion

        #region Public Fields
        public IDatabase Database;
        public StatementReport StatementReport;
        public StatementType Type;
        #endregion

        #region Constructors
        internal StatementValidator()
        {
            if (StatementReport.Errors is null)
            {
                StatementReport.Errors = new List<string>();
            }
        }
        #endregion

        #region Public Properties
        public CommonTokenStream TokenStream { get; set; }
        #endregion

        #region Public Methods
        public override void EnterDrop_table([NotNull] TSqlParser.Drop_tableContext context)
        {
            base.EnterDrop_table(context);
            DebugContext(context);

            _statement = new DropTableStatement(GetWhiteSpaceFromCurrentContext(context), Database);
            DropTableStatement drop = _statement as DropTableStatement;

            if (!drop.IsValidated)
            {
                StatementReport.Errors.Add($"Table {drop.TableName} not found in database {Database.Name}");
                StatementReport.IsValid = false;
            }
        }

        public override void EnterColumn_name_list(TSqlParser.Column_name_listContext context)
        {
            base.EnterColumn_name_list(context);
            DebugContext(context);

            if (_statement is IColumnList)
            {
                var statement = _statement as IColumnList;

                List<string> errors;

                if (!statement.TryValidateColumnList(new ContextWrapper(context, _charStream), Database, out errors))
                {
                    StatementReport.Errors.AddRange(errors);
                    StatementReport.IsValid = false;
                }
            }

            if (!StatementReport.OriginalStatement.Contains(DDLKeywords.CREATE) && Type != StatementType.DDL)
            {
                var columns = context.GetText().Split(",").ToList();

                if (!string.IsNullOrEmpty(_tableName))
                {
                    var table = Database.GetTable(_tableName);
                    foreach (var column in columns)
                    {
                        if (!table.HasColumn(column.Trim()))
                        {
                            StatementReport.Errors.Add($"Object Error: Column {column.Trim()} does not exist in table {_tableName}");
                            StatementReport.IsValid = false;
                        }
                    }
                }
                else
                {
                    StatementReport.Errors.Add($"Object Error: The table name was not set");
                    StatementReport.IsValid = false;
                }
            }
        }

        public override void EnterFull_table_name([NotNull] TSqlParser.Full_table_nameContext context)
        {
            base.EnterFull_table_name(context);
            DebugContext(context);

            if (!StatementReport.OriginalStatement.Contains(DDLKeywords.CREATE) && Type != StatementType.DDL)
            {
                if (!Database.HasTable(context.GetText().Trim()))
                {
                    StatementReport.Errors.Add($"Object Error: Table {context.GetText().Trim()} was not found");
                    StatementReport.IsValid = false;
                }
                else
                {
                    _tableName = context.GetText().Trim();
                }
            }

        }

        public override void EnterSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.EnterSearch_condition(context);
            DebugContext(context);

        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);
            DebugContext(context);

            // this sucks
            // we are about to start a new SELECT list to validate, so go ahead and new up a holding object
            _statement = new SelectStatement();
            _statement.IsValidated = false;
        }

        public override void EnterSelect_list_elem([NotNull] TSqlParser.Select_list_elemContext context)
        {
            base.EnterSelect_list_elem(context);
            DebugContext(context);

            if (_statement is IContextSelectListElement)
            {
                var statement = _statement as IContextSelectListElement;
                var wrapper = new ContextWrapper(context, _charStream);
                statement.HandleEnterSelectListElement(wrapper);

                List<string> errors;
                if (!statement.TryValidateSelectListElement(wrapper, Database, out errors))
                {
                    StatementReport.Errors.AddRange(errors);
                }
            }
        }

        public override void EnterSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            base.EnterSelect_statement(context);
            DebugContext(context);

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

        public override void EnterTable_name([NotNull] TSqlParser.Table_nameContext context)
        {
            base.EnterTable_name(context);
            DebugContext(context);

            if (_statement is IContextTableName)
            {
                var statement = _statement as IContextTableName;

                List<string> errors;

                if (!StatementReport.OriginalStatement.Contains(DDLKeywords.CREATE) && Type != StatementType.DDL)
                {
                    if (!statement.TryValidateEnterTableNameOrCreateTable(new ContextWrapper(context, _charStream), Database, out errors))
                    {
                        StatementReport.Errors.AddRange(errors);
                        StatementReport.IsValid = false;
                    }

                    if (_statement is IContextSelectListElement)
                    {
                        var state = _statement as IContextSelectListElement;

                        List<string> colErrors = new List<string>();
                        if (!state.TryValidateSelectListElement(Database, out colErrors))
                        {
                            if (errors is not null)
                            {
                                errors.AddRange(colErrors);
                            }

                            StatementReport.Errors.AddRange(errors);
                            StatementReport.IsValid = false;
                        }
                    }
                }
            }
        }

        public override void ExitSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.ExitSearch_condition(context);
            DebugContext(context);
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
        }
        public override void ExitSql_clauses([NotNull] TSqlParser.Sql_clausesContext context)
        {
            base.ExitSql_clauses(context);
            DebugContext(context);
        }
        public override void ExitTable_name([NotNull] TSqlParser.Table_nameContext context)
        {
            base.ExitTable_name(context);
            DebugContext(context);

            if (_statement is IColumnList)
            {
                var statement = _statement as IColumnList;
                var dmlStatement = _statement as IDMLStatement;

            }
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
        #endregion
    }
}
