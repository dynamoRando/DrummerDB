using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Memory.Interface
{
    internal interface IMemoryData
    {
        bool TryRemoveTree(TreeAddress address);
        bool DeleteRow(IRow row, TreeAddress address);

        bool DeleteRow(uint rowId, TreeAddress address);
        List<RowAddress> GetRowAddressesWithValue(TreeAddress address, RowValue value);
        bool HasRowsWithAllValues(TreeAddress address, ref IRowValue[] values);
        int CountOfRowsWithAllValues(TreeAddress address, ref IRowValue[] values);
        IRow[] GetRowsWithAllValues(TreeAddress address, ref IRowValue[] values);

        List<IRow> GetRowsWithValue(TreeAddress address, RowValue value, ITableSchema schema);

        bool HasRowsWithValue(TreeAddress address, IRowValue value);
        int CountOfRowsWithValue(TreeAddress address, IRowValue value);
        IRow[] GetRowsWithValue(TreeAddress address, IRowValue value, ITableSchema schema);

        /// <summary>
        /// Returns the name of the database for the specified database id. This method will load the system page into cache if it is not already there.
        /// </summary>
        /// <param name="dbId">The db id to look up.</param>
        /// <returns>The name of the database</returns>
        string GetDatabaseName(Guid dbId);

        /// <summary>
        /// Gets a row from cache for the specified values
        /// </summary>
        /// <param name="rowId">The row to get from cache</param>
        /// <param name="address">The tree address to get for</param>
        /// <returns>The row for the specified values if found, otherwise NULL.</returns>
        IRow GetRow(uint rowId, TreeAddress address);

        IRow GetRow(RowAddress address, TreeAddress treeAddress);

        RowAddress GetRowAddress(TreeAddress treeAddress, uint rowId);

        /// <summary>
        /// Returns a list of row addresses for every single row
        /// </summary>
        /// <param name="address">The address to get all rows for</param>
        /// <returns>A list of row addresses for every single row</returns>
        /// <remarks>This is an expensive operation. Use this sparingly.</remarks>
        List<RowAddress> GetRows(TreeAddress address);
        ResultsetValue GetValueAtAddress(in ValueAddress address, ColumnSchema column);
        List<ValueAddress> GetValues(TreeAddress address, string columnName, ITableSchema schema);
        List<ValueAddress> GetValuesForColumnByRows(TreeAddress address, string columnName, ITableSchema schema, List<RowAddress> rows);

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

        /// <summary>
        /// Attempts to add the requested to row and returns the result of the operation
        /// </summary>
        /// <param name="row">The row to add</param>
        /// <param name="address">The address of the tree to add the row to</param>
        /// <param name="schema">The schema of the table</param>
        /// <param name="pageId">The page id where the row was added</param>
        /// <returns>A result reporting the status of the attempt to add the row</returns>
        CacheAddRowResult TryAddRow(IRow row, TreeAddress address, ITableSchema schema, out uint pageId);

        void UpdateRow(IRow row, TreeAddress address, ITableSchema schema, out uint pageId);
    }
}
