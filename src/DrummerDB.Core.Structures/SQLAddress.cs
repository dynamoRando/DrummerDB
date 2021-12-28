using Drummersoft.DrummerDB.Common;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    ///  Identifies the full location of a row. DatabaseId, TableId, PageId, RowId, RowOffset
    /// </summary>
    internal record struct SQLAddress
    {
        public Guid DatabaseId { get; init; }
        public uint TableId { get; init; }
        public uint PageId { get; init; }
        public uint RowId { get; init; }
        public uint RowOffset { get; init; }
        public Guid SchemaId { get; init; }
        public byte[] ToBinaryFormat()
        {
            var arrays = new List<byte[]>(5);

            arrays.Add(DbBinaryConvert.GuidToBinary(DatabaseId));
            arrays.Add(DbBinaryConvert.UIntToBinary(TableId));
            arrays.Add(DbBinaryConvert.UIntToBinary(PageId));
            arrays.Add(DbBinaryConvert.UIntToBinary(RowId));
            arrays.Add(DbBinaryConvert.UIntToBinary(RowOffset));
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
            return new RowAddress(PageId, RowId, RowOffset, Guid.Empty);
        }
    }
}
