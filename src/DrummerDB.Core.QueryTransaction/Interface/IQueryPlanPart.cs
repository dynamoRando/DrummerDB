using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    interface IQueryPlanPart
    {
        int Order { get; set; }
        List<IQueryPlanPartOperator> Operations { get; set; }
        LockObjectRequest LockRequest { get; set; }

        PlanPartType Type { get; }
        void AddOperation(IQueryPlanPartOperator operation);
    }
}
