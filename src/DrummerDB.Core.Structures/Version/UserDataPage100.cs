using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Factory;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A byte array of data for a table
    /// </summary>
    internal class UserDataPage100 : UserDataPage
    {
        #region Private Fields
        private BaseDataPage100 _basePage;
        #endregion

        #region Public Properties
        /// <summary>
        /// The binary data for this page
        /// </summary>
        public override byte[] Data => _basePage.Data;
        public override PageAddress Address => _basePage.Address;

        /// <summary>
        /// The page type
        /// </summary>
        public override PageType Type => PageType.Data;
        #endregion

        #region Constructors
        /// <summary>
        /// Make a new Data Page in memory
        /// </summary>
        /// <param name="address">The address of this page</param>
        /// <param name="schema">The schema for the table this page is on</param>
        /// <param name="cache">A reference to cache</param>
        public UserDataPage100(PageAddress address, ITableSchema schema)
        {
            _basePage = BaseDataPageFactory.GetBaseDataPage100(address, schema, Enum.DataPageType.User);
        }

        /// <summary>
        /// Make a new page based on data from disk and attempts to set the Page Address and Type from the binary data
        /// </summary>
        /// <param name="data">The byte array from disk</param>
        /// <param name="schema">The schema for the table this page is on</param>
        public UserDataPage100(byte[] data, ITableSchema schema)
        {
            _basePage = BaseDataPageFactory.GetBaseDataPage100(data, schema);
        }
        #endregion

        #region Public Methods
        public override bool HasAllValues(IRowValue[] values)
        {
            return _basePage.HasAllValues(values);
        }

        public override RowAddress[] GetRowAddressesWithAllValues(IRowValue[] values)
        {
            return _basePage.GetRowAddressesWithAllValues(values);
        }

        public override List<uint> GetRowOffsets(uint rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false)
        {
            return _basePage.GetRowOffsets(rowId, stopAtFirstForward, includeDeletedRows);
        }

        public override bool HasRow(uint rowId)
        {
            return _basePage.HasRow(rowId);
        }

        public override void ForwardRows(uint rowId, uint newPageId, uint newPageOffset)
        {
            _basePage.ForwardRows(rowId, newPageId, newPageOffset);
        }

        public override PageRowStatus GetRowStatus(uint rowId)
        {
            return _basePage.GetRowStatus(rowId);
        }

        /// <summary>
        /// Checks to see if there is room left on the page for the specified row length
        /// </summary>
        /// <param name="rowSize">The length of the row</param>
        /// <returns>True if there is room on the page, otherwise false</returns>
        public override bool IsFull(uint rowSize)
        {
            return _basePage.IsFull(rowSize);
        }

        /// <summary>
        /// Flags the specified row as deleted and saves to the page's data.
        /// </summary>
        /// <param name="rowId">The row id to delete</param>
        /// <remarks>Note that deleting a row does not internally decrement the number of rows on a page nor the total bytes used. Those values are only reset if the page is rebuilt.</remarks>
        public override void DeleteRow(uint rowId)
        {
            _basePage.DeleteRow(rowId);
        }

        public override PageUpdateRowResult TryUpdateRowData(RowValueGroup updatedRow, out uint updatedOffset)
        {
            return _basePage.TryUpdateRowData(updatedRow, out updatedOffset);
        }

        /// <summary>
        /// Adds a row to the Page's data, updates the total row count and the total bytes used (in the byte array)
        /// </summary>
        /// <param name="row">The row to be added</param>
        /// <exception cref="InvalidOperationException">Thrown if there is not enough room on the Page's data.</exception>
        /// <returns>The offset of where the row was added onto the page</returns>
        public override uint AddRow(Row row)
        {
            return _basePage.AddRow(row);
        }

        public override Row GetRow(RowAddress address)
        {
            return _basePage.GetRow(address);
        }

        /// <summary>
        /// Returns the specified row from a Page's data
        /// </summary>
        /// <param name="rowId">The row id to return</param>
        /// <returns>The specified row if found, otherwise NULL</returns>
        /// <remarks>Note that this function can be used to get rows forwarded to other pages.</remarks>
        public override Row GetRow(uint rowId)
        {
            return _basePage.GetRow(rowId);
        }

        /// <summary>
        /// The Id of the Page, read from the Page's data
        /// </summary>
        /// <returns>The id of this page</returns>
        public override uint PageId()
        {
            return _basePage.PageId();
        }

        /// <summary>
        /// The Db Id of the Page, read from the Page's data
        /// </summary>
        /// <returns></returns>
        public override Guid DbId()
        {
            return _basePage.DbId();
        }

        /// <summary>
        /// The Table Id of the Page, read from the Page's data
        /// </summary>
        /// <returns></returns>
        public override uint TableId()
        {
            return _basePage.TableId();
        }

        /// <summary>
        /// The total rows on the page, read from the Page's data
        /// </summary>
        /// <returns>The total rows on this page</returns>
        public override uint TotalRows()
        {
            return _basePage.TotalRows();
        }

        /// <summary>
        /// The total bytes used on this page, read from the Page's data
        /// </summary>
        /// <returns>The total bytes on this page</returns>
        public override uint TotalBytesUsed()
        {
            return _basePage.TotalBytesUsed();
        }

        public override DataPageType DataPageType()
        {
            return _basePage.DataPageType();
        }

        public override List<RowAddress> GetRowsWithValue(IRowValue value)
        {
            return _basePage.GetRowsWithValue(value);
        }

        public override List<RowAddress> GetRowIdsOnPage(bool includeDeletedRows = false)
        {
            return _basePage.GetRowIdsOnPage(includeDeletedRows);
        }

        public override bool HasValue(IRowValue value)
        {
            return _basePage.HasValue(value);
        }

        public override RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column)
        {
            return _basePage.GetValueAtAddress(address, column);
        }

        public override uint GetCountOfRowIdsOnPage(bool includeDeletedRows = false)
        {
            throw new NotImplementedException();
        }

        public override RowAddress[] GetRowAddressesWithValue(IRowValue value)
        {
            return _basePage.GetRowAddressesWithValue(value);
        }

        public override uint GetCountOfRowsWithValue(IRowValue value)
        {
            return _basePage.GetCountOfRowsWithValue(value);
        }

        public override bool IsDeleted()
        {
            return _basePage.IsDeleted();
        }

        public override void Delete()
        {
            _basePage.Delete();
        }

        public override void UnDelete()
        {
            _basePage.UnDelete();
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
