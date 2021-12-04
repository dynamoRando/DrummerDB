using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class DeleteQueryPlanPart : IQueryPlanPart
    {
        public int Order { get; set; }
        public List<IQueryPlanPartOperator> Operations { get; set; }
        public LockObjectRequest LockRequest { get; set; }
        public StatementType StatementType => StatementType.DML;
        public PlanPartType Type => PlanPartType.Delete;    

        public DeleteQueryPlanPart()
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
