using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// Generates a SQL Query Plan from a SQL query
    /// </summary>
    internal interface IQueryPlanGenerator
    {
        QueryPlan GetQueryPlan(string statement, IDatabase database, IDbManager dbManager);
    }
}
