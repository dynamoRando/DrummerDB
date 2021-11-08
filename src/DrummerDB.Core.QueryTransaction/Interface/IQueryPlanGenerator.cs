using Drummersoft.DrummerDB.Core.Databases.Interface;

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
