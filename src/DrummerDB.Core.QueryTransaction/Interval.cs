using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Represents the location of a predicate when parsed from a query
    /// </summary>
    struct Interval : IEquatable<Interval>
    {
        public int A;
        public int B;

        public bool Equals(Interval other)
        {
            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return (this.A == other.A) && (this.B == other.B);
        }

        public static bool operator ==(Interval lhs, Interval rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Interval lhs, Interval rhs)
        {
            return !(lhs == rhs);
        }
    }
}
