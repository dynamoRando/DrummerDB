using System;
using System.Threading;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class ActivePlan
    {
        private QueryPlan _plan;
        private CancellationTokenSource _cancelSource;

        public CancellationTokenSource CancelSource => _cancelSource;
        public Guid PlanId => _plan.PlanId;

        public ActivePlan(QueryPlan plan, CancellationTokenSource cancelSource)
        {
            _plan = plan;
            _cancelSource = cancelSource;
        }

        public void Cancel()
        {
            _cancelSource.Cancel();
        }
    }
}
