using Drummersoft.DrummerDB.Core.Structures.DbDebug;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    /// <summary>
    /// A data page in a database used for system actions (storing table schemas, users, etc.) The other type of data page is a <seealso cref="UserDataPage"/>.
    /// </summary>
    /// <seealso cref="Drummersoft.DrummerDB.Core.Structures.Interface.IPage" />
    /// <seealso cref="Drummersoft.DrummerDB.Core.Structures.Interface.IBaseDataPage" />
    /// <remarks>For more information on types of pages, see Page.md</remarks>
    internal abstract class SystemDataPage : IPage, IBaseDataPage
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
        public abstract List<RowAddress> GetRowIdsOnPage(bool includeDeletedRows = false);
        public abstract PageRowStatus GetRowStatus(int rowId);
        public abstract List<RowAddress> GetRowsWithValue(IRowValue value);
        public abstract bool HasValue(IRowValue value);
        public abstract bool IsFull(int rowSize);
        public abstract int PageId();
        public abstract int TableId();
        public abstract int TotalBytesUsed();
        public abstract int TotalRows();
        public abstract PageUpdateRowResult TryUpdateRowData(IRow row, out int updatedOffset);
        public abstract RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column);
        public abstract RowDebug GetDebugRow(int rowId);
        public abstract bool HasRow(int rowId);
        public abstract List<int> GetRowOffsets(int rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false);
        public abstract int GetCountOfRowIdsOnPage(bool includeDeletedRows = false);
        public abstract RowAddress[] GetRowAddressesWithValue(IRowValue value);
        public abstract int GetCountOfRowsWithValue(IRowValue value);
    }
}
