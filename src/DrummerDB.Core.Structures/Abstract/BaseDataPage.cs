using Drummersoft.DrummerDB.Core.Structures.DbDebug;
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

        public abstract int AddRow(IRow row);
        public abstract DataPageType DataPageType();
        public abstract Guid DbId();
        public abstract void DeleteRow(int rowId);
        public abstract void ForwardRows(int rowId, int newPageId, int newPageOffset);
        public abstract IRow GetRow(int rowId);
        public abstract IRow GetRow(RowAddress address);
        public abstract List<RowAddress> GetRowIdsOnPage(bool includeDeletdRows = false);
        public abstract PageRowStatus GetRowStatus(int rowId);
        /// <summary>
        /// Determines if the specified row lives on this page
        /// </summary>
        /// <param name="rowId">The row Id to find</param>
        /// <returns><c>TRUE</c> if the <see cref="PageRowStatus"/> is <see cref="PageRowStatus.IsOnPage"/> or <see cref="PageRowStatus.IsOnPageAndForwardedOnSamePage"/>
        /// , otherwise <c>FALSE</c></returns>
        public abstract bool HasRow(int rowId);
        public abstract List<RowAddress> GetRowsWithValue(IRowValue value);
        public abstract bool IsFull(int rowSize);
        public abstract int PageId();
        public abstract int TableId();
        /// <summary>
        /// The total bytes used for the page, not including the Page's Preamble
        /// </summary>
        /// <returns>The total bytes used. This is the value of data, and does not include the preamble.</returns>
        public abstract int TotalBytesUsed();

        /// <summary>
        /// The total number of rows on the page. Note that this includes rows that are deleted. Total Rows != Logical Rows
        /// </summary>
        /// <returns>The total number of rows on the page. Note: Total Rows != Logical Rows</returns>
        public abstract int TotalRows();
        public abstract PageUpdateRowResult TryUpdateRowData(IRow row, out int updatedOffset);

        public abstract bool HasValue(IRowValue value);
        public abstract RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column);
        public abstract RowDebug GetDebugRow(int rowId);
        public abstract List<int> GetRowOffsets(int rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false);
        public abstract PageAddress Address { get; }
        public abstract int GetCountOfRowIdsOnPage(bool includeDeletedRows = false);
        public abstract RowAddress[] GetRowAddressesWithValue(IRowValue value);
        public abstract int GetCountOfRowsWithValue(IRowValue value);
        public abstract bool IsDeleted();
        public abstract void Delete();
        public abstract void UnDelete();
    }
}
