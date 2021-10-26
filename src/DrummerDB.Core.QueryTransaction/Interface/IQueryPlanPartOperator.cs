using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    interface IQueryPlanPartOperator
    {
        IQueryPlanPartOperator PreviousOperation { get; set; }
        IQueryPlanPartOperator NextOperation { get; set; }
    }
}
