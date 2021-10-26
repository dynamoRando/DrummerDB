using Drummersoft.DrummerDB.Core.Structures;
using System;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// An in-memory representation of a transaction entry. Used with a <seealso cref="TransactionItemMap"/> to store transaction file offsets, sizes, etc.
    /// </summary>
    internal class TransactionItem
    {
        internal TransactionEntryKey Key { get; set; }
        internal bool IsCompleted { get; set; }

        /// <summary>
        /// The offset of the Transaction in the file in bytes. This includes the 4 byte INT that indicates the size of the entry. This also includes the entire 
        /// transaction preamble. For more information, see IDbLogFile.md.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        internal long Offset { get; set; }
        internal int Size { get; set; }
        internal long Order { get; set; }

        public TransactionItem()
        {

        }

        public TransactionItem(TransactionEntry entry, long offset, long order)
        {
            Key = entry.Key;
            IsCompleted = entry.IsCompleted;
            Size = entry.BinarySize;
            Offset = offset;
            Order = order;
        }
    }
}
