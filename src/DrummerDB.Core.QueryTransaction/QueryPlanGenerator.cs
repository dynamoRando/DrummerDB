using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drummersoft.DrummerDB.Core.Databases;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class QueryPlanGenerator : IQueryPlanGenerator
    {
        #region Private Fields
        private StatementHandler _statementHandler;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public QueryPlanGenerator(StatementHandler statementHandler)
        {
            _statementHandler = statementHandler;
        }
        #endregion

        #region Public Methods
        public QueryPlan GetQueryPlan(string statement, IDatabase database, IDbManager dbManager)
        {
            //statement = statement.ToUpper();
            var type = QueryParser.DetermineStatementType(statement);
            return _statementHandler.ParseStatementForQueryPlan(statement, database, type);
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
