using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class ReviewLogicalStoragePolicyPlanPart : IQueryPlanPart
    {
        public int Order { get; set; }
        public List<IQueryPlanPartOperator> Operations { get; set; }
        public LockObjectRequest LockRequest { get; set; }
        public PlanPartType Type => PlanPartType.ReviewLogicalStoragePolicy;

        public ReviewLogicalStoragePolicyPlanPart()
        {
            Order = 0;
            Operations = new List<IQueryPlanPartOperator>();
            LockRequest = new LockObjectRequest();
        }

        public void AddOperation(IQueryPlanPartOperator operation)
        {
            Operations.Add(operation);
        }
    }
}
