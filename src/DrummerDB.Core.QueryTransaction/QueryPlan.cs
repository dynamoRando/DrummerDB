using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class QueryPlan
    {
        private string _sqlStatement = string.Empty;
        public readonly Guid PlanId;
        public List<IQueryPlanPart> Parts { get; set; }
        public LockObjectRequestCollection LockObjectRequests { get; set; }
        public TransactionPlan TransactionPlan {  get; set; }
        public string SqlStatement => _sqlStatement;

        public QueryPlan(string sqlStatement)
        {
            PlanId = Guid.NewGuid();
            Parts = new List<IQueryPlanPart>();
            LockObjectRequests = new LockObjectRequestCollection();
            _sqlStatement = sqlStatement;
        }
    }
}
