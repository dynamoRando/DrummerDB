using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying the location of a Row: PageId, RowId, RowOffset
    /// </summary>
    internal record struct RowAddress 
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
