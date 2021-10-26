﻿using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Tests.Mocks
{
    internal class MockCacheManager : ICacheManager
    {
        int _rowId = 0;
    
        public void AddSystemDbSystemPage(ISystemPage page)
        {
            throw new NotImplementedException();
        }

        public void AddUserDbSystemPage(ISystemPage page)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRow(IRow row, TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRow(int rowId, TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRow(RowAddress address)
        {
            throw new NotImplementedException();
        }

        public List<RowAddress> FindRowAddressesWithValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsWithAllValues(TreeAddress address, ref RowValueSearch[] values)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsWithAllValues(TreeAddress address, List<RowValueSearch> values)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsWithValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsWithValue(TreeAddress address, RowValueStruct value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsWithValue(TreeAddress address, RowValueSearch value)
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseName(Guid dbId)
        {
            throw new NotImplementedException();
        }

        public int GetMaxRowIdForTree(TreeAddress address)
        {
            _rowId++;
            return _rowId;
        }

        public IRow GetRow(int rowId, TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public IRow GetRow(RowAddress address, TreeAddress treeAddress)
        {
            throw new NotImplementedException();
        }

        public RowAddress GetRowAddress(TreeAddress treeAddress, int rowId)
        {
            throw new NotImplementedException();
        }

        public List<RowAddress> GetRows(TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public TreeStatus GetTreeMemoryStatus(TreeAddress address)
        {
            return TreeStatus.Ready;
        }

        public TreeStatus GetTreeSizeStatus(TreeAddress address, int sizeOfDataToAdd)
        {
            throw new NotImplementedException();
        }

        public ResultsetValue GetValueAtAddress(in ValueAddress address, ColumnSchema column)
        {
            throw new NotImplementedException();
        }

        public List<ValueAddress> GetValues(TreeAddress address, string columnName, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public List<ValueAddress> GetValuesForColumnByRows(TreeAddress address, string columnName, ITableSchema schema, List<RowAddress> rows)
        {
            throw new NotImplementedException();
        }

        public bool HasUserDataAddress(TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public bool HasUserDataPage(PageAddress address)
        {
            throw new NotImplementedException();
        }

        public bool HasValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public bool HasValue(TreeAddress address, RowValueSearch value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public CacheAddRowResult TryAddRow(IRow row, TreeAddress address, ITableSchema schema, out int pageId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRow(IRow row, TreeAddress address, ITableSchema schema, out int pageId)
        {
            throw new NotImplementedException();
        }

        public void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly)
        {
            throw new NotImplementedException();
        }

        public void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendlyName)
        {
            throw new NotImplementedException();
        }

        public int[] UserDataGetContainerPages(TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public IBaseDataPage UserDataGetPage(PageAddress address)
        {
            throw new NotImplementedException();
        }

        public bool UserSystemCacheHasDatabase(Guid dbId)
        {
            throw new NotImplementedException();
        }
    }
}
