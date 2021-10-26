using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Enum
{
    /// <summary>
    /// Represents a BOOL operation, i.e. AND, OR...
    /// </summary>
    internal enum BooleanComparisonOperator
    {
        Unknown,
        And,
        Or
    }
}
