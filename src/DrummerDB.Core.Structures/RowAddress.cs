using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying the location of a Row: PageId, RowId, RowOffset
    /// </summary>
    internal record struct RowAddress 
    {
        public uint PageId { get; init; }
        public uint RowId { get; init; }
        public Guid ParticipantId { get; init; }

        /// <summary>
        /// Represents the byte offset of the row on the page
        /// </summary>
        public uint RowOffset { get; init; }

        public RowAddress(uint pageId, uint rowId, uint rowOffset, Guid participantId)
        {
            PageId = pageId;
            RowId = rowId;
            RowOffset = rowOffset;
            ParticipantId = participantId;
        }
    }
}
