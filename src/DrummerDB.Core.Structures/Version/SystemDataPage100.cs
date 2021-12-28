using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Factory;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    internal class SystemDataPage100 : SystemDataPage
    {
        #region Private Fields
        private BaseDataPage100 _basePage;
        #endregion

        #region Public Properties
        /// <summary>
        /// The binary data for this page
        /// </summary>
        public override byte[] Data => _basePage.Data;

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
        public SystemDataPage100(PageAddress address, SystemTableSchema schema)
        {
            _basePage = BaseDataPageFactory.GetBaseDataPage100(address, schema, Enum.DataPageType.System);
        }

        /// <summary>
        /// Make a new page based on data from disk and attempts to set the Page Address and Type from the binary data
        /// </summary>
        /// <param name="data">The byte array from disk</param>
        /// <param name="schema">The schema for the table this page is on</param>
        public SystemDataPage100(byte[] data, SystemTableSchema schema)
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
        public override List<int> GetRowOffsets(int rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false)
        {
            return _basePage.GetRowOffsets(rowId, stopAtFirstForward, includeDeletedRows);
        }

        public override bool HasRow(int rowId)
        {
            return _basePage.HasRow(rowId);
        }
    
        public override void ForwardRows(int rowId, int newPageId, int newPageOffset)
        {
            _basePage.ForwardRows(rowId, newPageId, newPageOffset);
        }

        public override PageRowStatus GetRowStatus(int rowId)
        {
            return _basePage.GetRowStatus(rowId);
        }

        /// <summary>
        /// Checks to see if there is room left on the page for the specified row length
        /// </summary>
        /// <param name="rowSize">The length of the row</param>
        /// <returns>True if there is room on the page, otherwise false</returns>
        public override bool IsFull(int rowSize)
        {
            return _basePage.IsFull(rowSize);
        }

        /// <summary>
        /// Flags the specified row as deleted and saves to the page's data.
        /// </summary>
        /// <param name="rowId">The row id to delete</param>
        /// <remarks>Note that deleting a row does not internally decrement the number of rows on a page nor the total bytes used. Those values are only reset if the page is rebuilt.</remarks>
        public override void DeleteRow(int rowId)
        {
            _basePage.DeleteRow(rowId);
        }

        public override PageUpdateRowResult TryUpdateRowData(IRow updatedRow, out int updatedOffset)
        {
            return _basePage.TryUpdateRowData(updatedRow, out updatedOffset);
        }

        /// <summary>
        /// Adds a row to the Page's data, updates the total row count and the total bytes used (in the byte array)
        /// </summary>
        /// <param name="row">The row to be added</param>
        /// <exception cref="InvalidOperationException">Thrown if there is not enough room on the Page's data.</exception>
        /// <returns>The offset of where the row was added onto the page</returns>
        public override int AddRow(IRow row)
        {
            return _basePage.AddRow(row);
        }

        /// <summary>
        /// Returns the specified row from a Page's data
        /// </summary>
        /// <param name="rowId">The row id to return</param>
        /// <returns>The specified row if found, otherwise NULL</returns>
        /// <remarks>Note that this function can be used to get rows forwarded to other pages.</remarks>
        public override IRow GetRow(int rowId)
        {
            return _basePage.GetRow(rowId);
        }

        public override IRow GetRow(RowAddress address)
        {
            return _basePage.GetRow(address);
        }

        /// <summary>
        /// The Id of the Page, read from the Page's data
        /// </summary>
        /// <returns>The id of this page</returns>
        public override int PageId()
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
        public override int TableId()
        {
            return _basePage.TableId();
        }

        /// <summary>
        /// The total rows on the page, read from the Page's data
        /// </summary>
        /// <returns>The total rows on this page</returns>
        public override int TotalRows()
        {
            return _basePage.TotalRows();
        }

        /// <summary>
        /// The total bytes used on this page, read from the Page's data
        /// </summary>
        /// <returns>The total bytes on this page</returns>
        public override int TotalBytesUsed()
        {
            return _basePage.TotalBytesUsed();
        }

        /// <summary>
        /// Returns the type of data page, read from the Page's data
        /// </summary>
        /// <returns>The type of data page (of enum type DataPageType)</returns>
        public override DataPageType DataPageType()
        {
            return _basePage.DataPageType();
        }

        public override List<RowAddress> GetRowsWithValue(IRowValue value)
        {
            throw new NotImplementedException();
        }

        public override List<RowAddress> GetRowIdsOnPage(bool includeDeletedRows = false)
        {
            throw new NotImplementedException();
        }

        public override bool HasValue(IRowValue value)
        {
            throw new NotImplementedException();
        }

        public override RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column)
        {
            throw new NotImplementedException();
        }

        public override int GetCountOfRowIdsOnPage(bool includeDeletedRows = false)
        {
            throw new NotImplementedException();
        }

        public override RowAddress[] GetRowAddressesWithValue(IRowValue value)
        {
            throw new NotImplementedException();
        }

        public override int GetCountOfRowsWithValue(IRowValue value)
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
