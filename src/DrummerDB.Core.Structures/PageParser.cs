using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal static class PageParser
    {
        public delegate void ParsePageAction<T>(uint pageId, RowPreamble row, uint offset, uint targetRowId, ref T item);

        public static void ParsePageData<T>(ReadOnlySpan<byte> data, uint pageId, ParsePageAction<T> action, uint targetRowId, bool stopAtFirstForward, bool includeDeletedRows, ref T foo)
        {
            uint runningTotal = (uint)DataPageConstants.RowDataStartOffset();

            do
            {
                uint lengthOfPreamble = (uint)RowConstants.Preamble.Length();
                uint totalSlice = runningTotal + lengthOfPreamble;
                if (totalSlice >= Constants.PAGE_SIZE)
                {
                    break;
                }

                ReadOnlySpan<byte> preamble = data.Slice((int)runningTotal, RowConstants.Preamble.Length());
                RowPreamble item = new RowPreamble(preamble);

                if (item.Id == 0)
                {
                    break;
                }

                if (item.IsLogicallyDeleted)
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

                runningTotal += item.RowTotalSize;

                /*
                    switch (item.Type)
                    {
                        case RowType.Local:
                            // we've read the preample, so add it to our total
                            runningTotal += (uint)RowConstants.Preamble.Length();

                            // the remainder is the size of the row minus what we've already read
                            uint remainder = item.RowTotalSize - (uint)RowConstants.Preamble.Length();

                            runningTotal += remainder;
                            break;
                        default:
                            throw new InvalidOperationException("Unknown row type");
                    }
                 */

            }
            while (true);
        }
    }
}
