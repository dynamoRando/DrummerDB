using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal record struct TransactionEntryKey
    {
        public Guid TransactionBatchId { get; set; }
        public int SequenceNumber { get; set; }

        public override string ToString()
        {
            return $"TransactionEntryKey:{TransactionBatchId.ToString()}:{SequenceNumber.ToString()}";
        }
    }
}
