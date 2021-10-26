using Drummersoft.DrummerDB.Common;
using System;
using System.Collections.Generic;


// will be upgraded to record struct
namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    ///  Identifies the full location of a row. DatabaseId, TableId, PageId, RowId, RowOffset
    /// </summary>
    internal record SQLAddress 
    {
        public Guid DatabaseId { get; init; }
        public int TableId { get; init; }
        public int PageId { get; init; }
        public int RowId { get; init; }
        public int RowOffset { get; init; }
        public Guid SchemaId { get; init; }
        public byte[] ToBinaryFormat()
        {
            var arrays = new List<byte[]>(5);

            arrays.Add(DbBinaryConvert.GuidToBinary(DatabaseId));
            arrays.Add(DbBinaryConvert.IntToBinary(TableId));
            arrays.Add(DbBinaryConvert.IntToBinary(PageId));
            arrays.Add(DbBinaryConvert.IntToBinary(RowId));
            arrays.Add(DbBinaryConvert.IntToBinary(RowOffset));
            arrays.Add(DbBinaryConvert.GuidToBinary(SchemaId));

            return DbBinaryConvert.ArrayStitch(arrays);
        }

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
