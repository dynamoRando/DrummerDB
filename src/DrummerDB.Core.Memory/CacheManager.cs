using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.Factory;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Drummersoft.DrummerDB.Core.Memory
{
    /// <summary>
    /// Responsible for maintaining objects in memory. 
    /// </summary>
    internal class CacheManager : ICacheManager
    {
        #region Private Fields
        // internal objects

        private SystemCache _systemSystemCache;
        private LogService _log;

        // structures for user databases
        private DataCache _userDataCache;
        private SystemCache _userSystemCache;
        // structures for system databases?
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs an instance of a cache manager
        /// </summary>
        /// <param name="storage">A storage manager for managing objects on disk</param>
        internal CacheManager()
        {
            _userDataCache = new DataCache();
            _userSystemCache = new SystemCache();
            _systemSystemCache = new SystemCache();

        }

        internal CacheManager(LogService log)
        {
            _userDataCache = new DataCache(log);
            _userSystemCache = new SystemCache(log);
            _systemSystemCache = new SystemCache(log);
            _log = log;
        }
        #endregion

        #region Public Methods
        public bool TryRemoveTree(TreeAddress address)
        {
            return _userDataCache.TryRemoveTree(address);
        }

        public List<PageAddress> GetPageAddressesForTree(TreeAddress address)
        {
            return _userDataCache.GetPageAddressesForTree(address);
        }

        public void AddSystemDbSystemPage(ISystemPage page)
        {
            _systemSystemCache.AddSystemPage(page.DatabaseId, page);
        }

        public void AddUserDbSystemPage(ISystemPage page)
        {
            _userSystemCache.AddSystemPage(page.DatabaseId, page);
        }

        public bool DeleteRow(uint rowId, TreeAddress address)
        {
            return _userDataCache.TryDeleteRow(rowId, address);
        }

        public bool DeleteRow(IRow row, TreeAddress address)
        {
            return DeleteRow(row.Id, address);
        }

        public List<RowAddress> GetRowAddressesWithValue(TreeAddress address, RowValue value)
        {
            var result = new List<RowAddress>();

            if (!_userDataCache.HasTree(address))
            {
                return result;
            }

            return _userDataCache.GetRowAddressesWithValue(address, value);
        }

        public bool HasRowsWithAllValues(TreeAddress address, ref IRowValue[] values)
        {
            if (!_userDataCache.HasTree(address))
            {
                throw new InvalidOperationException("Tree not in memory");
            }

            foreach (var value in values)
            {
                if (!_userDataCache.HasRowsWithValue(address, value))
                {
                    return false;
                }
            }

            return true;
        }


        public bool HasRowsWithValue(TreeAddress address, IRowValue value)
        {
            if (!_userDataCache.HasTree(address))
            {
                throw new InvalidOperationException("Tree not in memory");
            }

            return _userDataCache.HasRowsWithValue(address, value);
        }

        public uint CountOfRowsWithValue(TreeAddress address, IRowValue value)
        {
            if (!_userDataCache.HasTree(address))
            {
                throw new InvalidOperationException("Tree not in memory");
            }

            return _userDataCache.CountOfRowsWithValue(address, value);
        }

        public List<Row> GetRowsWithValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            var result = new List<Row>();

            if (!_userDataCache.HasTree(address))
            {
                return result;
            }

            var locations = _userDataCache.GetRowAddressesWithValue(address, value);

            foreach (var location in locations)
            {
                result.Add(_userDataCache.GetRow(location.RowId, address));
            }

            return result;
        }

        public string GetDatabaseName(Guid dbId)
        {
            if (!_userSystemCache.HasDatabase(dbId))
            {
                // need a corresponding HasDatabaseName(dbId) function
                throw new NotImplementedException();
            }

            return _userSystemCache.GetDatabaseName(dbId);
        }

        public uint GetMaxRowIdForTree(TreeAddress address)
        {
            uint result = 0;

            var pages = _userDataCache.GetContainerPages(address);
            foreach (var pageId in pages)
            {
                var page = _userDataCache.GetPage(new PageAddress(address.DatabaseId, address.TableId, pageId, address.SchemaId));
                var rows = page.GetRowIdsOnPage();

                foreach (var row in rows)
                {
                    if (row.RowId > result)
                    {
                        result = row.RowId;
                    }
                }
            }

            return result;
        }

        public IRow GetRow(RowAddress address, TreeAddress treeAddress)
        {
            var pageAddress = new PageAddress(treeAddress.DatabaseId, treeAddress.TableId, address.PageId, treeAddress.SchemaId);
            var page = _userDataCache.GetPage(pageAddress);
            if (page is not null)
            {
                return page.GetRow(address);
            }
            return null;
        }

        /// <summary>
        /// Gets a row from cache for the specified values
        /// </summary>
        /// <param name="rowId">The row to get from cache</param>
        /// <param name="address">The tree address to get for</param>
        /// <returns>The row for the specified values, if found, otherwise NULL.</returns>
        public IRow GetRow(uint rowId, TreeAddress address)
        {
            return _userDataCache.GetRow(rowId, address);
        }

        public RowAddress GetRowAddress(TreeAddress treeAddress, uint rowId)
        {
            return _userDataCache.GetRowAddress(treeAddress, rowId);
        }

        public List<RowAddress> GetRows(TreeAddress address)
        {
            var result = new List<RowAddress>();

            var pages = _userDataCache.GetContainerPages(address);

            foreach (var pageId in pages)
            {
                var page = _userDataCache.GetPage(new PageAddress(address.DatabaseId, address.TableId, pageId, address.SchemaId));
                result.AddRange(page.GetRowIdsOnPage());
            }

            return result;
        }

        public TreeStatus GetTreeMemoryStatus(TreeAddress address)
        {
            return _userDataCache.GetTreeMemoryStatus(address);
        }

        public TreeStatus GetTreeSizeStatus(TreeAddress address, uint sizeOfDataToAdd)
        {
            return _userDataCache.GetTreeSizeStatus(address, sizeOfDataToAdd);
        }

        public ResultsetValue GetValueAtAddress(in ValueAddress address, ColumnSchema column)
        {
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            var page = _userDataCache.GetPage(new PageAddress(address.DatabaseId, address.TableId, address.PageId, address.SchemaId));
            if (page is not null)
            {
                RowValue value = page.GetValueAtAddress(address, column);

                if (_log is not null)
                {
                    sw.Stop();
                    _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
                }

                return new ResultsetValue { Value = value.GetValueInBinary(false, true), IsNullValue = value.IsNull() };
            }

            return new ResultsetValue();
        }

        /// <summary>
        /// Returns all values at the specified address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="columnName"></param>
        /// <param name="schema"></param>
        /// <returns>Values for the specifed column</returns>
        /// <remarks>This function excludes rows that have been deleted</remarks>
        public List<ValueAddress> GetValues(TreeAddress address, string columnName, ITableSchema schema)
        {
            return _userDataCache.GetValueAddresses(address, schema, columnName);
        }

        public List<ValueAddress> GetValuesForColumnByRows(TreeAddress address, string columnName, ITableSchema schema, List<RowAddress> rows)
        {
            return _userDataCache.GetValueAddressByRows(address, schema, columnName, rows);
        }

        public bool HasUserDataAddress(TreeAddress address)
        {
            return _userDataCache.HasTree(address);
        }

        public bool HasUserDataPage(PageAddress address)
        {
            var treeAddress = new TreeAddress(address.DatabaseId, address.TableId, address.SchemaId);
            var pages = _userDataCache.GetContainerPages(treeAddress);
            foreach (var page in pages)
            {
                if (page == address.PageId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            if (!_userDataCache.HasTree(address))
            {
                return false;
            }

            return _userDataCache.HasValueQuick(address, value);
        }

        public CacheAddRowResult TryAddRow(IRow row, TreeAddress address, ITableSchema schema, out int pageId)
        {
            return _userDataCache.TryAddRow(row, address, out pageId);
        }

        /// <summary>
        /// Updates the specified row in cache
        /// </summary>
        /// <param name="row">The row with updated values</param>
        /// <param name="address">The address of the table</param>
        /// <param name="schema">The schema of the table</param>
        /// <remarks>This does not save the changes to storage, and to ensure persistence to disk you should call to storage with the 
        /// updated information</remarks>
        public void UpdateRow(IRow row, TreeAddress address, ITableSchema schema, out uint pageId)
        {
            CacheUpdateRowResult cacheResult = CacheUpdateRowResult.Unknown;
            IBaseDataPage page = null;

            // TODO: Need to switch logic here if the row is a remote row vs local row
            // TODO: Need to save the pending row to the db xact log

            do
            {
                // TODO: We should lock the container when this is happening to prevent modifications

                cacheResult = _userDataCache.TryUpdateRow(row, address, out pageId);

                switch (cacheResult)
                {
                    case CacheUpdateRowResult.TreeNotInMemory:
                        throw new InvalidOperationException("this function is no longer handled in cache");
                        //HandleUserDataTreeNotInMemory(address, schema);
                        break;
                    case CacheUpdateRowResult.NoPagesOnTree:
                        throw new InvalidOperationException("this function is no longer handled in cache");
                        //HandleNoPagesOnTree(address, schema);
                        break;
                    case CacheUpdateRowResult.NoRoomOnTree:
                        HandleNoRoomOnTree(address, schema);
                        break;
                    case CacheUpdateRowResult.Success:
                        break;
                    default:
                        throw new InvalidOperationException("Unknown Update Result");
                        break;

                }
            }
            while (cacheResult != CacheUpdateRowResult.Success);

            if (cacheResult == CacheUpdateRowResult.Success && pageId != 0)
            {
                // TODO: need to save the modified page with the added row back to disk
                var pageAddress = new PageAddress(address.DatabaseId, address.TableId, pageId, address.SchemaId);
                var pageToSave = _userDataCache.GetPage(pageAddress);
                pageId = pageToSave.PageId();

                // we should not be doing this, should change caller to account for this

                //_storage.SavePageDataToDisk(pageAddress, pageToSave.Data, pageToSave.Type);

                // TODO: If the row was updated, should we also save the updated (forwarded) page back to disk as well?
                // TODO: need to update the db xact log that the xact is committed
                return;
            }

            pageId = 0;
        }

        public void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address)
        {
            _userDataCache.AddInitialData(page, address);
        }

        public void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly)
        {
            _userDataCache.AddInitialData(page, address, friendly);
        }

        public void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address)
        {
            _userDataCache.AddPageToContainer(page, address);
        }

        public void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly)
        {
            _userDataCache.AddPageToContainer(page, address);
        }

        public uint[] UserDataGetContainerPages(TreeAddress address)
        {
            return _userDataCache.GetContainerPages(address);
        }

        public IBaseDataPage UserDataGetPage(PageAddress address)
        {
            return _userDataCache.GetPage(address);
        }
        public bool UserSystemCacheHasDatabase(Guid dbId)
        {
            return _userSystemCache.HasDatabase(dbId);
        }

        public uint CountOfRowsWithAllValues(TreeAddress address, ref IRowValue[] values)
        {
            return _userDataCache.CountOfRowsWithAllValues(address, ref values);
        }

        public IRow[] GetRowsWithAllValues(TreeAddress address, ref IRowValue[] values)
        {
            return _userDataCache.GetRowsWithAllValues(address, ref values);
        }

        public IRow[] GetRowsWithValue(TreeAddress address, IRowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Returns the pages needed when creating a new datbase.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        /// <param name="type">The type of data file being created</param>
        /// <param name="dbId">The database identifier.</param>
        /// <param name="version">The version of the database.</param>
        /// <returns>A list of pages for a new database</returns>
        private List<IPage> GetNewDatabasePages(string dbName, DataFileType type, Guid dbId, int version)
        {

            // TODO: need to make sure this is in sync with DbMetaSystemDataPages.cs
            // probably need to add Users and UserObjects

            var pages = new List<IPage>();

            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    SystemPage systemPage = SystemPageFactory.GetSystemPage100(dbName, type, dbId);
                    pages.Add(systemPage);

                    SystemSchema100 generator = SystemSchemaFactory.GetSystemSchema100();

                    PageAddress systemUserTableAddress = new PageAddress(dbId, SystemSchemaConstants100.Tables.UserTable.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUserTablePage = SystemDataPageFactory.GetSystemDataPage100(systemUserTableAddress, generator.GetUserTableSchema(dbId));

                    pages.Add(systemUserTablePage);

                    PageAddress systemUserTableSchemaAddress = new PageAddress(dbId, SystemSchemaConstants100.Tables.UserTableSchema.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUserTableSchemaPage = SystemDataPageFactory.GetSystemDataPage100(systemUserTableSchemaAddress, generator.GetUserTableSchemaSchema(dbId));

                    pages.Add(systemUserTableSchemaPage);

                    PageAddress systemUserObjectsAddress = new PageAddress(dbId, SystemSchemaConstants100.Tables.UserObjects.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUserObjectsPage = SystemDataPageFactory.GetSystemDataPage100(systemUserObjectsAddress, generator.GetUserObjectsSchema(dbId));

                    pages.Add(systemUserObjectsPage);

                    PageAddress systemUsers = new PageAddress(dbId, SystemSchemaConstants100.Tables.Users.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUsersPage = SystemDataPageFactory.GetSystemDataPage100(systemUsers, generator.GetUsersSchema(dbId));

                    pages.Add(systemUsersPage);

                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return pages;
        }

        /// <summary>
        /// Checks the tree size in memory and on disk. If everything on disk is in memory, it will add a new page to the tree in cache.
        /// </summary>
        /// <param name="address">The address of the tree.</param>
        /// <param name="schema">The schema of the table.</param>
        private void HandleNoRoomOnTree(TreeAddress address, ITableSchema schema)
        {
            // this functionality should be in database

            throw new NotImplementedException();

            /*
            var pages = _userDataCache.GetContainerPages(address);
            var page = _storage.GetAnyUserDataPage(pages, address, schema, this);

            if (page is null)
            {

                var totalPagesOnDisk = _storage.GetTotalPages(address);
                var totalPagesInMemory = _userDataCache.GetContainerPages(address).Length;

                // everything on disk is in memory
                if (totalPagesOnDisk == totalPagesInMemory)
                {
                    var nextAddress = new PageAddress
                    {
                        DatabaseId = address.DatabaseId,
                        TableId = address.TableId,
                        PageId = _storage.GetMaxPageId(address) + 1
                    };
                    page = new UserDataPage100(nextAddress, schema, this);
                }
            }

            _userDataCache.AddPageToContainer(page, address);

            */
        }





        #endregion

    }
}
