using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    interface IQueryPlanPart
    {
        int Order { get; set; }
        List<IQueryPlanPartOperator> Operations { get; set; }
        LockObjectRequest LockRequest { get; set; }
    }
}
