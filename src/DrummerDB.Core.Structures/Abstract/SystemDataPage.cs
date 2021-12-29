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
        public abstract uint AddRow(Row row);
        public abstract DataPageType DataPageType();
        public abstract Guid DbId();
        public abstract void Delete();

        public abstract void DeleteRow(uint rowId);
        public abstract void ForwardRows(uint rowId, uint newPageId, uint newPageOffset);
        public abstract uint GetCountOfRowIdsOnPage(bool includeDeletedRows = false);

        public abstract uint GetCountOfRowsWithValue(IRowValue value);

        public abstract Row GetRow(uint rowId);
        public abstract Row GetRow(RowAddress address);
        public abstract RowAddress[] GetRowAddressesWithAllValues(IRowValue[] values);

        public abstract RowAddress[] GetRowAddressesWithValue(IRowValue value);

        public abstract List<RowAddress> GetRowIdsOnPage(bool includeDeletedRows = false);
        public abstract List<uint> GetRowOffsets(uint rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false);

        public abstract PageRowStatus GetRowStatus(uint rowId);
        public abstract List<RowAddress> GetRowsWithValue(IRowValue value);
        public abstract RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column);

        public abstract bool HasAllValues(IRowValue[] values);

        public abstract bool HasRow(uint rowId);

        public abstract bool HasValue(IRowValue value);
        public abstract bool IsDeleted();

        public abstract bool IsFull(uint rowSize);
        public abstract uint PageId();
        public abstract uint TableId();
        public abstract uint TotalBytesUsed();
        public abstract uint TotalRows();
        public abstract PageUpdateRowResult TryUpdateRowData(RowValueGroup row, out uint updatedOffset);
        public abstract void UnDelete();
    }
}
