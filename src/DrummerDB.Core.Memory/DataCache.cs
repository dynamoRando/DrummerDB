using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Drummersoft.DrummerDB.Core.Memory
{
    /// <summary>
    /// A concrete representation of an object for interfacing with Data Pages (System and User). For more information, see Page.md
    /// </summary>
    /// <seealso cref="Drummersoft.DrummerDB.Core.Memory.Interface.IDataCache" />
    internal class DataCache : IMemoryData
    {
        #region Private Fields
        private ConcurrentDictionary<TreeAddress, TreeContainer> _dataCache;
        private List<TreeAddressFriendly> _dataCacheFriendly;
        private LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public DataCache()
        {
            _dataCache = new ConcurrentDictionary<TreeAddress, TreeContainer>();
            _dataCacheFriendly = new List<TreeAddressFriendly>();
        }

        public DataCache(LogService log)
        {
            _dataCache = new ConcurrentDictionary<TreeAddress, TreeContainer>();
            _dataCacheFriendly = new List<TreeAddressFriendly>();
            _log = log;
        }
        #endregion

        #region Public Methods
        public bool TryRemoveTree(TreeAddress address)
        {
            return _dataCache.Remove(address, out _);
        }

        public List<PageAddress> GetPageAddressesForTree(TreeAddress address)
        {
            var addresses = new List<PageAddress>();

            TreeContainer container = GetContainer(address);
            foreach (var pageId in container.Pages())
            {
                var a = new PageAddress { DatabaseId = address.DatabaseId, PageId = pageId, TableId = address.TableId, SchemaId = address.SchemaId };
                addresses.Add(a);
            }

            return addresses;
        }

        public RowAddress GetRowAddress(TreeAddress treeAddress, uint rowId)
        {
            TreeContainer container = GetContainer(treeAddress);
            uint pageId = container.GetPageIdOfRow(rowId);
            var offsets = container.GetRowOffsets(rowId, pageId);
            var rowType = container.GetRowType(rowId, pageId);

            return new RowAddress(pageId, rowId, offsets.Max(), Guid.Empty, rowType);
        }

        public TreeStatus GetTreeSizeStatus(TreeAddress address, uint sizeOfDataToAdd)
        {
            if (IsTreeFull(address, sizeOfDataToAdd))
            {
                return TreeStatus.NoRoomOnTree;
            }

            return TreeStatus.Ready;
        }
        public TreeStatus GetTreeMemoryStatus(TreeAddress address)
        {
            if (!HasAddress(address))
            {
                return TreeStatus.TreeNotInMemory;
            }

            if (IsContainerEmpty(address))
            {
                return TreeStatus.NoPagesOnTree;
            }

            return TreeStatus.Ready;
        }

        public List<ValueAddress> GetValueAddressByRows(TreeAddress address, ITableSchema schema, string columnName, List<RowAddress> rows)
        {
            if (_log is not null)
            {
                var sw = Stopwatch.StartNew();
                var result = ValueAddressByRows(address, schema, columnName, rows);
                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
                return result;
            }
            else
            {
                return ValueAddressByRows(address, schema, columnName, rows);
            }
        }

        public List<ValueAddress> GetValueAddresses(TreeAddress address, ITableSchema schema, string columnName)
        {
            var result = new List<ValueAddress>();
            Stopwatch sw = null;
            if (_log is not null)
            {
                sw = Stopwatch.StartNew();
            }

            if (!HasAddress(address))
            {
                throw new InvalidOperationException();
            }
            else
            {
                var pageIds = GetContainerPages(address);
                var pageAddresses = new List<PageAddress>();

                foreach (var id in pageIds)
                {
                    var pageAddress = new PageAddress(address.DatabaseId, address.TableId, id, address.SchemaId);
                    pageAddresses.Add(pageAddress);
                }

                foreach (var pageAddress in pageAddresses)
                {
                    IBaseDataPage page = GetPage(pageAddress);

                    List<RowAddress> rows = null;

                    // if the table is all fixed length columns, we can avoid one extra call to get the actual row
                    // to calculate the ParseLengthValue of each row since the ParseLengthValue at that point
                    // is just the sizes of each of the fixed length columns before it
                    if (schema.HasAllFixedLengthColumns())
                    {
                        rows = page.GetRowIdsOnPage(false);
                        if (rows.Count != page.TotalRows())
                        {
                            throw new InvalidOperationException("Total rows on page does not match rows found");
                        }

                        // need to calculate value offset
                        schema.SortBinaryOrder();
                        uint columnValueOffset = 0;

                        foreach (var column in schema.Columns)
                        {
                            if (!string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                            {
                                columnValueOffset += column.Length;
                            }
                        }

                        uint valueOffset = (uint)(RowConstants.Preamble.Length() + columnValueOffset);
                        var schemaColumn = schema.Columns.Where(c => string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                        foreach (var row in rows)
                        {
                            var valueAddress =
                                        new ValueAddress
                                        {
                                            PageId = pageAddress.PageId,
                                            RowId = row.RowId,
                                            RowOffset = row.RowOffset,
                                            ValueOffset = valueOffset,
                                            ParseLength = schemaColumn.Length,
                                            DatabaseId = address.DatabaseId,
                                            TableId = address.TableId,
                                            ColumnName = columnName,
                                            ColumnId = GetColumnId(schema, columnName),
                                            SchemaId = address.SchemaId,
                                            RowType = row.RowType
                                        };
                            result.Add(valueAddress);
                        }
                    }
                    else
                    {
                        rows = page.GetRowIdsOnPage(true);
                        foreach (var row in rows)
                        {
                            Row physicalRow = page.GetRow(row);
                            if (physicalRow.IsLogicallyDeleted == false)
                            {
                                if (physicalRow.HasLocalData())
                                {
                                    if (!physicalRow.IsForwarded)
                                    {
                                        var rowValueGroup = physicalRow as RowValueGroup;
                                        rowValueGroup.SortBinaryOrder();

                                        var bytes = physicalRow.GetRowInPageBinaryFormat();
                                        schema.SortBinaryOrder();

                                        uint valueOffset = 0;

                                        if (physicalRow.IsRemotable())
                                        {
                                            valueOffset = RowConstants.Preamble.Length() + physicalRow.RemoteSize;
                                        }
                                        else
                                        {
                                            valueOffset = RowConstants.Preamble.Length();
                                        }


                                        foreach (var value in rowValueGroup.Values)
                                        {
                                            if (string.Equals(value.Column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                                            {
                                                var valueAddress =
                                                    new ValueAddress
                                                    {
                                                        PageId = pageAddress.PageId,
                                                        RowId = row.RowId,
                                                        RowOffset = row.RowOffset,
                                                        ValueOffset = valueOffset,
                                                        ParseLength = value.ParseValueLength,
                                                        DatabaseId = address.DatabaseId,
                                                        TableId = address.TableId,
                                                        ColumnName = columnName,
                                                        ColumnId = GetColumnId(schema, columnName),
                                                        SchemaId = address.SchemaId,
                                                        RowType = row.RowType,
                                                        RemotableId = row.RemotableId
                                                    };
                                                result.Add(valueAddress);
                                            }
                                            else
                                            {
                                                // this should return for fixed length values the fixed length of the data type
                                                // or if a fixed nullable length, the fixed length value + 1 byte (bool) for is null or not
                                                // if the value is variable length, this will NOT include the 4 byte leading prefix, it will just return the value
                                                // or if nullable variable length, the value + bool (or just bool if actually null)

                                                // basically, this offset needs to reflect the layout of the byte array on the page
                                                if (value.Column.IsFixedBinaryLength)
                                                {
                                                    valueOffset += value.BinarySize();
                                                }
                                                else
                                                {
                                                    // need to include the leading 4 byte prefix that tells us the size of the item
                                                    valueOffset += value.BinarySize() + Constants.SIZE_OF_INT;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var remotableRow = physicalRow as IRowRemotable;
                                    var valueAddress =
                                                new ValueAddress
                                                {
                                                    PageId = pageAddress.PageId,
                                                    RowId = row.RowId,
                                                    RowOffset = row.RowOffset,
                                                    ValueOffset = 0,
                                                    ParseLength = 0,
                                                    DatabaseId = address.DatabaseId,
                                                    TableId = address.TableId,
                                                    ColumnName = columnName,
                                                    ColumnId = GetColumnId(schema, columnName),
                                                    SchemaId = address.SchemaId,
                                                    RemotableId = remotableRow.RemoteId,
                                                    RowType = row.RowType

                                                };
                                    result.Add(valueAddress);

                                }
                            }
                        }
                    }


                }
            }

            if (_log is not null)
            {
                sw.Stop();
                _log.Performance(Assembly.GetExecutingAssembly().GetName().Name, LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }

            return result;
        }

        public bool HasTree(TreeAddress address)
        {
            return _dataCache.ContainsKey(address);
        }

        public bool TryDeleteRow(uint rowId, TreeAddress address)
        {
            if (HasAddress(address))
            {
                TreeContainer container = GetContainer(address);
                if (container is not null)
                {
                    container.DeleteRow(rowId);
                    return true;
                }

            }

            return false;
        }

        public CacheUpdateRowResult TryUpdateRow(Row row, TreeAddress address, out uint pageId)
        {
            TreeContainer container = null;
            pageId = 0;

            if (!HasAddress(address))
            {
                return CacheUpdateRowResult.TreeNotInMemory;
            }

            if (IsContainerEmpty(address))
            {
                return CacheUpdateRowResult.NoPagesOnTree;
            }

            container = GetContainer(address);
            pageId = container.UpdateRow(row);

            if (pageId != 0)
            {
                return CacheUpdateRowResult.Success;
            }

            return CacheUpdateRowResult.Unknown;
        }

        public CacheAddRowResult TryAddRow(Row row, TreeAddress address, out uint pageId)
        {
            if (row.IsTempForParticipant())
            {
                throw new InvalidOperationException("Temp rows should not be sent to cache or disk");
            }

            CacheAddRowResult result = CacheAddRowResult.Unknown;
            TreeContainer container = null;
            pageId = 0;

            if (!HasAddress(address))
            {
                return CacheAddRowResult.TreeNotInMemory;
            }

            if (IsContainerEmpty(address))
            {
                return CacheAddRowResult.NoPagesOnTree;
            }

            if (IsTreeFull(address, row.TotalSize))
            {
                return CacheAddRowResult.NoRoomOnTree;
            }
            container = GetContainer(address);
            pageId = container.AddRow(row);

            if (pageId != 0)
            {
                return CacheAddRowResult.Success;
            }

            return result;
        }

        public bool AddInitialData(IBaseDataPage page, TreeAddress address)
        {
            if (_dataCache.ContainsKey(address))
            {
                throw new InvalidOperationException("Tree is already loaded in memory");
            }

            TreeContainer container;
            if (_log is not null)
            {
                container = new TreeContainer(address, page, _log);
            }
            else
            {
                container = new TreeContainer(address, page);
            }

            return _dataCache.TryAdd(address, container);
        }

        public bool AddInitialData(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly)
        {
            if (_dataCache.ContainsKey(address))
            {
                throw new InvalidOperationException("Tree is already loaded in memory");
            }

            TreeContainer container;
            if (_log is not null)
            {
                container = new TreeContainer(address, page, _log);
            }
            else
            {
                container = new TreeContainer(address, page);
            }

            if (_dataCacheFriendly.Contains(friendly))
            {
                throw new InvalidOperationException("Table is already in cache");
            }

            _dataCacheFriendly.Add(friendly);

            return _dataCache.TryAdd(address, container);
        }

        public uint[] GetContainerPages(TreeAddress address)
        {
            TreeContainer container = GetContainer(address);
            if (container is not null)
            {
                return container.Pages();
            }
            else
            {
                return new uint[0];
            }
        }

        public void AddPageToContainer(IBaseDataPage page, TreeAddress address)
        {
            TreeContainer container = GetContainer(address);
            container.AddPage(page);
        }

        public void AddPageToContainer(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly)
        {
            TreeContainer container = GetContainer(address);
            container.AddPage(page);

            _dataCacheFriendly.Add(friendly);
        }

        public byte[] GetPageData(PageAddress address)
        {
            var container = GetContainer(new TreeAddress(address.DatabaseId, address.TableId, address.SchemaId));
            return container.GetPage(address.PageId).Data;
        }

        public IBaseDataPage GetPage(PageAddress address)
        {
            // is this actually thread safe?
            var container = GetContainer(new TreeAddress(address.DatabaseId, address.TableId, address.SchemaId));
            return container.GetPage(address.PageId);
        }

        /// <summary>
        /// Gets the row for the specified tree and id.
        /// </summary>
        /// <param name="rowId">The row identifier.</param>
        /// <param name="address">The address of the tree.</param>
        /// <returns>
        /// The row specified if found, otherwise <c>NULL.</c>
        /// </returns>
        public Row GetRow(uint rowId, TreeAddress address)
        {
            var container = GetContainer(address);
            return container.GetRow(rowId);
        }

        public RowAddress[] GetRowAddressesWithValue(TreeAddress address, IRowValue value)
        {
            List<RowAddress> result = null;
            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                result = new List<RowAddress>();
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    var rows = page.GetRowsWithValue(value);
                    result.AddRange(rows);
                }
            }

            return result.ToArray();
        }

        public List<RowAddress> GetRowAddressesWithValue(TreeAddress address, RowValue value)
        {
            List<RowAddress> result = null;
            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                result = new List<RowAddress>();
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    var rows = page.GetRowsWithValue(value);
                    result.AddRange(rows);
                }
            }

            return result;
        }


        public bool HasValue(TreeAddress address, RowValue value)
        {
            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    var result = page.HasValue(value);
                    if (result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns an array of row ids that have the specified value at the address
        /// </summary>
        /// <param name="address">The address of the tree</param>
        /// <param name="value">The value to look for</param>
        /// <returns>An array of row ids that contain the value</returns>
        public List<RowAddress> HasValue(TreeAddress address, IRowValue value)
        {
            List<RowAddress> result = null;
            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                result = new List<RowAddress>();
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    var rows = page.GetRowsWithValue(value);
                    result.AddRange(rows);
                }
            }

            return result;
        }

        public bool HasRowsWithValue(TreeAddress address, IRowValue value)
        {
            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    if (page.HasValue(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public bool HasValueQuick(TreeAddress address, RowValue value)
        {
            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    if (page.HasValue(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsContainerEmpty(TreeAddress address)
        {
            var container = GetContainer(address);
            return container.IsEmpty();
        }

        public bool IsTreeFull(TreeAddress address, uint rowSize)
        {
            var container = GetContainer(address);
            return container.IsTreeFull(rowSize);
        }
        public bool HasAddress(TreeAddress address)
        {
            return _dataCache.ContainsKey(address);
        }

        public bool DeleteRow(IRow row, TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRow(uint rowId, TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public List<RowAddress> FindRowAddressesWithValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsWithAllValues(TreeAddress address, ref RowValue[] values)
        {
            throw new NotImplementedException();
        }

        public bool HasRowsWithAllValues(TreeAddress address, ref IRowValue[] values)
        {
            throw new NotImplementedException();
        }

        public uint CountOfRowsWithAllValues(TreeAddress address, ref IRowValue[] values)
        {
            var addresses = new List<RowAddress>();

            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                foreach (var value in values)
                {
                    foreach (var id in container.Pages())
                    {
                        var page = container.GetPage(id);
                        if (page.HasValue(value))
                        {
                            addresses.AddRange(page.GetRowsWithValue(value));
                        }
                        else
                        {
                            // we want all values in the array to be found
                            // as soon as one of them are not found, we should bail
                            // in other words, this should behave as an AND operation for all values.
                            // all values must be TRUE
                            return 0;
                        }
                    }
                }
            }

            return (uint)addresses.Distinct().Count();
        }

        public Row[] GetRowsWithAllValues(TreeAddress address, ref IRowValue[] values)
        {
            var result = new List<Row>();

            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    if (page.HasAllValues(values))
                    {
                        var addresses = page.GetRowAddressesWithAllValues(values);
                        foreach (var addy in addresses)
                        {
                            result.Add(page.GetRow(addy));
                        }
                    }
                }
            }

            return result.Distinct().ToArray();
        }

        public List<Row> FindRowsWithAllValues(TreeAddress address, List<RowValue> values)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsWithValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public uint CountOfRowsWithValue(TreeAddress address, IRowValue value)
        {
            uint count = 0;
            TreeContainer container;
            _dataCache.TryGetValue(address, out container);

            if (container is not null)
            {
                foreach (var id in container.Pages())
                {
                    var page = container.GetPage(id);
                    count += page.GetCountOfRowsWithValue(value);
                }
            }

            return count;
        }

        public Row[] GetRowsWithValue(TreeAddress address, IRowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseName(Guid dbId)
        {
            throw new NotImplementedException();
        }

        public Row GetRow(RowAddress address, TreeAddress treeAddress)
        {
            throw new NotImplementedException();
        }

        public List<RowAddress> GetRows(TreeAddress address)
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

        public bool HasValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public CacheAddRowResult TryAddRow(Row row, TreeAddress address, ITableSchema schema, out uint pageId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRow(Row row, TreeAddress address, ITableSchema schema, out uint pageId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods
        private List<ValueAddress> ValueAddressByRows(TreeAddress address, ITableSchema schema, string columnName, List<RowAddress> rows)
        {
            var result = new List<ValueAddress>();

            foreach (var row in rows)
            {
                Row physicalRow = GetRow(row.RowId, address);

                if (!physicalRow.HasLocalData())
                {
                    throw new InvalidOperationException("Remote row handling should be done at database level");
                }

                if (physicalRow.IsLogicallyDeleted == false)
                {
                    physicalRow.AsValueGroup().SortBinaryOrder();
                    var bytes = physicalRow.GetRowInPageBinaryFormat();
                    schema.SortBinaryOrder();

                    uint valueOffset = (uint)RowConstants.Preamble.Length();

                    foreach (var value in physicalRow.AsValueGroup().Values)
                    {
                        if (string.Equals(value.Column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            ValueAddress valueAddress;
                            if (physicalRow.IsRemotable())
                            {
                               valueAddress =
                               new ValueAddress
                               {
                                   PageId = row.PageId,
                                   RowId = row.RowId,
                                   RowOffset = row.RowOffset,
                                   ValueOffset = valueOffset,
                                   ParseLength = value.ParseValueLength,
                                   DatabaseId = address.DatabaseId,
                                   TableId = address.TableId,
                                   ColumnName = columnName,
                                   SchemaId = address.SchemaId,
                                   ColumnId = value.Column.Id,
                                   RowType = physicalRow.Type,
                                   RemotableId = (physicalRow as IRowRemotable).RemoteId
                               };
                            }
                            else
                            {
                               valueAddress =
                               new ValueAddress
                               {
                                   PageId = row.PageId,
                                   RowId = row.RowId,
                                   RowOffset = row.RowOffset,
                                   ValueOffset = valueOffset,
                                   ParseLength = value.ParseValueLength,
                                   DatabaseId = address.DatabaseId,
                                   TableId = address.TableId,
                                   ColumnName = columnName,
                                   SchemaId = address.SchemaId,
                                   ColumnId = value.Column.Id,
                                   RowType = physicalRow.Type
                               };
                            }

                            result.Add(valueAddress);
                        }
                        else
                        {
                            // this should return for fixed length values the fixed length of the data type
                            // or if a fixed nullable length, the fixed length value + 1 byte (bool) for is null or not
                            // if the value is variable length, this will NOT include the 4 byte leading prefix, it will just return the value
                            // or if nullable variable length, the value + bool (or just bool if actually null)

                            // basically, this offset needs to reflect the layout of the byte array on the page
                            if (value.Column.IsFixedBinaryLength)
                            {
                                valueOffset += value.BinarySize();
                            }
                            else
                            {
                                // need to include the leading 4 byte prefix that tells us the size of the item
                                valueOffset += value.BinarySize() + Constants.SIZE_OF_INT;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Tries to get the container from cache. 
        /// </summary>
        /// <param name="address">The address of the container</param>
        /// <returns>The container specified, or NULL the container is not in cache</returns>
        private TreeContainer GetContainer(TreeAddress address)
        {
            TreeContainer container = null;
            _dataCache.TryGetValue(address, out container);
            return container;
        }

        private uint GetColumnId(ITableSchema schema, string columnName)
        {
            foreach (var column in schema.Columns)
            {
                if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return column.Id;
                }
            }

            return 0;
        }

        public List<Row> GetRowsWithValue(TreeAddress address, RowValue value, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
