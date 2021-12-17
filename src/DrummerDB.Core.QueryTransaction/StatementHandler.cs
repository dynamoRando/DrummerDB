using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.SQLParsing;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Responsible for handling parsing a SQL statement for valid objects and for generating a query plan
    /// </summary>
    /// <remarks>This class is to abstract away the Antlr generated code. For more information, see IQueryManager.md</remarks>
    internal class StatementHandler
    {
        #region Private Fields
        StatementValidator _validator;
        QueryPlanGeneratorBase _generator;
        ParseTreeWalker _walker;
        IDbManager _db;
        LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public StatementHandler(IDbManager db)
        {
            _db = db;
        }

        public StatementHandler(IDbManager db, LogService log)
        {
            _db = db;
            _log = log;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Attempts to validate the supplied SQL statement is valid for the provided database. In other words, ensures that the tables exist, columns exist, etc. Also validates syntax.
        /// </summary>
        /// <param name="statement">The SQL Statement to validate. For reasons specific to the Antlr generated grammar, this string must be in UPPERCASE.</param>
        /// <param name="database">The database to validate the statement against.</param>
        /// <param name="type">The type of SQL statement: DDL or DML</param>
        /// <returns>A report detailing any errors found.</returns>
        /// <remarks>The Antlr generated code requires the statement to be in UPPERCASE</remarks>
        public StatementReport ParseStatementForValiditity(string statement, IDatabase database, StatementType type)
        {
            if (_validator is null)
            {
                _validator = new StatementValidator();
            }

            if (_walker is null)
            {
                _walker = new ParseTreeWalker();
            }

            _validator.StatementReport = new StatementReport();
            _validator.StatementReport.OriginalStatement = statement;
            _validator.StatementReport.IsValid = true;
            _validator.Type = type;

            if (_validator.StatementReport.Errors is null)
            {
                _validator.StatementReport.Errors = new List<string>();
            }

            // not sure if there's a way to not have to allocate new objects each time we evaluate a SQL statement
            AntlrInputStream inputStream = new AntlrInputStream(statement);
            var caseStream = new CaseChangingCharStream(inputStream, true);
            TSqlLexer lexer = new TSqlLexer(caseStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            TSqlParser parser = new TSqlParser(tokens);

            var errorHandler = new SyntaxErrorListener();
            parser.AddErrorListener(errorHandler);

            IParseTree tree = null;

            if (type == StatementType.DML)
            {
                tree = parser.dml_clause();
            }
            else if (type == StatementType.DDL)
            {
                tree = parser.ddl_clause();
            }

            _validator.Database = database;

            _validator.TokenStream = tokens;
            _walker.Walk(_validator, tree);

            if (errorHandler.Errors.Count > 0)
            {
                foreach (var error in errorHandler.Errors)
                {
                    string errorMesage = $"Syntax Error: {error.Message} at {error.Line.ToString()}:{error.CharPositionInLine.ToString()}";
                    _validator.StatementReport.Errors.Add(errorMesage);
                }

                _validator.StatementReport.IsValid = false;
            }

            parser.RemoveErrorListener(errorHandler);

            return _validator.StatementReport;
        }

        // This should be called after the statement has been validated. Will return a Query Plan to be executed by the Query Executor
        public QueryPlan ParseStatementForQueryPlan(string statement, IDatabase database, StatementType type)
        {
            if (_walker is null)
            {
                _walker = new ParseTreeWalker();
            }

            if (_generator is null)
            {
                _generator = new QueryPlanGeneratorBase(_db);
            }

            _generator.QueryPlan = new QueryPlan(statement);

            // not sure if there's a way to not have to allocate new objects each time we evaluate a SQL statement
            AntlrInputStream inputStream = new AntlrInputStream(statement);
            var caseStream = new CaseChangingCharStream(inputStream, true);
            TSqlLexer lexer = new TSqlLexer(caseStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            TSqlParser parser = new TSqlParser(tokens);

            var errorHandler = new SyntaxErrorListener();
            parser.AddErrorListener(errorHandler);

            IParseTree tree = null;

            if (type == StatementType.DML)
            {
                tree = parser.dml_clause();
            }
            else if (type == StatementType.DDL)
            {
                tree = parser.ddl_clause();
            }

            _generator.Database = database;
            _generator.LogService = _log;

            _generator.TokenStream = tokens;
            _walker.Walk(_generator, tree);

            // always remove the error handler
            parser.RemoveErrorListener(errorHandler);

            // for now, default transaction plans
            // this should be determined by reading the sql statement
            // if there are explicit begin tran/commit statements, (TransactionBehavior.Explit)
            // or if the statement is a continuation of a previous sent command (TransactionBehavior.Open)
            // or if we just want to immediately try/commmit (TransactionBehavior.Normal)
            if (_generator.QueryPlan.TransactionPlan is null)
            {
                var xactPlan = new TransactionPlan();
                xactPlan.Behavior = TransactionBehavior.Normal;
                _generator.QueryPlan.TransactionPlan = xactPlan;
            }

            return _generator.QueryPlan;
        }

        public QueryPlan ParseStatementForQueryPlan(string statement, IDatabase database, StatementType type, ICoopActionPlanOption[] options)
        {
            if (_walker is null)
            {
                _walker = new ParseTreeWalker();
            }

            if (_generator is null)
            {
                _generator = new QueryPlanGeneratorBase(_db);
            }

            _generator.QueryPlan = new QueryPlan(statement, options);

            // not sure if there's a way to not have to allocate new objects each time we evaluate a SQL statement
            AntlrInputStream inputStream = new AntlrInputStream(statement);
            var caseStream = new CaseChangingCharStream(inputStream, true);
            TSqlLexer lexer = new TSqlLexer(caseStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            TSqlParser parser = new TSqlParser(tokens);

            var errorHandler = new SyntaxErrorListener();
            parser.AddErrorListener(errorHandler);

            IParseTree tree = null;

            if (type == StatementType.DML)
            {
                tree = parser.dml_clause();
            }
            else if (type == StatementType.DDL)
            {
                tree = parser.ddl_clause();
            }

            _generator.Database = database;
            _generator.LogService = _log;

            _generator.TokenStream = tokens;
            _walker.Walk(_generator, tree);

            // always remove the error handler
            parser.RemoveErrorListener(errorHandler);

            // for now, default transaction plans
            // this should be determined by reading the sql statement
            // if there are explicit begin tran/commit statements, (TransactionBehavior.Explit)
            // or if the statement is a continuation of a previous sent command (TransactionBehavior.Open)
            // or if we just want to immediately try/commmit (TransactionBehavior.Normal)
            if (_generator.QueryPlan.TransactionPlan is null)
            {
                var xactPlan = new TransactionPlan();
                xactPlan.Behavior = TransactionBehavior.Normal;
                _generator.QueryPlan.TransactionPlan = xactPlan;
            }

            return _generator.QueryPlan;
        }

        public QueryPlan ParseStatementForQueryPlan(string statement, SystemDatabase database, StatementType type)
        {
            if (_walker is null)
            {
                _walker = new ParseTreeWalker();
            }

            if (_generator is null)
            {
                _generator = new QueryPlanGeneratorBase(_db);
            }

            _generator.QueryPlan = new QueryPlan(statement);
            _generator.LogService = _log;

            // not sure if there's a way to not have to allocate new objects each time we evaluate a SQL statement
            AntlrInputStream inputStream = new AntlrInputStream(statement);
            var caseStream = new CaseChangingCharStream(inputStream, true);
            TSqlLexer lexer = new TSqlLexer(caseStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            TSqlParser parser = new TSqlParser(tokens);

            var errorHandler = new SyntaxErrorListener();
            parser.AddErrorListener(errorHandler);

            IParseTree tree = null;

            if (type == StatementType.DML)
            {
                tree = parser.dml_clause();
            }
            else if (type == StatementType.DDL)
            {
                tree = parser.ddl_clause();
            }

            _generator.Database = database;

            _generator.TokenStream = tokens;
            _walker.Walk(_generator, tree);

            // always remove the error handler
            parser.RemoveErrorListener(errorHandler);

            // for now, default transaction plans
            // this should be determined by reading the sql statement
            // if there are explicit begin tran/commit statements, (TransactionBehavior.Explit)
            // or if the statement is a continuation of a previous sent command (TransactionBehavior.Open)
            // or if we just want to immediately try/commmit (TransactionBehavior.Normal)
            if (_generator.QueryPlan.TransactionPlan is null)
            {
                var xactPlan = new TransactionPlan();
                xactPlan.Behavior = TransactionBehavior.Normal;
                _generator.QueryPlan.TransactionPlan = xactPlan;
            }

            return _generator.QueryPlan;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
