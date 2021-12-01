using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    interface IQueryPlanPart
    {
        int Order { get; set; }
        List<IQueryPlanPartOperator> Operations { get; set; }
        LockObjectRequest LockRequest { get; set; }
    }
}
