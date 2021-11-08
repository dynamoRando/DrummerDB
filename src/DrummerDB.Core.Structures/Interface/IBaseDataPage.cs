using Drummersoft.DrummerDB.Core.Structures.DbDebug;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    /// <summary>
    /// Represents a common data structure for pages that hold table data
    /// </summary>
    internal interface IBaseDataPage
    {
        /// <summary>
        /// Returns the type of data page, read from the Page's data
        /// </summary>
        /// <returns>The type of data page (of enum type DataPageType)</returns>
        DataPageType DataPageType();

        /// <summary>
        /// The backing byte array for the page
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        /// Adds a row to the Page's data, updates the total row count and the total bytes used (in the byte array)
        /// </summary>
        /// <param name="row">The row to be added</param>
        /// <exception cref="InvalidOperationException">Thrown if there is not enough room on the Page's data.</exception>
        /// <returns>The offset of where the row was added onto the page</returns>
        int AddRow(IRow row);

        /// <summary>
        /// Returns the specified row from a Page's data. 
        /// </summary>
        /// <param name="rowId">The row id to return</param>
        /// <returns>The specified row if found, otherwise NULL</returns>
        /// <remarks>Note that this function can be used to get rows forwarded to other pages. In other words, you can use this function to find where a row was forwarded.</remarks>
        IRow GetRow(int rowId);
        IRow GetRow(RowAddress address);
        bool IsFull(int rowSize);
        int PageId();
        int TotalBytesUsed();
        int TotalRows();

        /// <summary>
        /// Attempts to update the specified row (by comparing RowId) on the existing page and returns the result. If the row size fits the previous size, it will update in place. Otherwise, it will
        /// append the new row to the end of the page and mark the previous row entries as forwarded (on this page) with the forwarded flag and offsets.
        /// </summary>
        /// <param name="row">The row to update</param>
        /// <param name="updatedOffset">The new offset location of the updated row, if the update was successful, otherwise 0.</param>
        /// <returns>A status of the attempted update</returns>
        /// <remarks>Do not use this function to try and update rows to be forwarded to other pages. Use the ForwardRows function instead.</remarks>
        PageUpdateRowResult TryUpdateRowData(IRow row, out int updatedOffset);

        void DeleteRow(int rowId);
        PageType Type { get; }
        Guid DbId();
        int TableId();

        /// <summary>
        /// Used to determine the status of a row on a page
        /// </summary>
        /// <param name="rowId">The row to get the status for</param>
        /// <returns>A status of the row on page</returns>
        PageRowStatus GetRowStatus(int rowId);

        /// <summary>
        /// Marks rows on a page as forwared with the specified data. Use when denoting a row has been forwarded to another page.
        /// </summary>
        /// <param name="rowId">The row to mark as forwarded</param>
        /// <param name="newPageId">The page id where the row now lives</param>
        /// <param name="newPageOffset">The number of bytes offset on the page where the row lives</param>
        void ForwardRows(int rowId, int newPageId, int newPageOffset);

        /// <summary>
        /// Gets the rows with the specified value
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <returns>A list of row ids that contain the specified value</returns>
        List<RowAddress> GetRowsWithValue(IRowValue value);
        RowAddress[] GetRowAddressesWithValue(IRowValue value);

        /// <summary>
        /// Gets all the Row Ids on this page
        /// </summary>
        /// <returns>A list of row ids</returns>
        List<RowAddress> GetRowIdsOnPage(bool includeDeletedRows = false);
        int GetCountOfRowIdsOnPage(bool includeDeletedRows = false);

        bool HasValue(IRowValue value);
        RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column);
        RowDebug GetDebugRow(int rowId);

        /// <summary>
        /// Determines if the specified row lives on this page
        /// </summary>
        /// <param name="rowId">The row Id to find</param>
        /// <returns><c>TRUE</c> if the <see cref="PageRowStatus"/> is <see cref="PageRowStatus.IsOnPage"/> or <see cref="PageRowStatus.IsOnPageAndForwardedOnSamePage"/>
        /// , otherwise <c>FALSE</c></returns>
        bool HasRow(int rowId);
        List<int> GetRowOffsets(int rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false);
        int GetCountOfRowsWithValue(IRowValue value);
    }
}
