using Drummersoft.DrummerDB.Core.Databases.Remote;
using Drummersoft.DrummerDB.Core.Databases.Remote.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.DbDebug;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class Table : ITable
    {
        #region Private Fields
        private ICacheManager _cache;
        private LogService _log;
        // this is never set?
        private RemoteDataManager _remoteManager;

        private TableSchema _schema;
        private ProcessUserDatabaseSettings _settings;
        private IStorageManager _storage;
        private ITransactionEntryManager _xEntryManager;
        #endregion

        #region Public Properties
        /// <summary>
        /// The address of the table 
        /// </summary>
        public TreeAddress Address => _schema.Address;

        /// <summary>
        /// The name of the table
        /// </summary>
        public string Name => _schema.Name;
        #endregion

        #region Constructors
        public Table(TableSchema schema, ICacheManager cache, IRemoteDataManager remoteManager, IStorageManager storage, ITransactionEntryManager xEntryManager) :
            this(schema, cache, storage, xEntryManager)
        {
            _remoteManager = remoteManager as RemoteDataManager;
        }

        public Table(TableSchema schema, ICacheManager cache, IRemoteDataManager remoteManager, IStorageManager storage, ITransactionEntryManager xEntryManager, LogService log) :
         this(schema, cache, storage, xEntryManager, log)
        {
            _remoteManager = remoteManager as RemoteDataManager;
        }

        public Table(TableSchema schema, ICacheManager cache, IStorageManager storage, ITransactionEntryManager xEntryManager)
        {
            _cache = cache;
            _schema = schema;
            _storage = storage;
            _xEntryManager = xEntryManager;
            BringTreeOnline();
        }

        public Table(TableSchema schema, ICacheManager cache, IStorageManager storage, ITransactionEntryManager xEntryManager, LogService log)
        {
            _cache = cache;
            _schema = schema;
            _storage = storage;
            _xEntryManager = xEntryManager;
            _log = log;
            BringTreeOnline();
        }

        public Table(TableSchema schema, ICacheManager cache, IStorageManager storage, ProcessUserDatabaseSettings settings, ITransactionEntryManager xEntryManager) :
            this(schema, cache, storage, xEntryManager)
        {
            _settings = settings;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks the tree status in cache and attempts to load pages into memory if needed
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void BringTreeOnline()
        {
            var treeStaus = _cache.GetTreeMemoryStatus(Address);

            if (treeStaus != TreeStatus.Ready)
            {
                switch (treeStaus)
                {
                    case TreeStatus.TreeNotInMemory:
                        HandleDataTreeNotInMemory();
                        break;
                    case TreeStatus.NoPagesOnTree:
                        HandleNoPagesOnTree();
                        break;
                    case TreeStatus.Unknown:
                    default:
                        throw new InvalidOperationException("Unknown tree status");
                }
            }
        }

        public int CountOfRowsWithAllValues(IRowValue[] values)
        {
            return _cache.CountOfRowsWithAllValues(Address, ref values);
        }

        public int CountOfRowsWithValue(RowValue value)
        {
            if (!HasColumn(value.Column.Name))
            {
                throw new ColumnNotFoundException(value.Column.Name, _schema.Name);
            }

            return _cache.CountOfRowsWithValue(Address, value);
        }

        public List<RowAddress> FindRowAddressesWithValue(RowValue value, ValueComparisonOperator comparison, TransactionRequest transaction, TransactionMode transactionMode)
        {
            var result = new List<RowAddress>();

            if (!HasColumn(value.Column.Name))
            {
                throw new ColumnNotFoundException(value.Column.Name, _schema.Name);
            }

            var rows = _cache.GetValues(Address, value.Column.Name, _schema);

            foreach (var row in rows)
            {
                ResultsetValue physicalValue = _cache.GetValueAtAddress(row, value.Column);

                // note: we need to make sure the binary arrays are actually equal
                byte[] actualValue = physicalValue.Value;
                byte[] expectedValue = value.GetValueInBinary(false, true);
                if (ValueComparer.IsMatch(value.Column.DataType, actualValue, expectedValue, comparison))
                {
                    var rowAddress = new RowAddress(row.PageId, row.RowId, row.RowOffset);
                    result.Add(rowAddress);
                }
            }

            return result;
        }

        public List<IRow> FindRowsNotWithAllValues(List<RowValue> values)
        {
            throw new NotImplementedException();
        }

        public List<IRow> FindRowsNotWithValue(RowValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the rows with any values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>This performs effectively an OR operation on all the values</remarks>
        public List<IRow> FindRowsWithAnyValues(List<RowValue> values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the column from this table
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The column with the name specified from this table</returns>
        /// <exception cref="ColumnNotFoundException"></exception>
        /// <remarks>This function is useful as a shortcut when setting <seealso cref="RowValue"/> parameters.</remarks>
        public ColumnSchema GetColumn(string columnName)
        {
            foreach (var column in _schema.Columns)
            {
                if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }

            throw new ColumnNotFoundException(columnName, Name);
        }

        public ColumnSchema GetColumn(int id)
        {
            foreach (var column in _schema.Columns)
            {
                if (column.Id == id)
                {
                    return column;
                }
            }

            throw new ColumnNotFoundException($"Column Id {id.ToString()} not found in table {_schema.Name}");
        }

        public ColumnSchemaStruct GetColumnStruct(int id)
        {
            foreach (var column in _schema.Columns)
            {
                if (column.Id == id)
                {
                    return DbUtil.ConvertColumnSchemaToStruct(column);
                }
            }

            throw new ColumnNotFoundException($"Column Id {id.ToString()} not found in table {_schema.Name}");
        }

        public ColumnSchemaStruct GetColumnStruct(string columnName)
        {
            foreach (var column in _schema.Columns)
            {
                if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return DbUtil.ConvertColumnSchemaToStruct(column);
                }
            }

            return new ColumnSchemaStruct();
        }

        public LogicalStoragePolicy GetLogicalStoragePolicy()
        {
            if (_schema is TableSchema)
            {
                var schema = _schema as TableSchema;
                return schema.StoragePolicy;
            }

            return LogicalStoragePolicy.None;
        }

        /// <summary>
        /// Creates a new row for the table's schema. Note that this does not set any values (they will be NULL). Defaults IsLocal = TRUE, and ParticipantId = NULL.
        /// </summary>
        /// <returns>A row representing the table's schema with no values set</returns>
        public Row GetNewLocalRow()
        {
            var row = new Row(GetNextRowId(), true);
            var values = new RowValue[_schema.Columns.Length];
            int i = 0;

            foreach (var column in _schema.Columns)
            {
                var value = new RowValue();
                value.Column = column;
                values[i] = value;
                i++;
            }

            row.Values = values;

            return row;
        }

        public Row GetNewRemoteRow(Participant participant)
        {
            var row = new Row(GetNextRowId(), false, participant);
            var values = new RowValue[_schema.Columns.Length];
            int i = 0;

            foreach (var column in _schema.Columns)
            {
                var value = new RowValue();
                value.Column = column;
                values[i] = value;
                i++;
            }

            row.Values = values;

            return row;
        }

        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <param name="address">The address of the row</param>
        /// <returns>The row with the specified address if found, otherwise <c>NULL.</c></returns>
        public IRow GetRow(RowAddress address)
        {
            IRow row = _cache.GetRow(address.RowId, Address);

            // or alt
            //IRow row = _cache.GetRow(address, Address);

            if (row.IsLocal)
            {
                return row;
            }
            else
            {
                throw new NotImplementedException("Remote row handling not implemented yet");

                var participantId = row.ParticipantId;
                var remoteAddress = new SQLAddress { DatabaseId = this.Address.DatabaseId, TableId = this.Address.TableId, RowId = row.Id };
                var participant = new Participant { Id = participantId.Value };
                var remoteRow = _remoteManager.GetRowFromParticipant(participant, remoteAddress);

                // not sure if this is correct, or we need to do some sort of conversion for the row that came from cache
                // for example, does remote row identify itself as local? from the perspective of the caller, is is not.
                return remoteRow;
            }
        }

        public List<RowAddress> GetRowAddressesWithValue(RowValue value, TransactionRequest transaction, TransactionMode transactionMode)
        {
            if (!HasColumn(value.Column.Name))
            {
                throw new ColumnNotFoundException(value.Column.Name, _schema.Name);
            }

            return _cache.GetRowAddressesWithValue(Address, value);
        }

        /// <summary>
        /// Returns a list of row address from the cache for every row location
        /// </summary>
        /// <returns>A list of row address from the cache for every row location</returns>
        /// <remarks>This is an expensive operation and should be done sparingly.</remarks>
        public List<RowAddress> GetRows()
        {
            BringTreeOnline();

            return _cache.GetRows(Address);
        }

        public IRow[] GetRowsWithAllValues(IRowValue[] values)
        {
            foreach (var value in values)
            {
                if (!HasColumn(value.Column.Name))
                {
                    throw new ColumnNotFoundException(value.Column.Name, _schema.Name);
                }
            }

            var result = _cache.GetRowsWithAllValues(Address, ref values);

            return result;
        }

        public List<IRow> GetRowsWithValue(RowValue value)
        {
            if (!HasColumn(value.Column.Name))
            {
                throw new ColumnNotFoundException(value.Column.Name, _schema.Name);
            }

            return _cache.GetRowsWithValue(Address, value, _schema);
        }

        public ResultsetValue GetValueAtAddress(ValueAddress address, TransactionRequest transaction)
        {
            if (HasColumn(address.ColumnName))
            {
                ColumnSchema column = GetColumn(address.ColumnName);

                // this will be saved to disk
                // need a corresponding method to mark the transaction as completed
                TransactionEntry transactionEntry = GetTransactionSelectEntry(transaction);

                return _cache.GetValueAtAddress(address, column);
            }

            throw new InvalidOperationException($"Column: {address.ColumnName} is not part of table {Name}");

        }

        /// <summary>
        /// Returns the value addresses for all the rows specified, for the specified column
        /// </summary>
        /// <param name="rows">The rows to search for the specified value</param>
        /// <param name="columnName">The column to get the value for</param>
        /// <param name="transaction"></param>
        /// <param name="transactionMode"></param>
        /// <returns></returns>
        public List<ValueAddress> GetValuesForColumnByRows(List<RowAddress> rows, string columnName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            // default don't log
            if (_settings is null)
            {
                return GetValuesForColumnByRows(columnName, rows);
            }

            return null;
        }

        /// <summary>
        /// Determines if all the passed values are found in the table.
        /// </summary>
        /// <param name="values">The values to search for in the table</param>
        /// <returns>
        ///   <c>true</c> if the table has all values; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>This effectively is an AND operation on the values supplied.</remarks>
        public bool HasAllValues(List<RowValue> values)
        {
            values.ForEach(value =>
            {
                if (!(HasColumn(value.Column.Name)))
                {
                    throw new ColumnNotFoundException(value.Column.Name, _schema.Name);
                }
            });

            bool hasAllValues = false;

            foreach (var value in values)
            {
                if (!_cache.HasValue(Address, value, _schema))
                {
                    hasAllValues = false;
                    break;
                }
                else
                {
                    hasAllValues = true;
                }
            }

            return hasAllValues;
        }

        /// <summary>
        /// Determines whether the table has a column with the specified name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>
        ///   <c>true</c> if the table has a column with the specified name; otherwise, <c>false</c>.
        /// </returns>
        public bool HasColumn(string columnName)
        {
            return _schema.HasColumn(columnName);
        }

        /// <summary>
        /// Checks to see if any row in the table contains the specified value
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>
        ///   <c>true</c> if the specified table has the value; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ColumnNotFoundException">Thrown when the <seealso cref="ColumnSchema"/> in the row value is not part
        /// of the table.</exception>
        public bool HasValue(RowValue value)
        {
            BringTreeOnline();

            if (!HasColumn(value.Column.Name))
            {
                throw new ColumnNotFoundException(value.Column.Name, _schema.Name);
            }

            return _cache.HasValue(Address, value, _schema);
        }

        /// <summary>
        /// Returns the total number of rows for the table
        /// </summary>
        /// <returns>The total number of rows in cache</returns>
        /// <remarks>This is an expensive operation.</remarks>
        public int RowCount()
        {
            BringTreeOnline();
            return _cache.GetRows(Address).Count;
        }

        public TableSchema Schema()
        {
            return _schema;
        }

        public void SetLogicalStoragePolicy(LogicalStoragePolicy policy)
        {
            if (_schema is TableSchema)
            {
                var schema = _schema as TableSchema;
                schema.SetStoragePolicy(policy);
            }
        }

        public bool XactAddRow(IRow row, TransactionRequest request, TransactionMode transactionMode)
        {
            if (row.IsLocal)
            {
                return XactAddLocalRow(row, request, transactionMode);
            }
            else
            {
                return XactAddRemoteRow(row as Row, request, transactionMode);
            }
        }

        /// <summary>
        /// Adds the row to the table
        /// </summary>
        /// <param name="row">The row.</param>
        /// <remarks>This should eventually be deprecated, because we are doing this in a transactional manner.</remarks>
        public bool XactAddRow(IRow row)
        {
            return XactAddRow(row, new TransactionRequest(), TransactionMode.None);
        }

        public bool XactDeleteRow(IRow row, TransactionRequest request, TransactionMode transactionMode)
        {
            BringTreeOnline();

            TransactionEntry xact = null;
            RowAddress rowAddress;

            switch (transactionMode)
            {
                case TransactionMode.None:

                    return _cache.DeleteRow(row.Id, Address);

                case TransactionMode.Try:

                    rowAddress = _cache.GetRowAddress(Address, row.Id);
                    xact = GetTransactionDeleteEntry(request, row, rowAddress);
                    _xEntryManager.AddEntry(xact);
                    _storage.LogOpenTransaction(_schema.DatabaseId, xact);
                    return _cache.DeleteRow(row.Id, Address);

                case TransactionMode.Rollback:

                    xact = _xEntryManager.FindDeleteTransactionForRowId(row.Id);

                    if (xact is not null)
                    {
                        if (XactAddRow(row))
                        {
                            xact.MarkDeleted();
                            _storage.RemoveOpenTransaction(_schema.DatabaseId, xact);
                            _xEntryManager.RemoveEntry(xact);
                        }
                    }

                    return false;
                case TransactionMode.Commit:
                    xact = _xEntryManager.FindDeleteTransactionForRowId(row.Id);

                    if (xact is not null)
                    {
                        var deleteAction = xact.GetActionAsDelete();
                        xact.MarkComplete();
                        IBaseDataPage pageToSave = _cache.UserDataGetPage(deleteAction.Address.ToPageAddress());
                        _storage.SavePageDataToDisk(deleteAction.Address.ToPageAddress(), pageToSave.Data, pageToSave.Type, pageToSave.DataPageType(), pageToSave.IsDeleted());
                        _storage.LogCloseTransaction(_schema.DatabaseId, xact);
                        _xEntryManager.RemoveEntry(xact);

                        return true;
                    }

                    return false;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }

            return false;
        }

        /// <summary>
        /// Returns the <see cref="ValueAddress"/>es for the specifed column
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <param name="transaction">The <see cref="TransactionRequest"/> to log</param>
        /// <param name="transactionMode">The <see cref="TransactionMode"/></param>
        /// <returns>A list of <see cref="ValueAddress"/>es for the specified column</returns>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="TransactionMode"/> is unknown</exception>
        /// <remarks>This function excludes rows that have been deleted</remarks>
        public List<ValueAddress> XactGetAllValuesForColumn(string columnName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            // default don't log
            if (_settings is null)
            {
                return GetAllValuesForColumn(columnName);
            }
            else
            {
                if (_settings.LogSelectStatementsForHost)
                {
                    // this assumes the transaction batch id == transaction id
                    // if not this entire implementation is wrong
                    TransactionEntry xact = GetTransactionSelectEntry(transaction);


                    // we need to log the SELECT action in the TRY phase, and then in the COMMIT phase, mark in the transaction log the read as committed
                    // we take no action on the data file since we're just reading data, not modifying it
                    switch (transactionMode)
                    {
                        case TransactionMode.Try:
                            _storage.LogOpenTransaction(_schema.DatabaseId, xact);
                            return GetAllValuesForColumn(columnName);
                        case TransactionMode.Commit:
                            xact.MarkComplete();
                            _storage.LogCloseTransaction(_schema.DatabaseId, xact);
                            return GetAllValuesForColumn(columnName);
                        case TransactionMode.Rollback:
                            xact.MarkDeleted();
                            _storage.RemoveOpenTransaction(_schema.DatabaseId, xact);
                            return GetAllValuesForColumn(columnName);
                        case TransactionMode.Unknown:
                            throw new InvalidOperationException("Unknown transaction mode");
                        default:
                            throw new InvalidOperationException("Unknown transaction mode");
                    }
                }
                else
                {
                    return GetAllValuesForColumn(columnName);
                }
            }
        }
        public void XactSetLogicalStoragePolicy(LogicalStoragePolicy policy, TransactionRequest transaction, TransactionMode transactionMode)
        {
            if (_schema is TableSchema)
            {
                var schema = _schema as TableSchema;
                schema.SetStoragePolicy(policy);
            }
        }

        public bool XactUpdateRow(IRow row)
        {
            return XactUpdateRow(row, new TransactionRequest(), TransactionMode.None);
        }

        public bool XactUpdateRow(IRow row, TransactionRequest request, TransactionMode transactionMode)
        {
            BringTreeOnline();

            TransactionEntry xact = null;
            int pageId = 0;
            IRow beforeRow = null;

            if (TreeHasRoom(row.Size()))
            {
                switch (transactionMode)
                {
                    case TransactionMode.None:
                        _cache.UpdateRow(row, Address, _schema, out pageId);

                        return true;
                        break;
                    case TransactionMode.Try:
                        beforeRow = _cache.GetRow(row.Id, Address);
                        _cache.UpdateRow(row, Address, _schema, out pageId);

                        xact = GetTransactionUpdateEntry(request, beforeRow, row, pageId);
                        _xEntryManager.AddEntry(xact);
                        _storage.LogOpenTransaction(_schema.DatabaseId, xact);

                        return true;

                    case TransactionMode.Rollback:
                        xact = _xEntryManager.FindUpdateTransactionForRowId(row.Id);

                        if (xact is not null)
                        {
                            var updateAction = xact.GetActionAsUpdate();

                            _cache.UpdateRow(updateAction.Before, Address, _schema, out pageId);
                            xact.MarkDeleted();
                            _storage.RemoveOpenTransaction(_schema.DatabaseId, xact);
                            _xEntryManager.RemoveEntry(xact);
                            return true;
                        }

                        return false;

                    case TransactionMode.Commit:
                        xact = _xEntryManager.FindUpdateTransactionForRowId(row.Id);

                        if (xact is not null)
                        {
                            var updateAction = xact.GetActionAsUpdate();
                            xact.MarkComplete();
                            IBaseDataPage pageToSave = _cache.UserDataGetPage(updateAction.Address.ToPageAddress());
                            _storage.SavePageDataToDisk(updateAction.Address.ToPageAddress(), pageToSave.Data, pageToSave.Type,
                                pageToSave.DataPageType(), pageToSave.IsDeleted());
                            _storage.LogCloseTransaction(_schema.DatabaseId, xact);
                            _xEntryManager.RemoveEntry(xact);

                            return true;
                        }

                        return false;
                    default:
                        throw new InvalidOperationException("Unknown transaction mode");
                }
            }

            return false;
        }
        #endregion

        #region Private Methods
        private List<ValueAddress> GetAllValuesForColumn(string columnName)
        {

            BringTreeOnline();
            List<ValueAddress> items = null;
            if (_log is not null)
            {
                var sw = Stopwatch.StartNew();
                items = _cache.GetValues(Address, columnName, _schema);
                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
            }
            else
            {
                items = _cache.GetValues(Address, columnName, _schema);
            }

            return items;
        }

        private int GetColumnId(string columnName)
        {
            foreach (var column in _schema.Columns)
            {
                if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return column.Id;
                }
            }

            return 0;
        }

        private int GetNextRowId()
        {
            BringTreeOnline();
            int maxid = _cache.GetMaxRowIdForTree(Address);
            maxid++;
            return maxid;
        }

        private TransactionEntry GetTransactionDeleteEntry(TransactionRequest transaction, IRow rowToBeDeleted, RowAddress rowAddress)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(transaction.TransactionBatchId);
            var tranDataAction = new DeleteTransaction(_schema.DatabaseId, _schema.Id, rowToBeDeleted.Id, rowAddress.PageId, rowAddress.RowOffset, rowToBeDeleted, _schema.Schema.SchemaGUID);
            var entry = new TransactionEntry(transaction.TransactionBatchId, _schema.ObjectId, TransactionActionType.Data, Constants.DatabaseVersions.V100, tranDataAction, transaction.UserName, false, sequenceId);

            return entry;
        }

        /// <summary>
        /// Creates a new <see cref="TransactionEntry"/> with an action of INSERT for this database, table, and row
        /// </summary>
        /// <param name="transactionBatchId">The transaction batch id.</param>
        /// <param name="row">The row being added</param>
        /// <returns>A transaction entry with an action of INSERT for this database, table, and row</returns>
        /// <remarks>This needs to be expanded to include the user issuing the transaction and the SQL plan.</remarks>
        private TransactionEntry GetTransactionInsertEntry(TransactionRequest transaction, IRow row, int pageId)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(transaction.TransactionBatchId);
            var tranDataAction = new InsertTransaction(_schema.DatabaseId, _schema.Id, row.Id, pageId, _schema.Schema.SchemaGUID, row.GetRowInTransactionBinaryFormat());
            var entry = new TransactionEntry(transaction.TransactionBatchId, _schema.ObjectId, TransactionActionType.Data, Constants.DatabaseVersions.V100, tranDataAction, transaction.UserName, false, sequenceId);

            return entry;
        }

        private TransactionEntry GetTransactionSelectEntry(TransactionRequest transaction)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(transaction.TransactionBatchId);

            var tranDataAction = new SelectTableTransaction(_schema.DatabaseId, _schema.Id);
            var entry = new TransactionEntry(transaction.TransactionBatchId, _schema.ObjectId, TransactionActionType.Data, Constants.DatabaseVersions.V100, tranDataAction, transaction.UserName, false, sequenceId);

            return entry;
        }

        private TransactionEntry GetTransactionUpdateEntry(TransactionRequest transaction, IRow before, IRow after, int pageId)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(transaction.TransactionBatchId);
            var tranDataAction = new UpdateTransaction(_schema.DatabaseId, _schema.Id, before.Id, pageId, before, after, _schema.Schema.SchemaGUID);
            var entry = new TransactionEntry(transaction.TransactionBatchId, _schema.ObjectId, TransactionActionType.Data, Constants.DatabaseVersions.V100, tranDataAction, transaction.UserName, false, sequenceId);

            return entry;
        }

        private List<ValueAddress> GetValuesForColumnByRows(string columnName, List<RowAddress> rows)
        {
            return _cache.GetValuesForColumnByRows(Address, columnName, _schema, rows);
        }

        /// <summary>
        /// Attempts to load the specified tree into cache from disk
        /// </summary>
        private void HandleDataTreeNotInMemory()
        {
            bool isUserDatabase = _storage.IsUserDatabase(Address.DatabaseId);

            if (isUserDatabase)
            {
                UserDataPage page = _storage.GetAnyUserDataPage(new int[0], Address, _schema, _schema.Id);

                if (page is null)
                {
                    var totalPagesOnDisk = _storage.GetTotalPages(Address);

                    // is a brand new database or table
                    if (totalPagesOnDisk == 0)
                    {
                        var nextAddress = new PageAddress
                        (
                            Address.DatabaseId,
                            Address.TableId,
                            _storage.GetMaxPageId(Address) + 1,
                            Schema().Schema.SchemaGUID
                        );
                        page = new UserDataPage100(nextAddress, _schema);
                    }
                    else
                    {
                        // need to get pages on disk loaded into memory
                        var pages = _storage.GetAllUserDataPages(Address, _schema);
                        foreach (var diskPage in pages)
                        {
                            var cachePages = _cache.UserDataGetContainerPages(Address);

                            if (cachePages.Length == 0)
                            {
                                _cache.UserDataAddIntitalData(diskPage, Address, new TreeAddressFriendly(_schema.DatabaseName, _schema.Name, _schema.Schema.SchemaName, Address));
                            }
                            else
                            {
                                if (!_cache.HasUserDataPage(diskPage.Address))
                                {
                                    _cache.UserDataAddPageToContainer(diskPage, Address);
                                }
                            }
                        }

                        return;
                    }
                }

                _cache.UserDataAddIntitalData(page, Address, new TreeAddressFriendly(_schema.DatabaseName, _schema.Name, _schema.Schema.SchemaName, Address));
            }
            else
            {
                UserDataPage page = _storage.GetAnySystemDataPage(new int[0], Address, _schema, _schema.Id);

                if (page is null)
                {
                    var totalPagesOnDisk = _storage.GetTotalPages(Address);

                    // TotalPagesOnDisk may be > 0 (usually 5 pages) because all the pages are system pages, not user data pages

                    // is a brand new database or table
                    //if (totalPagesOnDisk == 0)
                    //{
                    var nextAddress = new PageAddress
                    (
                        Address.DatabaseId,
                        Address.TableId,
                        _storage.GetMaxPageId(Address) + 1,
                        Schema().Schema.SchemaGUID
                    );
                    page = new UserDataPage100(nextAddress, _schema);
                    //}
                }

                _cache.UserDataAddIntitalData(page, Address, new TreeAddressFriendly(_schema.DatabaseName, _schema.Name, _schema.Schema.SchemaName, Address));
            }
        }

        private void HandleNoPagesOnTree()
        {
            var page = _storage.GetAnyUserDataPage(new int[0], Address, _schema, _schema.Id);

            if (page is null)
            {
                var totalPagesOnDisk = _storage.GetTotalPages(Address);

                // is a brand new database or table
                if (totalPagesOnDisk == 0)
                {
                    var nextAddress = new PageAddress
                   (
                        Address.DatabaseId,
                        Address.TableId,
                        _storage.GetMaxPageId(Address) + 1,
                        Schema().Schema.SchemaGUID
                    );
                    page = new UserDataPage100(nextAddress, _schema);
                }
            }

            _cache.UserDataAddPageToContainer(page, Address, new TreeAddressFriendly(_schema.DatabaseName, _schema.Name, _schema.Schema.SchemaName, Address));
        }

        private void HandleNoRoomOnTree()
        {
            var pages = _cache.UserDataGetContainerPages(Address);
            var page = _storage.GetAnyUserDataPage(pages, Address, _schema, _schema.Id);

            if (page is null)
            {
                var totalPagesOnDisk = _storage.GetTotalPages(Address);
                var totalPagesInMemory = _cache.UserDataGetContainerPages(Address).Length;

                // everything on disk is in memory
                if (totalPagesOnDisk == totalPagesInMemory)
                {
                    var nextAddress = new PageAddress
                    (
                        Address.DatabaseId,
                        Address.TableId,
                        _storage.GetMaxPageId(Address) + 1,
                        Schema().Schema.SchemaGUID
                    );
                    page = new UserDataPage100(nextAddress, _schema);
                }
            }

            _cache.UserDataAddPageToContainer(page, Address);
        }

        /// <summary>
        /// Checks the tree for this table in cache to see if it has room to add the row size specified
        /// </summary>
        /// <param name="sizeOfDataToAdd">The row size to add to the cache</param>
        /// <returns><c>TRUE</c> if there is room available, otherwise <c>FALSE</c></returns>
        private bool TreeHasRoom(int sizeOfDataToAdd)
        {
            var sizeStatus = _cache.GetTreeSizeStatus(Address, sizeOfDataToAdd);
            if (sizeStatus == TreeStatus.Ready)
            {
                return true;
            }

            return false;
        }

        private bool XactAddLocalRow(IRow row, TransactionRequest request, TransactionMode transactionMode)
        {
            int pageId = 0;
            CacheAddRowResult addResult;
            TransactionEntry xact;

            switch (transactionMode)
            {
                case TransactionMode.None:
                    do
                    {
                        addResult = _cache.TryAddRow(row, Address, _schema, out pageId);

                        var debugRow = row as Row;
                        string debugData = BitConverter.ToString(debugRow.GetRowInPageBinaryFormat());

                        switch (addResult)
                        {
                            case CacheAddRowResult.NoPagesOnTree:
                                HandleNoPagesOnTree();
                                break;
                            case CacheAddRowResult.NoRoomOnTree:
                                HandleNoRoomOnTree();
                                break;
                            case CacheAddRowResult.TreeNotInMemory:
                                HandleDataTreeNotInMemory();
                                break;
                            case CacheAddRowResult.Success:
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown response from cache when adding {row.Id}");
                        }
                    }
                    while (addResult != CacheAddRowResult.Success);

                    if (addResult == CacheAddRowResult.Success && pageId != 0)
                    {
                        var pageAddress = new PageAddress(Address.DatabaseId, Address.TableId, pageId, Schema().Schema.SchemaGUID);
                        IBaseDataPage pageToSave = _cache.UserDataGetPage(pageAddress);

                        string pageData = BitConverter.ToString(pageToSave.Data);

                        _storage.SavePageDataToDisk(pageAddress, pageToSave.Data, pageToSave.Type, pageToSave.DataPageType(), pageToSave.IsDeleted());

                        return true;
                    }

                    return false;
                case TransactionMode.Try:
                    do
                    {
                        addResult = _cache.TryAddRow(row, Address, _schema, out pageId);

                        switch (addResult)
                        {
                            case CacheAddRowResult.NoPagesOnTree:
                                HandleNoPagesOnTree();
                                break;
                            case CacheAddRowResult.NoRoomOnTree:
                                HandleNoRoomOnTree();
                                break;
                            case CacheAddRowResult.TreeNotInMemory:
                                HandleDataTreeNotInMemory();
                                break;
                            case CacheAddRowResult.Success:
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown response from cache when adding {row.Id}");
                        }
                    }
                    while (addResult != CacheAddRowResult.Success);

                    if (addResult == CacheAddRowResult.Success && pageId != 0)
                    {
                        xact = GetTransactionInsertEntry(request, row, pageId);
                        _xEntryManager.AddEntry(xact);
                        _storage.LogOpenTransaction(_schema.DatabaseId, xact);
                        return true;
                    }
                    return false;
                case TransactionMode.Rollback:
                    xact = _xEntryManager.FindInsertTransactionForRowId(row.Id, Address.DatabaseId, Address.TableId);
                    if (xact is not null)
                    {
                        var insertAction = xact.GetActionAsInsert();

                        var page = _cache.UserDataGetPage(insertAction.Address.ToPageAddress());
                        page.DeleteRow(row.Id);

                        _xEntryManager.RemoveEntry(xact);

                        xact.MarkDeleted();
                        _storage.RemoveOpenTransaction(_schema.DatabaseId, xact);
                        return true;
                    }

                    return false;
                case TransactionMode.Commit:
                    xact = _xEntryManager.FindInsertTransactionForRowId(row.Id, Address.DatabaseId, Address.TableId);
                    if (xact is not null)
                    {
                        var insertAction = xact.GetActionAsInsert();
                        xact.MarkComplete();
                        IBaseDataPage pageToSave = _cache.UserDataGetPage(insertAction.Address.ToPageAddress());

                        var debug = new PageDebug(pageToSave.Data);
                        string dataString = debug.DebugData();

                        _storage.SavePageDataToDisk(insertAction.Address.ToPageAddress(),
                            pageToSave.Data, pageToSave.Type, pageToSave.DataPageType(),
                            pageToSave.IsDeleted()
                            );
                        _storage.LogCloseTransaction(_schema.DatabaseId, xact);
                        _xEntryManager.RemoveEntry(xact);

                        return true;
                    }

                    return false;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }
        }

        private bool XactAddRemoteRow(Row row, TransactionRequest request, TransactionMode transactionMode)
        {
            // copy paste of local row actions

            int pageId = 0;
            CacheAddRowResult addResult;
            TransactionEntry xact;
            bool remoteSaveIsSuccessful = false;
            string errorMessage = string.Empty;

            switch (transactionMode)
            {
                case TransactionMode.None:

                    // we need to has the row data and the participant id and save this to cache
                    // we then need to save the data at the participant.
                    
                    remoteSaveIsSuccessful = _remoteManager.SaveRowAtParticipant(
                        row,
                        _schema.DatabaseName,
                        _schema.DatabaseId,
                        Name,
                        _schema.ObjectId,
                        out errorMessage
                        );

                    if (remoteSaveIsSuccessful)
                    {
                        // need to add the participant id and data hash to cache 
                        // and then save the page to disk
                        // and also save this action to the transaction log
                    }

                    /*
                        addResult = _cache.TryAddRow(row, Address, _schema, out pageId);

                        var debugRow = row as Row;
                        string debugData = BitConverter.ToString(debugRow.GetRowInPageBinaryFormat());

                        switch (addResult)
                        {
                        case CacheAddRowResult.NoPagesOnTree:
                            HandleNoPagesOnTree();
                            break;
                        case CacheAddRowResult.NoRoomOnTree:
                            HandleNoRoomOnTree();
                            break;
                        case CacheAddRowResult.TreeNotInMemory:
                            HandleDataTreeNotInMemory();
                            break;
                        case CacheAddRowResult.Success:
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown response from cache when adding {row.Id}");
                        }
                        }
                        while (addResult != CacheAddRowResult.Success);

                        if (addResult == CacheAddRowResult.Success && pageId != 0)
                        {
                        var pageAddress = new PageAddress(Address.DatabaseId, Address.TableId, pageId, Schema().Schema.SchemaGUID);
                        IBaseDataPage pageToSave = _cache.UserDataGetPage(pageAddress);

                        string pageData = BitConverter.ToString(pageToSave.Data);

                        _storage.SavePageDataToDisk(pageAddress, pageToSave.Data, pageToSave.Type, pageToSave.DataPageType(), pageToSave.IsDeleted());

                        throw new NotImplementedException();
                        return true;
                        }

                        return false;
                     */
                    throw new NotImplementedException();


                    break;

                case TransactionMode.Try:


                    // we need to has the row data and the participant id and save this to cache
                    // we then need to save the data at the participant.


                    remoteSaveIsSuccessful = _remoteManager.SaveRowAtParticipant(
                        row,
                        _schema.DatabaseName,
                        _schema.DatabaseId,
                        Name,
                        _schema.ObjectId,
                        out errorMessage
                        );

                    if (remoteSaveIsSuccessful)
                    {
                        // need to add the participant id and data hash to cache 
                        // and then save the page to disk
                        // and also save this action to the transaction log
                    }
                    throw new NotImplementedException();

                    // old code is below;
                    do
                    {
                        addResult = _cache.TryAddRow(row, Address, _schema, out pageId);

                        switch (addResult)
                        {
                            case CacheAddRowResult.NoPagesOnTree:
                                HandleNoPagesOnTree();
                                break;
                            case CacheAddRowResult.NoRoomOnTree:
                                HandleNoRoomOnTree();
                                break;
                            case CacheAddRowResult.TreeNotInMemory:
                                HandleDataTreeNotInMemory();
                                break;
                            case CacheAddRowResult.Success:
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown response from cache when adding {row.Id}");
                        }
                    }
                    while (addResult != CacheAddRowResult.Success);

                    if (addResult == CacheAddRowResult.Success && pageId != 0)
                    {
                        xact = GetTransactionInsertEntry(request, row, pageId);
                        _xEntryManager.AddEntry(xact);
                        _storage.LogOpenTransaction(_schema.DatabaseId, xact);

                        throw new NotImplementedException();
                        return true;
                    }
                    return false;
                case TransactionMode.Rollback:
                    xact = _xEntryManager.FindInsertTransactionForRowId(row.Id, Address.DatabaseId, Address.TableId);
                    if (xact is not null)
                    {
                        var insertAction = xact.GetActionAsInsert();

                        var page = _cache.UserDataGetPage(insertAction.Address.ToPageAddress());
                        page.DeleteRow(row.Id);

                        _xEntryManager.RemoveEntry(xact);

                        xact.MarkDeleted();
                        _storage.RemoveOpenTransaction(_schema.DatabaseId, xact);

                        throw new NotImplementedException();
                        return true;
                    }

                    return false;
                case TransactionMode.Commit:
                    xact = _xEntryManager.FindInsertTransactionForRowId(row.Id, Address.DatabaseId, Address.TableId);
                    if (xact is not null)
                    {
                        var insertAction = xact.GetActionAsInsert();
                        xact.MarkComplete();
                        IBaseDataPage pageToSave = _cache.UserDataGetPage(insertAction.Address.ToPageAddress());

                        var debug = new PageDebug(pageToSave.Data);
                        string dataString = debug.DebugData();

                        _storage.SavePageDataToDisk(insertAction.Address.ToPageAddress(),
                            pageToSave.Data, pageToSave.Type, pageToSave.DataPageType(),
                            pageToSave.IsDeleted()
                            );
                        _storage.LogCloseTransaction(_schema.DatabaseId, xact);
                        _xEntryManager.RemoveEntry(xact);

                        throw new NotImplementedException();
                        return true;
                    }

                    throw new NotImplementedException();
                    return false;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }

            throw new NotImplementedException();
        }
        #endregion

    }
}
