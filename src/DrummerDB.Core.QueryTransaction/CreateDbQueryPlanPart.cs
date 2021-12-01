using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class CreateDbQueryPlanPart : IQueryPlanPart
    {
        public int Order { get; set; }
        public List<IQueryPlanPartOperator> Operations { get; set; }
        public LockObjectRequest LockRequest { get; set; }
        public StatementType StatementType => StatementType.DDL;

        public CreateDbQueryPlanPart()
        {
            Order = 0;
            Operations = new List<IQueryPlanPartOperator>();
            LockRequest = new LockObjectRequest();
        }
    }
}
