namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    /// <summary>
    /// Constants for a Row
    /// </summary>
    internal class RowConstants
    {
        internal static int SIZE_OF_ROW_ID => Constants.SIZE_OF_INT;
        internal static int SIZE_OF_IS_LOCAL => Constants.SIZE_OF_BOOL;
        internal static int SIZE_OF_ROW_SIZE => Constants.SIZE_OF_INT;
        internal static int SIZE_OF_IS_DELETED => Constants.SIZE_OF_BOOL;
        internal static int SIZE_OF_PARTICIPANT_ID => Constants.SIZE_OF_GUID;
        internal static int SIZE_OF_IS_FORWARDED => Constants.SIZE_OF_BOOL;
        internal static int SIZE_OF_FORWARD_OFFSET => Constants.SIZE_OF_INT;
        internal static int SIZE_OF_FORWARDED_PAGE_ID => Constants.SIZE_OF_INT;

        internal static int LengthOfPreamble()
        {
            return SIZE_OF_ROW_ID + SIZE_OF_IS_LOCAL + SIZE_OF_IS_DELETED + SIZE_OF_IS_FORWARDED + SIZE_OF_FORWARD_OFFSET + SIZE_OF_FORWARDED_PAGE_ID;
        }

        internal static int RowIdOffset()
        {
            return 0;
        }

        internal static int IsLocalOffset()
        {
            return SIZE_OF_ROW_ID;
        }

        internal static int IsDeletedOffset()
        {
            return SIZE_OF_ROW_ID + SIZE_OF_IS_LOCAL;
        }

        internal static int IsForwardedOffset()
        {
            return SIZE_OF_ROW_ID + SIZE_OF_IS_LOCAL + SIZE_OF_IS_DELETED;
        }

        internal static int ForwardOffset()
        {
            return SIZE_OF_ROW_ID + SIZE_OF_IS_LOCAL + SIZE_OF_IS_DELETED + SIZE_OF_IS_FORWARDED;
        }

        internal static int SizeOfRowOffset()
        {
            return LengthOfPreamble();
        }

        internal static int RowDataOffset()
        {
            return LengthOfPreamble() + SIZE_OF_ROW_SIZE;
        }

        internal static int ParticipantIdOffset()
        {
            return LengthOfPreamble();
        }

        internal static int ForwardedPageIdOffset()
        {
            return SIZE_OF_ROW_ID + SIZE_OF_IS_LOCAL + SIZE_OF_IS_DELETED + SIZE_OF_IS_FORWARDED + SIZE_OF_FORWARD_OFFSET;
        }
    }
}
