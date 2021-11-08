using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

// will be upgraded to record struct
namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying the location of a value in a row:
    /// DatabaseId, TableId, ColumnName, PageId, RowId, RowOffset, ValueOffset, ParseLength, ColumnId
    /// </summary>
    /// <remarks>This object is used to point directly to a value held on a page</remarks>
    internal record ValueAddress
    {
        /// <summary>
        /// The database of the value
        /// </summary>
        public Guid DatabaseId { get; init; }

        /// <summary>
        /// The table of the value
        /// </summary>
        public int TableId { get; init; }

        /// <summary>
        /// The name of the column for the value
        /// </summary>
        public string ColumnName { get; init; }

        /// <summary>
        /// The page of the value
        /// </summary>
        public int PageId { get; init; }

        /// <summary>
        /// The row of the value
        /// </summary>
        public int RowId { get; init; }

        /// <summary>
        /// Represents the byte offset of the row on the page
        /// </summary>
        public int RowOffset { get; init; }

        /// <summary>
        /// Represents the value offset in the byte array of the row. Should include the size prefix if applicable.
        /// </summary>
        public int ValueOffset { get; init; }

        /// <summary>
        /// The total length of the value to parse. See <seealso cref="IRowValue.ParseValueLength"/> for more information.
        /// </summary>
        public int ParseLength { get; init; }

        /// <summary>
        /// The id of the column for the value
        /// </summary>
        public int ColumnId { get; init; }

        public Guid SchemaId { get; init; }

        public PageAddress ToPageAddress()
        {
            return new PageAddress(DatabaseId, TableId, PageId, SchemaId);
        }

        public TreeAddress ToTreeAddress()
        {
            return new TreeAddress(DatabaseId, TableId, SchemaId);
        }

        public RowAddress ToRowAddress()
        {
            return new RowAddress(PageId, RowId, RowOffset);
        }
    }
}
