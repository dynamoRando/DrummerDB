using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class RowPreamble
    {
        public uint Id { get; set; }  
        public RowType Type { get; set; }
        public bool IsForwarded { get; set; }
        public uint ForwardOffset { get; set; }
        public uint ForwardedPageId { get; set; }    
        public bool IsLogicallyDeleted { get; set; }
        public uint RowTotalSize { get; set; }
        public uint RowRemotableSize { get; set; }
        public uint RowValueSize { get; set; }

        public RowPreamble(uint id, RowType type)
        {
            Id = id;
            Type = type;    
        }

        public RowPreamble(ReadOnlySpan<byte> preamble)
        {
            checked
            {
                Id = DbBinaryConvert.BinaryToUInt(preamble.Slice(RowConstants.Preamble.RowIdOffset(), RowConstants.Preamble.SIZE_OF_ROW_ID));
                Type = (RowType)DbBinaryConvert.BinaryToUInt(preamble.Slice(RowConstants.Preamble.RowTypeOffset(), RowConstants.Preamble.SIZE_OF_ROW_TYPE));
                IsForwarded = DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.Preamble.IsForwardedOffset(), RowConstants.Preamble.SIZE_OF_IS_FORWARDED));
                ForwardOffset = DbBinaryConvert.BinaryToUInt(preamble.Slice(RowConstants.Preamble.ForwardOffset(), RowConstants.Preamble.SIZE_OF_FORWARD_OFFSET));
                ForwardedPageId = DbBinaryConvert.BinaryToUInt(preamble.Slice(RowConstants.Preamble.ForwardedPageIdOffset(), RowConstants.Preamble.SIZE_OF_FORWARDED_PAGE_ID));
                IsLogicallyDeleted = DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.Preamble.IsLogicallyDeletedOffset(), RowConstants.Preamble.SIZE_OF_IS_LOGICALLY_DELETED));
                RowTotalSize = DbBinaryConvert.BinaryToUInt(preamble.Slice(RowConstants.Preamble.RowTotalSizeOffset(), RowConstants.Preamble.SIZE_OF_ROW_TOTAL_SIZE));
                RowRemotableSize = DbBinaryConvert.BinaryToUInt(preamble.Slice(RowConstants.Preamble.RowRemotableSizeOffset(), RowConstants.Preamble.SIZE_OF_ROW_REMOTABLE_SIZE));
                RowValueSize = DbBinaryConvert.BinaryToUInt(preamble.Slice(RowConstants.Preamble.RowValueSizeOffset(), RowConstants.Preamble.SIZE_OF_ROW_VALUE_SIZE));
            }
        }

        public byte[] ToBinaryFormat()
        {
            var arrays = new List<byte[]>(9);

            var bId = DbBinaryConvert.UIntToBinary(Id);
            arrays.Add(bId);

            var bType = DbBinaryConvert.UIntToBinary((uint)Type);
            arrays.Add(bType);

            var bIsForwarded = DbBinaryConvert.BooleanToBinary(IsForwarded);
            arrays.Add(bIsForwarded);

            var bForwardOffset = DbBinaryConvert.UIntToBinary(ForwardOffset);
            arrays.Add(bForwardOffset);

            var bForwardPageId = DbBinaryConvert.UIntToBinary(ForwardedPageId);
            arrays.Add(bForwardPageId);

            var bIsLogicallyDeleted = DbBinaryConvert.BooleanToBinary(IsLogicallyDeleted);
            arrays.Add(bIsLogicallyDeleted);

            var bTotalRowSize = DbBinaryConvert.UIntToBinary(RowTotalSize);
            arrays.Add(bTotalRowSize);  

            var bRemoteSize = DbBinaryConvert.UIntToBinary(RowRemotableSize);
            arrays.Add(bRemoteSize);

            var bValueSize = DbBinaryConvert.UIntToBinary(RowValueSize);
            arrays.Add(bValueSize);

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
