using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    internal abstract class BaseDataPage : IBaseDataPage, IPage
    {
        public abstract byte[] Data { get; }
        public abstract PageType Type { get; }

        public abstract uint AddRow(Row row);
        public abstract DataPageType DataPageType();
        public abstract Guid DbId();
        public abstract void DeleteRow(uint rowId);
        public abstract void ForwardRows(uint rowId, uint newPageId, uint newPageOffset);
        public abstract Row GetRow(uint rowId);
        public abstract Row GetRow(RowAddress address);
        public abstract List<RowAddress> GetRowIdsOnPage(bool includeDeletdRows = false);
        public abstract PageRowStatus GetRowStatus(uint rowId);
        /// <summary>
        /// Determines if the specified row lives on this page
        /// </summary>
        /// <param name="rowId">The row Id to find</param>
        /// <returns><c>TRUE</c> if the <see cref="PageRowStatus"/> is <see cref="PageRowStatus.IsOnPage"/> or <see cref="PageRowStatus.IsOnPageAndForwardedOnSamePage"/>
        /// , otherwise <c>FALSE</c></returns>
        public abstract bool HasRow(uint rowId);
        public abstract List<RowAddress> GetRowsWithValue(IRowValue value);
        public abstract bool IsFull(uint rowSize);
        public abstract uint PageId();
        public abstract uint TableId();
        /// <summary>
        /// The total bytes used for the page, not including the Page's Preamble
        /// </summary>
        /// <returns>The total bytes used. This is the value of data, and does not include the preamble.</returns>
        public abstract uint TotalBytesUsed();

        /// <summary>
        /// The total number of rows on the page. Note that this includes rows that are deleted. Total Rows != Logical Rows
        /// </summary>
        /// <returns>The total number of rows on the page. Note: Total Rows != Logical Rows</returns>
        public abstract uint TotalRows();
        public abstract PageUpdateRowResult TryUpdateRowData(RowValueGroup row, out uint updatedOffset);

        public abstract bool HasValue(IRowValue value);
        public abstract RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column);
        public abstract List<uint> GetRowOffsets(uint rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false);
        public abstract PageAddress Address { get; }
        public abstract uint GetCountOfRowIdsOnPage(bool includeDeletedRows = false);
        public abstract RowAddress[] GetRowAddressesWithValue(IRowValue value);
        public abstract uint GetCountOfRowsWithValue(IRowValue value);
        public abstract bool IsDeleted();
        public abstract void Delete();
        public abstract void UnDelete();
        public abstract RowAddress[] GetRowAddressesWithAllValues(IRowValue[] values);
        public abstract bool HasAllValues(IRowValue[] values);
    }
}
