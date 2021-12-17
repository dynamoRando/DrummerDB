using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class QueryPlan
    {
        private string _sqlStatement = string.Empty;
        private ICooperativePlanOptions[] _options;

        public readonly Guid PlanId;
        public List<IQueryPlanPart> Parts { get; set; }
        public LockObjectRequestCollection LockObjectRequests { get; set; }
        public TransactionPlan TransactionPlan { get; set; }
        public string SqlStatement => _sqlStatement;
        public ICooperativePlanOptions[] Options => _options;
        public bool HasCooperativeOptions => _options.Length > 0;

        public QueryPlan(string sqlStatement)
        {
            PlanId = Guid.NewGuid();
            Parts = new List<IQueryPlanPart>();
            LockObjectRequests = new LockObjectRequestCollection();
            _sqlStatement = sqlStatement;
            _options = new ICooperativePlanOptions[0];
        }

        public QueryPlan(string sqlStatement, ICooperativePlanOptions[] options)
        {
            PlanId = Guid.NewGuid();
            Parts = new List<IQueryPlanPart>();
            LockObjectRequests = new LockObjectRequestCollection();
            _sqlStatement = sqlStatement;
            _options = options;
        }

        /// <summary>
        /// Checks the part plan for the specified type
        /// </summary>
        /// <param name="partType">The type of part to check in the plan</param>
        /// <returns><c>TRUE</c> if there is a plan part with the type, otherwise <c>FALSE</c></returns>
        public bool HasPart(PlanPartType partType)
        {
            foreach (var part in Parts)
            {
                if (part.Type == partType)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddPart(IQueryPlanPart part)
        {
            Parts.Add(part);
        }

        /// <summary>
        /// Gets the first plan part that matches the type
        /// </summary>
        /// <param name="type">The type of plan part to get</param>
        /// <returns>The first instance of this plan part, otherwise <c>NULL</c></returns>
        public IQueryPlanPart GetPart(PlanPartType type)
        {
            foreach (var part in Parts)
            {
                if (part.Type == type)
                {
                    return part;
                }
            }

            return null;
        }
    }
}
