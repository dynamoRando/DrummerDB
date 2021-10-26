using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// Executes a supplied SQL Query Plan
    /// </summary>
    internal interface IQueryExecutor
    {
        Task<Resultset> ExecutePlanAsync(QueryPlan plan, string un, string pw, Guid userSessionId);
    }
}
