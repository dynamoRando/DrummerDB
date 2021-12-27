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
            internal static int SIZE_OF_ROW_ID => Constants.SIZE_OF_UINT;
            internal static int SIZE_OF_ROW_TYPE => Constants.SIZE_OF_UINT;
            internal static int SIZE_OF_IS_FORWARDED => Constants.SIZE_OF_BOOL;
            internal static int SIZE_OF_FORWARD_OFFSET => Constants.SIZE_OF_UINT;
            internal static int SIZE_OF_FORWARDED_PAGE_ID => Constants.SIZE_OF_UINT;
            internal static int SIZE_OF_IS_LOGICALLY_DELETED => Constants.SIZE_OF_BOOL;
            internal static int SIZE_OF_ROW_TOTAL_SIZE => Constants.SIZE_OF_UINT;
            internal static int SIZE_OF_ROW_REMOTABLE_SIZE => Constants.SIZE_OF_UINT;
            internal static int SIZE_OF_ROW_VALUE_SIZE => Constants.SIZE_OF_UINT;

            internal static int RowIdOffset()
            {
                return 0;
            }

            internal static int RowTypeOffset()
            {
                return SIZE_OF_ROW_ID;
            }

            internal static int IsForwardedOffset()
            {
                return
                    SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE;
            }

            internal static int ForwardOffset()
            {
                return
                    SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED
                    ;
            }

            internal static int ForwardedPageIdOffset()
            {
                return
                    SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET
                    ;
            }

            internal static int IsLogicallyDeletedOffset()
            {
                return
                    SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID
                ;
            }

            internal static int RowTotalSizeOffset()
            {
                return
                    SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID + 
                    SIZE_OF_IS_LOGICALLY_DELETED
                ;
            }

            internal static int RowRemotableSizeOffset()
            {
                return
                    SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID +
                    SIZE_OF_IS_LOGICALLY_DELETED +
                    SIZE_OF_ROW_TOTAL_SIZE
                ;
            }

            internal static int RowValueSizeOffset()
            {
                return
                    SIZE_OF_ROW_ID +
                    SIZE_OF_ROW_TYPE +
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_FORWARDED_PAGE_ID +
                    SIZE_OF_IS_LOGICALLY_DELETED +
                    SIZE_OF_ROW_TOTAL_SIZE +
                    SIZE_OF_ROW_REMOTABLE_SIZE
                ;
            }

            internal static int Length()
            {
                return 
                    SIZE_OF_ROW_ID + 
                    SIZE_OF_ROW_TYPE + 
                    SIZE_OF_IS_FORWARDED +
                    SIZE_OF_FORWARD_OFFSET +
                    SIZE_OF_ROW_TOTAL_SIZE + 
                    SIZE_OF_ROW_REMOTABLE_SIZE +
                    SIZE_OF_ROW_VALUE_SIZE
                    ;
            }
        }
            

    }
}
