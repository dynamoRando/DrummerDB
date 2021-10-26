using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal struct TransactionEntryKey : IEquatable<TransactionEntryKey>
    {
        public Guid TransactionBatchId { get; set; }
        public int SequenceNumber { get; set; }

        public bool Equals(TransactionEntryKey other)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return (this.TransactionBatchId == other.TransactionBatchId) &&
                (this.SequenceNumber == other.SequenceNumber);
        }

        public static bool operator ==(TransactionEntryKey lhs, TransactionEntryKey rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TransactionEntryKey lhs, TransactionEntryKey rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return $"TransactionEntryKey:{TransactionBatchId.ToString()}:{SequenceNumber.ToString()}";
        }
    }
}
