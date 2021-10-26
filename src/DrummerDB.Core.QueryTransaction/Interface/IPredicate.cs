using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// Represents part of a WHERE clause, either a <see cref="Predicate"/> or a <see cref="BoolPredicate"/>.
    /// </summary>
    interface IPredicate
    {
        int Id { get; }
        Interval Interval { get; }
    }
}
