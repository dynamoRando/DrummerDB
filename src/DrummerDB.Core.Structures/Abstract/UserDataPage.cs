﻿using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    /// <summary>
    /// A type of data page meant for usually holding user defined structures (tables, etc.)
    /// </summary>
    /// <seealso cref="Drummersoft.DrummerDB.Core.Structures.Interface.IPage" />
    /// <seealso cref="Drummersoft.DrummerDB.Core.Structures.Interface.IBaseDataPage" />
    /// <remarks>A UserDataPage is also used in the <seealso cref="SystemDatabase"/> to hold various system information. This is the only exception
    /// to holding non-user defined data structures.</remarks>
    internal abstract class UserDataPage : IPage, IBaseDataPage
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
        public abstract PageRowStatus GetRowStatus(int rowId);
        public abstract bool IsFull(int rowSize);
        public abstract int PageId();
        public abstract int TableId();
        public abstract int TotalBytesUsed();
        public abstract int TotalRows();
        public abstract PageUpdateRowResult TryUpdateRowData(IRow row, out int updatedOffset);
        public abstract List<RowAddress> GetRowsWithValue(IRowValue value);
        public abstract List<RowAddress> GetRowIdsOnPage(bool includeDeletedRows = false);
        public abstract bool HasValue(IRowValue value);
        public abstract RowValue GetValueAtAddress(ValueAddress address, ColumnSchema column);
        
        public abstract bool HasRow(int rowId);
        public abstract List<int> GetRowOffsets(int rowId, bool stopAtFirstForward = false, bool includeDeletedRows = false);
        public abstract int GetCountOfRowIdsOnPage(bool includeDeletedrows = false);
        public abstract RowAddress[] GetRowAddressesWithValue(IRowValue value);
        public abstract int GetCountOfRowsWithValue(IRowValue value);
        public abstract bool IsDeleted();
        public abstract void Delete();
        public abstract void UnDelete();
        public abstract RowAddress[] GetRowAddressesWithAllValues(IRowValue[] values);
        public abstract bool HasAllValues(IRowValue[] values);
        public abstract PageAddress Address { get; }
    }
}
