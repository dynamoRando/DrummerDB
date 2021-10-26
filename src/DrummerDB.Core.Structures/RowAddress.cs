using System;

// will be upgraded in .net 6 to a record struct
namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying the location of a Row: PageId, RowId, RowOffset
    /// </summary>
    /// <seealso cref="System.IEquatable{Drummersoft.DrummerDB.Core.Structures.RowAddress}" />
    internal record RowAddress : IEquatable<RowAddress>
    {
        public int PageId { get; init; }
        public int RowId { get; init; }

        /// <summary>
        /// Represents the byte offset of the row on the page
        /// </summary>
        public int RowOffset { get; init; }

        public RowAddress(int pageId, int rowId, int rowOffset)
        {
            PageId = pageId;
            RowId = rowId;
            RowOffset = rowOffset;
        }
    }
}
