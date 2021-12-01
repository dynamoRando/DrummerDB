using Drummersoft.DrummerDB.Core.Structures.Version;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal static class PageParser
    {
        public delegate void ParsePageAction<T>(int pageId, Row row, int offset, int targetRowId, ref T item);

        public static void ParsePageData<T>(ReadOnlySpan<byte> data, int pageId, ParsePageAction<T> action, int targetRowId, bool stopAtFirstForward, bool includeDeletedRows, ref T foo)
        {
            int runningTotal = DataPageConstants.RowDataStartOffset();

            do
            {
                int lengthOfPreamble = RowConstants.LengthOfPreamble();
                int totalSlice = runningTotal + lengthOfPreamble;
                if (totalSlice >= Constants.PAGE_SIZE)
                {
                    break;
                }

                ReadOnlySpan<byte> preamble = data.Slice(runningTotal, RowConstants.LengthOfPreamble());
                Row item = new Row(preamble);

                if (item.Id == 0)
                {
                    break;
                }

                if (item.IsDeleted)
                {
                    if (includeDeletedRows)
                    {
                        action(pageId, item, runningTotal, targetRowId, ref foo);
                    }
                }
                else
                {
                    action(pageId, item, runningTotal, targetRowId, ref foo);
                }

                if (stopAtFirstForward && item.IsForwarded && item.ForwardOffset > 0)
                {
                    break;
                }

                if (item.IsLocal) //  the next item is the row size, get the size of the row to add to the running total
                {
                    // we've read the preample, so add it to our total
                    runningTotal += RowConstants.LengthOfPreamble();

                    // get the row size, which includes the preamble, the size of data, and the size of the row size itself
                    int rowSize = Row.GetRowSizeFromBinary(data.Slice(runningTotal, RowConstants.SIZE_OF_ROW_SIZE));

                    // the remainder is the size of the row minus what we've already read
                    int remainder = rowSize - RowConstants.LengthOfPreamble();

                    runningTotal += remainder;
                }
                else // the next item is the participant id, we need to add to the running total the size of a GUID 
                {
                    runningTotal += RowConstants.SIZE_OF_PARTICIPANT_ID;
                    if (runningTotal + RowConstants.SIZE_OF_PARTICIPANT_ID > Constants.PAGE_SIZE)
                    {
                        break;
                    }
                    var participantBytes = data.Slice(runningTotal, RowConstants.SIZE_OF_PARTICIPANT_ID);

                    //Guid participantId = DbBinaryConvert.BinaryToGuid(participantBytes);
                    //throw new NotImplementedException("Have not implemented retrival of a remote row. Need to get remote row and then");

                    //^^ the above would've been to get the ParticipantId, which is the only other data in the array.  Now fast forward to the next potential row
                    // add the preamble to our total since we've already read it
                    runningTotal += RowConstants.LengthOfPreamble();
                }
            }
            while (true);
        }
    }
}
