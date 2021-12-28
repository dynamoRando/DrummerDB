namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    /// <summary>
    /// Constants for a Row
    /// </summary>
    internal class RowConstants
    {
        // Row Layout Is Row Preamble + Remotable Data (If Applicable) + Values

        internal static class Preamble
        {
            internal static ushort SIZE_OF_ROW_ID => Constants.SIZE_OF_UINT;
            internal static ushort SIZE_OF_ROW_TYPE => Constants.SIZE_OF_UINT;
            internal static ushort SIZE_OF_IS_FORWARDED => Constants.SIZE_OF_BOOL;
            internal static ushort SIZE_OF_FORWARD_OFFSET => Constants.SIZE_OF_UINT;
            internal static ushort SIZE_OF_FORWARDED_PAGE_ID => Constants.SIZE_OF_UINT;
            internal static ushort SIZE_OF_IS_LOGICALLY_DELETED => Constants.SIZE_OF_BOOL;
            internal static ushort SIZE_OF_ROW_TOTAL_SIZE => Constants.SIZE_OF_UINT;
            internal static ushort SIZE_OF_ROW_REMOTABLE_SIZE => Constants.SIZE_OF_UINT;
            internal static ushort SIZE_OF_ROW_VALUE_SIZE => Constants.SIZE_OF_UINT;

            internal static short RowIdOffset()
            {
                return 0;
            }

            internal static ushort RowTypeOffset()
            {
                return SIZE_OF_ROW_ID;
            }

            internal static ushort IsForwardedOffset()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE);
            }

            internal static ushort ForwardOffset()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED)
                    ;
            }

            internal static ushort ForwardedPageIdOffset()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET)
                    ;
            }

            internal static ushort IsLogicallyDeletedOffset()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID)
                ;
            }

            internal static ushort RowTotalSizeOffset()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID + 
                    SIZE_OF_IS_LOGICALLY_DELETED)
                ;
            }

            internal static ushort RowRemotableSizeOffset()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID +
                    SIZE_OF_IS_LOGICALLY_DELETED +
                    SIZE_OF_ROW_TOTAL_SIZE)
                ;
            }

            internal static ushort RowValueSizeOffset()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID +
                    SIZE_OF_IS_LOGICALLY_DELETED +
                    SIZE_OF_ROW_TOTAL_SIZE +
                    SIZE_OF_ROW_REMOTABLE_SIZE)
                ;
            }

            internal static ushort Length()
            {
                return
                    (ushort)(SIZE_OF_ROW_ID + 
                    SIZE_OF_ROW_TYPE + 
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_ROW_TOTAL_SIZE + 
                    SIZE_OF_ROW_REMOTABLE_SIZE +
                    SIZE_OF_ROW_VALUE_SIZE)
                    ;
            }
        }
            
        internal static class RemotableFixedData
        {
            internal static ushort SIZE_OF_REMOTE_ID => Constants.SIZE_OF_GUID;
            internal static ushort SIZE_OF_IS_REMOTE_DELETED => Constants.SIZE_OF_BOOL;
            internal static ushort SIZE_OF_REMOTE_DELETED_UTC => Constants.SIZE_OF_DATETIME;
            internal static ushort SIZE_OF_REMOTE_TYPE => Constants.SIZE_OF_UINT;
            internal static ushort SIZE_OF_DATA_HASH_LENGTH => Constants.SIZE_OF_UINT;

            // data hash is a variable value

            public static ushort Length()
            {
                return
                    (ushort)(SIZE_OF_REMOTE_ID +
                    SIZE_OF_IS_REMOTE_DELETED +
                    SIZE_OF_REMOTE_DELETED_UTC +
                    SIZE_OF_REMOTE_TYPE +
                    SIZE_OF_DATA_HASH_LENGTH)
                    ;
            }
        }
    }
}
