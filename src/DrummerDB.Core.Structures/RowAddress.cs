using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying the location of a Row: 
    /// PageId, RowId, RemotableId, RowOffset, RowType
    /// </summary>
    internal record struct RowAddress 
    {
        public uint PageId { get; init; }
        public uint RowId { get; init; }
        public Guid RemotableId { get; init; }

        /// <summary>
        /// Represents the byte offset of the row on the page
        /// </summary>
        public uint RowOffset { get; init; }
        public RowType RowType { get; init; }
        public bool HasDataLocally => Row.HasLocalData(RowType);

        public RowAddress(uint pageId, uint rowId, uint rowOffset, Guid remotableId, RowType type)
        {
            PageId = pageId;
            RowId = rowId;
            RowOffset = rowOffset;
            RemotableId = remotableId;
            RowType = type;
        }
    }
}
