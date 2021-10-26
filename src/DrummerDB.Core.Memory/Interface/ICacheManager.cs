using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Memory.Interface
{
    /// <summary>
    /// Responsible for maintaining objects in memory. May have references to <see cref="INetworkManager"/> for network activites (Participants, Db Clients) 
    /// and <see cref="IStorageManager"/> for disk I/O actions, depending on implementation.
    /// </summary>
    internal interface ICacheManager
    {
      
        void AddSystemDbSystemPage(ISystemPage page);

        void AddUserDbSystemPage(ISystemPage page);

        bool DeleteRow(IRow row, TreeAddress address);

        bool DeleteRow(int rowId, TreeAddress address);

        List<RowAddress> FindRowAddressesWithValue(TreeAddress address, RowValue value, ITableSchema schema);

        List<IRow> FindRowsWithAllValues(TreeAddress address, ref RowValueSearch[] values);

        /// <summary>
        /// Returns a list of rows that contain all the values specified in the values param. This is effectively an AND operation.
        /// </summary>
        /// <param name="address">The address of the table</param>
        /// <param name="values">A list of values to search for</param>
        /// <returns>A list of rows matching all the values specified (or an empty list.)</returns>
        List<IRow> FindRowsWithAllValues(TreeAddress address, List<RowValueSearch> values);

        /// <summary>
        /// Finds rows with the specified value and returns them.
        /// </summary>
        /// <param name="address">The address of the tree.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="schema">The schema of the table. Specified in case the Tree needs to be loaded into memroy from disk.</param>
        /// <returns>A list of rows that contain the specified value.</returns>
        List<IRow> FindRowsWithValue(TreeAddress address, RowValue value, ITableSchema schema);

        List<IRow> FindRowsWithValue(TreeAddress address, RowValueStruct value, ITableSchema schema);

        List<IRow> FindRowsWithValue(TreeAddress address, RowValueSearch value);

        /// <summary>
        /// Returns the name of the database for the specified database id. This method will load the system page into cache if it is not already there.
        /// </summary>
        /// <param name="dbId">The db id to look up.</param>
        /// <returns>The name of the database</returns>
        string GetDatabaseName(Guid dbId);

        int GetMaxRowIdForTree(TreeAddress address);

        /// <summary>
        /// Gets a row from cache for the specified values
        /// </summary>
        /// <param name="rowId">The row to get from cache</param>
        /// <param name="address">The tree address to get for</param>
        /// <returns>The row for the specified values if found, otherwise NULL.</returns>
        IRow GetRow(int rowId, TreeAddress address);

        IRow GetRow(RowAddress address, TreeAddress treeAddress);

        RowAddress GetRowAddress(TreeAddress treeAddress, int rowId);

        /// <summary>
        /// Returns a list of row addresses for every single row
        /// </summary>
        /// <param name="address">The address to get all rows for</param>
        /// <returns>A list of row addresses for every single row</returns>
        /// <remarks>This is an expensive operation. Use this sparingly.</remarks>
        List<RowAddress> GetRows(TreeAddress address);

        TreeStatus GetTreeMemoryStatus(TreeAddress address);

        TreeStatus GetTreeSizeStatus(TreeAddress address, int sizeOfDataToAdd);

        ResultsetValue GetValueAtAddress(in ValueAddress address, ColumnSchema column);

        List<ValueAddress> GetValues(TreeAddress address, string columnName, ITableSchema schema);

        List<ValueAddress> GetValuesForColumnByRows(TreeAddress address, string columnName, ITableSchema schema, List<RowAddress> rows);

        bool HasUserDataAddress(TreeAddress address);

        bool HasUserDataPage(PageAddress address);

        /// <summary>
        /// Determines whether the specified tree address has the specified value.
        /// </summary>
        /// <param name="address">The address of the tree.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="schema">The schema of the table. Specified in case the Tree needs to be loaded into memory from disk.</param>
        /// <returns>
        ///   <c>true</c> if the specified address has the value; otherwise, <c>false</c>.
        /// </returns>
        bool HasValue(TreeAddress address, RowValue value, ITableSchema schema);

        bool HasValue(TreeAddress address, RowValueSearch value, ITableSchema schema);

        /// <summary>
        /// Attempts to add the requested to row and returns the result of the operation
        /// </summary>
        /// <param name="row">The row to add</param>
        /// <param name="address">The address of the tree to add the row to</param>
        /// <param name="schema">The schema of the table</param>
        /// <param name="pageId">The page id where the row was added</param>
        /// <returns>A result reporting the status of the attempt to add the row</returns>
        CacheAddRowResult TryAddRow(IRow row, TreeAddress address, ITableSchema schema, out int pageId);

        void UpdateRow(IRow row, TreeAddress address, ITableSchema schema, out int pageId);
        void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address);
        void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly);
        void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address);
        void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendlyName);
        int[] UserDataGetContainerPages(TreeAddress address);
        IBaseDataPage UserDataGetPage(PageAddress address);
        bool UserSystemCacheHasDatabase(Guid dbId);
    }
}
