using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.DbDebug;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Drummersoft.DrummerDB.Core.Structures.PageParser;

namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    internal class BaseDataPage100 : BaseDataPage
    {
        /*
         * Page Byte Array Layout:
         * PageId PageType IsDeleted - page preamble
         * PageId PageType || TotalBytesUsed TotalRows DatabaseId TableId DataPageType - total data page premable
         * <rowDataStart> [row] [row] [row] [row] <rowDataEnd>
         * <rowDataEnd == [rowId = -1, IsLocal = true]>
         */

        #region Private Fields
        private LogService _log;

        //private ICacheManager _cache;

        /// <summary>
        /// The binary data of the page 
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// The dbId, tableId, and PageId
        /// </summary>
        private PageAddress _address;

        /// <summary>
        /// The schema for the table
        /// </summary>
        private ITableSchema _schema;

        /// <summary>
        /// The total bytes used in the page, not including the preamble. 
        /// </summary>
        /// <remarks>This includes rows that are deleted.</remarks>
        private int _totalBytesUsed = 0;

        /// <summary>
        /// The total number of rows in the page
        /// </summary>
        /// <remarks>This includes rows that are deleted.</remarks>
        private int _totalRows = 0;

        /// <summary>
        /// The type of data page
        /// </summary>
        private DataPageType _dataPageType;

        /// <summary>
        /// Version 100
        /// </summary>
        private int _V100 = Constants.DatabaseVersions.V100;
        #endregion

        #region Public Properties
        /// <summary>
        /// The binary data for this page
        /// </summary>
        public override byte[] Data => _data;
        public override PageAddress Address => _address;
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
        /// <param name="pageType">The type of page this is for</param>
        /// <param name="cache">The cache for network calls</param>
        public BaseDataPage100(PageAddress address, ITableSchema schema, DataPageType pageType)
        {
            _data = new byte[Constants.PAGE_SIZE];
            _address = address;
            _schema = schema;
            _totalBytesUsed = 0;
            _totalRows = 0;
            _dataPageType = pageType;

            SavePageIdToData();
            SavePageTypeToData();
            SaveDatabaseIdToData();
            SaveTableIdToData();
            SaveDataPageTypeToData();
        }


        /// <summary>
        /// Make a new Data Page in memory
        /// </summary>
        /// <param name="address">The address of this page</param>
        /// <param name="schema">The schema for the table this page is on</param>
        /// <param name="pageType">The type of page this is for</param>
        /// <param name="cache">The cache for network calls</param>
        public BaseDataPage100(PageAddress address, ITableSchema schema, DataPageType pageType, LogService log)
        {
            _data = new byte[Constants.PAGE_SIZE];
            _address = address;
            _schema = schema;
            _totalBytesUsed = 0;
            _totalRows = 0;
            _dataPageType = pageType;

            SavePageIdToData();
            SavePageTypeToData();
            SaveDatabaseIdToData();
            SaveTableIdToData();
            SaveDataPageTypeToData();

            _log = log;
        }

        /// <summary>
        /// Make a new page based on data from disk and attempts to set the Page Address and Type from the binary data
        /// </summary>
        /// <param name="data">The byte array from disk</param>
        /// <param name="schema">The schema for the table this page is on</param>
        /// <param name="cache">The cache for network calls</param>
        public BaseDataPage100(byte[] data, ITableSchema schema)
        {
            _data = data;
            _schema = schema;


            SetPageAddressFromData();
            SetPageTypeFromData();
            _address.SchemaId = schema.Schema.SchemaGUID;
            SetTotalRows();
            SetTotalBytesUsed();

        }
        #endregion

        #region Public Methods
        public override bool HasAllValues(IRowValue[] values)
        {
            List<RowAddress> rows = GetRowIdsOnPage();
            foreach (var row in rows)
            {
                bool rowHasAllValues = true;
                var rowData = GetRowAtOffset(row.RowOffset, row.RowId);

                foreach (var value in values)
                {
                    byte[] a;
                    byte[] b;

                    a = rowData.GetValueInByte(value.Column.Name);
                    b = value.GetValueInBinary(false, value.Column.IsNullable);

                    if (!DbBinaryConvert.BinaryEqual(a, b))
                    {
                        rowHasAllValues = false;
                        break;
                    }
                }

                if (rowHasAllValues)
                {
                    return true;
                }
            }

            return false;
        }

        public override RowAddress[] GetRowAddressesWithAllValues(IRowValue[] values)
        {
            List<RowAddress> result = new List<RowAddress>();

            List<RowAddress> rows = GetRowIdsOnPage();
            foreach (var row in rows)
            {
                bool rowHasAllValues = true;
                var rowData = GetRowAtOffset(row.RowOffset, row.RowId);

                foreach (var value in values)
                {
                    byte[] a;
                    byte[] b;

                    a = rowData.GetValueInByte(value.Column.Name);
                    b = value.GetValueInBinary(false, value.Column.IsNullable);

                    if (!DbBinaryConvert.BinaryEqual(a, b))
                    {
                        rowHasAllValues = false;
                        break;
                    }
                }

                if (rowHasAllValues)
                {
                    result.Add(row);
                }
            }

            return result.Distinct().ToArray();
        }

        public override bool IsDeleted()
        {
            var dataSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> isDeletedSpan = dataSpan.Slice(PageConstants.PageIsDeletedOffset(), PageConstants.SIZE_OF_IS_DELETED(_V100));
            var result = DbBinaryConvert.BinaryToBoolean(isDeletedSpan);

            return result;
        }

        public override void Delete()
        {
            var bDelete = BitConverter.GetBytes(true);
            bDelete.CopyTo(_data, PageConstants.PageIsDeletedOffset(_V100));
        }

        public override void UnDelete()
        {
            var bDelete = BitConverter.GetBytes(false);
            bDelete.CopyTo(_data, PageConstants.PageIsDeletedOffset(_V100));
        }
        public override int GetCountOfRowIdsOnPage(bool includeDeletedRows = false)
        {
            int totalCount = 0;
            var action = new ParsePageAction<int>(CountRows);
            ParsePageData(new ReadOnlySpan<byte>(_data), PageId(), action, -1, false, includeDeletedRows, ref totalCount);

            return totalCount;
        }

        public override List<RowAddress> GetRowIdsOnPage(bool includeDeletedRows = false)
        {
            var addresses = new List<RowAddress>();
            var action = new ParsePageAction<List<RowAddress>>(AddToRowAddresses);

            ParsePageData(new ReadOnlySpan<byte>(_data), PageId(), action, -1, false, includeDeletedRows, ref addresses);
            return addresses;
        }

        public override void ForwardRows(int rowId, int newPageId, int newPageOffset)
        {
            var locations = GetRowOffsets(rowId);

            foreach (var location in locations)
            {
                string debug = $"Forwarding Row {rowId.ToString()} at location {location.ToString()} to new location {newPageOffset.ToString()} on page new {newPageId.ToString()}. " +
                    $"Current page is {PageId().ToString()}";
                Debug.WriteLine(debug);

                var oldRow = GetRowAtOffset(location, rowId);
                oldRow.ForwardRow(newPageOffset, newPageId);
                var rowData = oldRow.GetRowInPageBinaryFormat();
                Array.Copy(rowData, 0, _data, location, rowData.Length);
            }
        }

        /// <summary>
        /// Determines if the specified row lives on this page
        /// </summary>
        /// <param name="rowId">The row Id to find</param>
        /// <returns><c>TRUE</c> if the <see cref="PageRowStatus"/> is <see cref="PageRowStatus.IsOnPage"/> or <see cref="PageRowStatus.IsOnPageAndForwardedOnSamePage"/>
        /// , otherwise <c>FALSE</c></returns>
        public override bool HasRow(int rowId)
        {
            PageRowStatus status = GetRowStatus(rowId);
            if (status == PageRowStatus.IsOnPage || status == PageRowStatus.IsOnPageAndForwardedOnSamePage)
            {
                return true;
            }
            return false;
        }

        public override PageRowStatus GetRowStatus(int rowId)
        {
            var offsets = GetRowOffsets(rowId);

            if (offsets.Count == 0)
            {
                return PageRowStatus.NotOnPage;
            }

            if (offsets.Count > 0)
            {
                var row = GetRowAtOffset(offsets.Max(), rowId);
                if (row.IsForwarded && row.ForwardedPageId == PageId())
                {
                    return PageRowStatus.IsOnPageAndForwardedOnSamePage;
                }

                if (row.IsForwarded && row.ForwardedPageId != PageId())
                {
                    return PageRowStatus.IsForwardedToOtherPage;
                }

                return PageRowStatus.IsOnPage;
            }

            return PageRowStatus.Unknown;
        }

        /// <summary>
        /// Checks to see if there is room left on the page for the specified row length
        /// </summary>
        /// <param name="rowSize">The length of the row</param>
        /// <returns>True if there is room on the page, otherwise false</returns>
        public override bool IsFull(int rowSize)
        {
            // total bytes used by rows on the page + the size of the row we wish to add + the number of bytes for the page preamble
            if ((_totalBytesUsed + rowSize + DataPageConstants.RowDataStartOffset(_V100)) > _data.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Flags the specified row as deleted and saves to the page's data.
        /// </summary>
        /// <param name="rowId">The row id to delete</param>
        /// <remarks>Note that deleting a row does not internally decrement the number of rows on a page nor the total bytes used. Those values are only reset if the page is rebuilt.</remarks>
        public override void DeleteRow(int rowId)
        {
            var offsets = GetRowOffsets(rowId);
            foreach (var offset in offsets)
            {
                var row = GetRowAtOffset(offset, rowId);
                row.IsDeleted = true;
                var rowData = row.GetRowInPageBinaryFormat();
                Array.Copy(rowData, 0, _data, offset, rowData.Length);
            }
        }

        public override PageUpdateRowResult TryUpdateRowData(IRow updatedRow, out int updatedOffset)
        {
            PageUpdateRowResult result = PageUpdateRowResult.Unknown;
            int rowId = updatedRow.Id;
            int existingRowOffset = GetRowOffsets(rowId).Max();
            IRow existingRow = GetRow(rowId);

            if (updatedRow.Size() == existingRow.Size())
            {
                // update in place
                byte[] rowData = updatedRow.GetRowInPageBinaryFormat();
                Array.Copy(rowData, 0, _data, existingRowOffset, rowData.Length);
                updatedOffset = existingRowOffset;
                result = PageUpdateRowResult.Success;
            }
            else
            {
                if (!(IsFull(updatedRow.Size())))
                {
                    // add the row to the page
                    int newRowOffset = AppendRowToData(updatedRow);

                    string debug = $"Appending Row {updatedRow.Id.ToString()} at location: {newRowOffset.ToString()}";
                    Debug.WriteLine(debug);

                    List<int> locations = GetRowOffsets(rowId);
                    locations.Remove(newRowOffset);

                    // if we have not forwarded this row before
                    if (locations.Count == 0)
                    {
                        existingRow.ForwardRow(newRowOffset, PageId());
                        byte[] rowData = existingRow.GetRowInPageBinaryFormat();
                        Array.Copy(rowData, 0, _data, existingRowOffset, rowData.Length);
                        result = PageUpdateRowResult.Success;
                        updatedOffset = newRowOffset;
                    }
                    else // need to loop thru all the locations and update them to the new offset
                    {
                        int pageId = PageId();

                        foreach (var location in locations)
                        {
                            debug = $"Forwarding Row {rowId.ToString()} at location {location.ToString()} to new location {newRowOffset.ToString()}";
                            Debug.WriteLine(debug);

                            IRow oldRow = GetRowAtOffset(location, rowId);
                            oldRow.ForwardRow(newRowOffset, pageId);
                            byte[] rowData = oldRow.GetRowInPageBinaryFormat();
                            Array.Copy(rowData, 0, _data, location, rowData.Length);
                        }

                        result = PageUpdateRowResult.Success;
                        updatedOffset = newRowOffset;
                    }
                }
                else
                {
                    result = PageUpdateRowResult.NotEnoughRoom;
                    updatedOffset = 0;
                }
            }

            return result;

            /*
             * if the row size is the same as the row already in data, then we can do an in place update
             * if the row size is less than the row already in data, then we have a problem because we have unused bytes and the counts now will be off
             * if the row size is greater than the row already in data, then we need to see if we can append the data (making sure adding the modified row won't fill the page) and figure out what to do with the old location of the row (have a forwarding offset?)
             */
        }

        /// <summary>
        /// Adds a row to the Page's data, updates the total row count and the total bytes used (in the byte array)
        /// </summary>
        /// <param name="row">The row to be added</param>
        /// <exception cref="InvalidOperationException">Thrown if there is not enough room on the Page's data.</exception>
        /// <returns>The offset of where the row was added onto the page</returns>
        public override int AddRow(IRow row)
        {
            int offset = 0;

            if (!IsFull(row.Size()))
            {
                offset = AppendRowToData(row);
                _totalRows++;
                SaveTotalRows();
                SaveTotalBytesUsedToData();
            }
            else
            {
                throw new InvalidOperationException("There is no more room on the Page");
            }

            return offset;
        }

        public override IRow GetRow(RowAddress address)
        {
            return GetRowAtOffset(address.RowOffset, address.RowId);
        }

        /// <summary>
        /// Returns the specified row from a Page's data
        /// </summary>
        /// <param name="rowId">The row id to return</param>
        /// <returns>The specified row if found, otherwise NULL</returns>
        /// <remarks>Note that this function can be used to get rows forwarded to other pages.</remarks>
        public override IRow GetRow(int rowId)
        {
            IRow row = null;
            List<int> offsets = GetRowOffsets(rowId, false, true);

            int rowOffset;
            if (offsets.Count == 0)
            {
                return null;
            }
            else
            {
                rowOffset = offsets.Max();
            }

            if (rowOffset != 0)
            {
                row = GetRowAtOffset(rowOffset, rowId);

                if (row.IsForwarded && row.ForwardedPageId == PageId())
                {
                    row = GetRowAtOffset(row.ForwardOffset, rowId);
                }
            }

            return row;
        }

        public override RowDebug GetDebugRow(int rowId)
        {
            RowDebug row = null;
            List<int> offsets = GetRowOffsets(rowId, true);

            int rowOffset;
            if (offsets.Count == 0)
            {
                return null;
            }
            else
            {
                rowOffset = offsets.Max();
            }

            if (rowOffset != 0)
            {
                row = GetDebugRowAtOffset(rowOffset, rowId);

                if (row.IsForwarded() && row.ForwardedPageId() == PageId())
                {
                    row = GetDebugRowAtOffset(row.ForwardOffset(), rowId);
                }
            }

            return row;
        }

        /// <summary>
        /// The Id of the Page, read from the Page's data
        /// </summary>
        /// <returns>The id of this page</returns>
        public override int PageId()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> idBytes = idSpan.Slice(PageConstants.PageIdOffset(), PageConstants.SIZE_OF_PAGE_ID(_V100));
            var result = BitConverter.ToInt32(idBytes);
            return result;
        }

        /// <summary>
        /// The Db Id of the Page, read from the Page's data
        /// </summary>
        /// <returns></returns>
        public override Guid DbId()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> idBytes = idSpan.Slice(DataPageConstants.DatabaseIdOffset(_V100), DataPageConstants.SIZE_OF_DATABASE_ID(_V100));
            var result = DbBinaryConvert.BinaryToGuid(idBytes);
            return result;
        }

        /// <summary>
        /// The Table Id of the Page, read from the Page's data
        /// </summary>
        /// <returns></returns>
        public override int TableId()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> idBytes = idSpan.Slice(DataPageConstants.TableIdOffset(_V100), DataPageConstants.SIZE_OF_TABLE_ID(_V100));
            var result = BitConverter.ToInt32(idBytes);
            return result;
        }

        /// <summary>
        /// The total rows on the page, read from the Page's data
        /// </summary>
        /// <returns>The total rows on this page</returns>
        public override int TotalRows()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> idBytes = idSpan.Slice(DataPageConstants.TotalRowsOffset(_V100), DataPageConstants.SIZE_OF_TOTAL_ROWS(_V100));
            var result = BitConverter.ToInt32(idBytes);
            return result;
        }

        /// <summary>
        /// The total bytes used on this page, read from the Page's data
        /// </summary>
        /// <returns>The total bytes on this page</returns>
        public override int TotalBytesUsed()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> idBytes = idSpan.Slice(DataPageConstants.TotalBytesUsedOffset(_V100), DataPageConstants.SIZE_OF_TOTAL_ROWS(_V100));
            var result = BitConverter.ToInt32(idBytes);
            return result;
        }

        /// <summary>
        /// Returns the type of data page, read from the Page's data
        /// </summary>
        /// <returns>The type of data page (of enum type DataPageType)</returns>
        public override DataPageType DataPageType()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> idBytes = idSpan.Slice(DataPageConstants.DataPageTypeOffset(_V100), DataPageConstants.SIZE_OF_DATA_PAGE_TYPE(_V100));
            var result = BitConverter.ToInt32(idBytes);

            if (System.Enum.IsDefined(typeof(DataPageType), result))
            {
                DataPageType type = (DataPageType)result;
                if (type != Enum.DataPageType.System && type != Enum.DataPageType.User)
                {
                    throw new InvalidOperationException("Tried to load incorrect data page type for data page");
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown Data Page Type");
            }

            return (DataPageType)result;
        }


        public override int GetCountOfRowsWithValue(IRowValue value)
        {
            int count = 0;

            List<RowAddress> rows = GetRowIdsOnPage();
            foreach (var row in rows)
            {
                var rowData = GetRowAtOffset(row.RowOffset, row.RowId);

                if (!rowData.IsForwarded && !rowData.IsDeleted)
                {
                    byte[] a;
                    byte[] b;

                    a = rowData.GetValueInByte(value.Column.Name);
                    b = value.GetValueInBinary(false, value.Column.IsNullable);

                    if (DbBinaryConvert.BinaryEqual(a, b))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public override List<RowAddress> GetRowsWithValue(IRowValue value)
        {
            var result = new List<RowAddress>();

            List<RowAddress> rows = GetRowIdsOnPage();
            foreach (var row in rows)
            {
                var rowData = GetRowAtOffset(row.RowOffset, row.RowId);

                byte[] a;
                byte[] b;

                if (!rowData.IsForwarded && !rowData.IsDeleted)
                {
                    a = rowData.GetValueInByte(value.Column.Name);
                    b = value.GetValueInBinary(false, value.Column.IsNullable);

                    if (DbBinaryConvert.BinaryEqual(a, b))
                    {
                        result.Add(row);
                    }
                }
            }

            return result;
        }

        public override RowAddress[] GetRowAddressesWithValue(IRowValue value)
        {
            int count = GetCountOfRowsWithValue(value);
            var result = new RowAddress[count];
            int index = 0;

            List<RowAddress> rows = GetRowIdsOnPage();
            foreach (var row in rows)
            {
                var rowData = GetRowAtOffset(row.RowOffset, row.RowId);

                byte[] a;
                byte[] b;

                a = rowData.GetValueInByte(value.Column.Name);
                b = value.GetValueInBinary();

                if (DbBinaryConvert.BinaryEqual(a, b))
                {
                    result[index] = row;
                    index++;
                }
            }

            return result;
        }


        public override bool HasValue(IRowValue value)
        {
            List<RowAddress> rows = GetRowIdsOnPage();
            foreach (var row in rows)
            {

                //RowDebug debug = GetDebugRowAtOffset(row.RowOffset, row.RowId);

                IRow rowData = GetRowAtOffset(row.RowOffset, row.RowId);

                if (DbBinaryConvert.BinaryEqual(rowData.GetValueInByteSpan(value.Column.Name), value.GetValueInByteSpan()))
                {
                    return true;
                }

                /*
                
                Re-write to see if better in performance, also this was silly to compare strings

                if (rowData.GetValueInString(value.Column.Name).ToUpper() == value.GetValueInString().ToUpper())
                {
                    return true;
                }
                */
            }

            return false;
        }

        public override RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column)
        {
            var span = new ReadOnlySpan<byte>(_data);
            int valueLocation = address.RowOffset + address.ValueOffset;

            return new RowValue(column, span.Slice(valueLocation, address.ParseLength));
        }

        /// <summary>
        /// Returns a list of offsets for the specified row. If the returned list is empty, it means the row is not on this page. Note that this will return the offsets for a row forwarded to other pages.
        /// </summary>
        /// <param name="rowId">The row to find</param>
        /// <param name="stopAtFirstForward">Stops searching data on page at the first row that has a forwarding offset</param>
        /// <returns>A list of offsets from the start of the page for the specified row. If the list is empty, it means the row is not on this page.</returns>
        /// <remarks>The reason a row may have multiple offsets is because of variable size updates to a row. In other words, every time a row updates, if the size changes, we leave the
        /// existing row in place and update the ForwardOffset of the row. The highest offset in the returned list is usually the row version that has not been forwarded, 
        /// i.e. the latest version of the row. Be sure to validate this when calling this function.
        /// 
        /// Note that this returns forwarded rows that may be on other pages.</remarks>
        public override List<int> GetRowOffsets(int rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false)
        {
            var offsets = new List<int>();
            var action = new ParsePageAction<List<int>>(AddParsedRowToOffsets);
            ParsePageData(new ReadOnlySpan<byte>(_data), PageId(), action, rowId, stopAtFirstForward, includeDeletedRows, ref offsets);

            return offsets;
        }
        #endregion

        #region Private Methods
        private void CountRows(int pageId, Row row, int offset, int targetRowId, ref int totalCount)
        {
            totalCount++;
        }

        private void AddToRowAddresses(int pageId, Row row, int offset, int targetRowId, ref List<RowAddress> addresses)
        {
            if (row.IsForwarded)
            {
                if (row.ForwardedPageId == pageId)
                {
                    addresses.Add(new RowAddress(pageId, row.Id, offset, Guid.Empty));
                }
            }
            else
            {
                addresses.Add(new RowAddress(pageId, row.Id, offset, Guid.Empty));
            }

        }

        private void AddParsedRowToOffsets(int pageId, Row row, int offset, int targetRowId, ref List<int> items)
        {
            if (targetRowId == row.Id)
            {
                items.Add(offset);
            }
        }

        private void SetTotalRows()
        {
            _totalRows = TotalRows();
        }

        private void SetTotalBytesUsed()
        {
            _totalBytesUsed = TotalBytesUsed();
        }

        private RowDebug GetDebugRowAtOffset(int offset, int rowId)
        {
            RowDebug row = null;
            var span = new ReadOnlySpan<byte>(_data);

            string pageData = BitConverter.ToString(span.ToArray());
            Debug.WriteLine(pageData);

            if (offset != 0)
            {
                int runningTotal = offset;
                var preamble = span.Slice(offset, RowConstants.LengthOfPreamble());
                RowDebug item = new RowDebug();

                item.SetPreamble(preamble);
                item.SetSchema(_schema as TableSchema);

                if (item.RowId() == rowId)
                {
                    row = item;
                    if (row.IsLocal())
                    {
                        ReadOnlySpan<byte> sizeOfRowData = span.Slice(runningTotal + RowConstants.LengthOfPreamble(), RowConstants.SIZE_OF_ROW_SIZE);

                        row.SetRowSize(sizeOfRowData);

                        int sizeOfRow = DbBinaryConvert.BinaryToInt(sizeOfRowData);
                        int rowdataSlice = sizeOfRow - RowConstants.LengthOfPreamble() - RowConstants.SIZE_OF_ROW_SIZE;
                        runningTotal += RowConstants.LengthOfPreamble() + RowConstants.SIZE_OF_ROW_SIZE;

                        string stringData = BitConverter.ToString(span.Slice(runningTotal, rowdataSlice).ToArray());
                        Debug.WriteLine(stringData);

                        row.SetRowData(span.Slice(runningTotal, rowdataSlice));
                    }
                    else
                    {
                        throw new NotImplementedException("remote row data has not been implemented");
                        // TODO - haven't tested this yet, this is prototyping
                        var binaryParticipantId = span.Slice(runningTotal + RowConstants.SIZE_OF_PARTICIPANT_ID);

                        row.SetParticipant(binaryParticipantId);

                        Guid partipantId = DbBinaryConvert.BinaryToGuid(binaryParticipantId);
                        //row.ParticipantId = partipantId;

                        //byte[] data = _cache.GetRemoteRowData(partipantId, new SQLAddress { DatabaseId = _address.DatabaseId, TableId = _address.TableId, PageId = PageId(), RowId = row.Id, RowOffset = 0 });
                        //row.SetRowData(_schema, data);

                        // ideally, we shouldn't have DrummerDB.Structures take a dependency on Network
                        // maybe we should have DrummerDB.Databases take a dependency on Network?
                        // In theory, we would have the database ask network to go get the remote bytes

                        runningTotal += RowConstants.SIZE_OF_PARTICIPANT_ID + RowConstants.LengthOfPreamble();
                    }
                }
            }

            return row;
        }

        /// <summary>
        /// Returns a row at the specified offset
        /// </summary>
        /// <param name="offset">The offset of the row</param>
        /// <param name="rowId">The row id to get</param>
        /// <returns>A row from the specified offset on the page's data</returns>
        private IRow GetRowAtOffset(int offset, int rowId)
        {
            IRow row = null;
            var span = new ReadOnlySpan<byte>(_data);

            if (offset != 0)
            {
                int runningTotal = offset;
                var preamble = span.Slice(offset, RowConstants.LengthOfPreamble());
                Row item = new Row(preamble);
                if (item.Id == rowId)
                {
                    row = item;
                    if (row.IsLocal)
                    {
                        ReadOnlySpan<byte> sizeOfRowData = span.Slice(runningTotal + RowConstants.LengthOfPreamble(), RowConstants.SIZE_OF_ROW_SIZE);
                        int sizeOfRow = DbBinaryConvert.BinaryToInt(sizeOfRowData);
                        int rowdataSlice = sizeOfRow - RowConstants.LengthOfPreamble() - RowConstants.SIZE_OF_ROW_SIZE;
                        runningTotal += RowConstants.LengthOfPreamble() + RowConstants.SIZE_OF_ROW_SIZE;
                        row.SetRowData(_schema, span.Slice(runningTotal, rowdataSlice));
                    }
                    else
                    {
                        ReadOnlySpan<byte> sizeOfRowData = span.Slice(runningTotal + RowConstants.LengthOfPreamble(), RowConstants.SIZE_OF_ROW_SIZE);
                        int sizeOfRow = DbBinaryConvert.BinaryToInt(sizeOfRowData);
                        int rowdataSlice = sizeOfRow - RowConstants.LengthOfPreamble() - RowConstants.SIZE_OF_ROW_SIZE;
                        runningTotal += RowConstants.LengthOfPreamble() + RowConstants.SIZE_OF_ROW_SIZE;

                        // format needs to be 
                        // participant id
                        // length of data hash (int - 4 bytes)
                        // data hash

                        var remoteData = span.Slice(runningTotal, rowdataSlice);
                        int remoteDataTotal = runningTotal;

                        runningTotal += sizeOfRow;

                        var binaryParticipantId = span.Slice(remoteDataTotal, RowConstants.SIZE_OF_PARTICIPANT_ID);
                        Guid partipantId = DbBinaryConvert.BinaryToGuid(binaryParticipantId);
                        row.ParticipantId = partipantId;

                        remoteDataTotal += RowConstants.SIZE_OF_PARTICIPANT_ID;

                        int dataHashLength = DbBinaryConvert.BinaryToInt(span.Slice(remoteDataTotal, Constants.SIZE_OF_INT));
                        remoteDataTotal += Constants.SIZE_OF_INT;

                        var hashData = span.Slice(remoteDataTotal, dataHashLength).ToArray();
                        row.Hash = hashData;

                        remoteDataTotal += dataHashLength;
                    }
                }
            }

            return row;
        }




        // TO DO: In the future, for remote rows, going to have a dependency on the Communication system
        // or intercept the saving of the row to send the row to the particiapnt, and then save only the preamble data + participant id
        /// <summary>
        /// Appends the row to the Page's data, and increments the field _totalBytesUsed by the size of the row
        /// </summary>
        /// <param name="row">The row to be added</param>
        /// <returns>The offset where the row was added</returns>
        private int AppendRowToData(IRow row)
        {
            var rowData = row.GetRowInPageBinaryFormat();
            int nextAvailableRowOffset = DataPageConstants.RowDataStartOffset(_V100) + _totalBytesUsed;

            Array.Copy(rowData, 0, _data, nextAvailableRowOffset, rowData.Length);
            _totalBytesUsed += rowData.Length;

            return nextAvailableRowOffset;
        }

        /// <summary>
        /// Saves the field _totalRows to the Page's data
        /// </summary>
        private void SaveTotalRows()
        {
            var array = DbBinaryConvert.IntToBinary(_totalRows);
            Array.Copy(array, 0, _data, DataPageConstants.TotalRowsOffset(_V100), array.Length);
        }

        /// <summary>
        /// Saves the field _totalBytesUsed to the Page's data
        /// </summary>
        private void SaveTotalBytesUsedToData()
        {
            var array = DbBinaryConvert.IntToBinary(_totalBytesUsed);
            Array.Copy(array, 0, _data, DataPageConstants.TotalBytesUsedOffset(_V100), array.Length);
        }

        /// <summary>
        /// Saves the Page's Id to the Page's data
        /// </summary>
        private void SavePageIdToData()
        {
            var bId = BitConverter.GetBytes(_address.PageId);
            bId.CopyTo(_data, PageConstants.PageIdOffset());
        }

        /// <summary>
        /// Saves the page type to Pages's data
        /// </summary>
        private void SavePageTypeToData()
        {
            var bType = DbBinaryConvert.IntToBinary((int)Type);
            Array.Copy(bType, 0, _data, PageConstants.PageTypeOffset(_V100), bType.Length);
        }

        private void SaveDataPageTypeToData()
        {
            var bType = DbBinaryConvert.IntToBinary((int)_dataPageType);
            Array.Copy(bType, 0, _data, DataPageConstants.DataPageTypeOffset(_V100), bType.Length);
        }

        /// <summary>
        /// Saves the page's database id to data
        /// </summary>
        private void SaveDatabaseIdToData()
        {
            var bDbId = DbBinaryConvert.GuidToBinary(_address.DatabaseId);
            Array.Copy(bDbId, 0, _data, DataPageConstants.DatabaseIdOffset(_V100), bDbId.Length);
        }

        /// <summary>
        /// Saves the page's table id to data
        /// </summary>
        private void SaveTableIdToData()
        {
            var bTbId = DbBinaryConvert.IntToBinary(_address.TableId);
            Array.Copy(bTbId, 0, _data, DataPageConstants.TableIdOffset(_V100), bTbId.Length);
        }

        /// <summary>
        /// Sets the Page Address From Data
        /// </summary>
        private void SetPageAddressFromData()
        {
            int pageId = PageId();
            int tableId = TableId();
            Guid dbId = DbId();

            _address = new PageAddress(dbId, tableId, pageId, Guid.Empty);
        }

        /// <summary>
        /// Sets the Page Type From Data
        /// </summary>
        private void SetPageTypeFromData()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            var idBytes = idSpan.Slice(PageConstants.PageTypeOffset(_V100), PageConstants.SIZE_OF_PAGE_TYPE(_V100));
            var result = BitConverter.ToInt32(idBytes);

            if (System.Enum.IsDefined(typeof(PageType), result))
            {
                PageType type = (PageType)result;
                if (type != PageType.Data)
                {
                    // throw new InvalidOperationException("Tried to load incorrect page type for data page");
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown Page Type");
            }
        }



        #endregion
    }

}
