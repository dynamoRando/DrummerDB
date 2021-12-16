using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class BaseUserDatabase : UserDatabase
    {
        #region Private Fields
        private DatabaseMetadata _metaData;
        private ITransactionEntryManager _xEntryManager;
        private TableCollection _inMemoryTables;
        private LogService _log;
        #endregion

        #region Public Properties
        public override string Name => _metaData.Name;
        public override int Version => _metaData.Version;
        public override Guid Id => _metaData.Id;
        public override DatabaseType DatabaseType => DatabaseType.Unknown;
        public TableCollection InMemoryTables => _inMemoryTables;
        public DatabaseMetadata MetaData => _metaData;
        #endregion

        #region Constructors
        public BaseUserDatabase(DatabaseMetadata metadata) : base(metadata)
        {
        }

        internal BaseUserDatabase(DatabaseMetadata metadata, ITransactionEntryManager xEntryManager) : base(metadata)
        {
            _metaData = metadata;
            _xEntryManager = xEntryManager;
            _inMemoryTables = new TableCollection();

            LoadTablesIntoMemory();
        }

        internal BaseUserDatabase(DatabaseMetadata metadata, ITransactionEntryManager xEntryManager, LogService log) : base(metadata)
        {
            _metaData = metadata;
            _xEntryManager = xEntryManager;
            _inMemoryTables = new TableCollection();
            _log = log;

            LoadTablesIntoMemory();
        }
        #endregion

        #region Public Methods
        public bool XactLogNotifyHostAcceptedContract(TransactionRequest transaction, TransactionMode transactionMode, Contract contract)
        {

            var storage = _metaData.StorageManager;
            TransactionEntry xact = null;

            switch (transactionMode)
            {
                case TransactionMode.None:

                    xact = GetTransactionEntryForNotifyAcceptContract(GetAcceptContractTransaction(contract), transaction);
                    _xEntryManager.AddEntry(xact);
                    storage.LogOpenTransaction(_metaData.Id, xact);
                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkComplete();
                    storage.LogCloseTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                case TransactionMode.Try:

                    xact = GetTransactionEntryForNotifyAcceptContract(GetAcceptContractTransaction(contract), transaction);
                    _xEntryManager.AddEntry(xact);
                    storage.LogOpenTransaction(_metaData.Id, xact);

                    return true;
                case TransactionMode.Rollback:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkDeleted();
                    storage.RemoveOpenTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                case TransactionMode.Commit:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkComplete();
                    storage.LogCloseTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }

            throw new NotImplementedException();
        }

        public bool XactLogParticipantAcceptsContract(TransactionRequest transaction, TransactionMode transactionMode, Participant participant, Contract contract)
        {
            var storage = _metaData.StorageManager;
            TransactionEntry xact = null;

            switch (transactionMode)
            {
                case TransactionMode.None:
                    
                    xact = GetTransactionEntryForParticipantAcceptsContract(GetParticipantAcceptedContractTransaction(participant, contract), transaction);
                    _xEntryManager.AddEntry(xact);
                    storage.LogOpenTransaction(_metaData.Id, xact);
                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkComplete();
                    storage.LogCloseTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                case TransactionMode.Try:

                    xact = GetTransactionEntryForParticipantAcceptsContract(GetParticipantAcceptedContractTransaction(participant, contract), transaction);
                    _xEntryManager.AddEntry(xact);
                    storage.LogOpenTransaction(_metaData.Id, xact);

                    return true;
                case TransactionMode.Rollback:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkDeleted();
                    storage.RemoveOpenTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                case TransactionMode.Commit:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkComplete();
                    storage.LogCloseTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }
        }

        public bool XactLogParticipantSaveLatestContract(TransactionRequest transaction, TransactionMode transactionMode, Participant participant, Contract contract)
        {
            var storage = _metaData.StorageManager;
            TransactionEntry xact = null;

            switch (transactionMode)
            {
                case TransactionMode.None:

                    xact = GetTransactionEntryForParticipantSaveContract(GetParticipantSaveContractTransaction(participant, contract), transaction);
                    _xEntryManager.AddEntry(xact);
                    storage.LogOpenTransaction(_metaData.Id, xact);
                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkComplete();
                    storage.LogCloseTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                case TransactionMode.Try:

                    xact = GetTransactionEntryForParticipantSaveContract(GetParticipantSaveContractTransaction(participant, contract), transaction);
                    _xEntryManager.AddEntry(xact);
                    storage.LogOpenTransaction(_metaData.Id, xact);

                    return true;
                case TransactionMode.Rollback:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkDeleted();
                    storage.RemoveOpenTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                case TransactionMode.Commit:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkComplete();
                    storage.LogCloseTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }
        }

        public override bool XactDropTable(string tableName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            var storage = _metaData.StorageManager;
            TransactionEntry xact = null;
            var cache = _metaData.CacheManager;
            Table table;
            TableSchema schema;
            List<IPage> pages;

            switch (transactionMode)
            {
                case TransactionMode.None:

                    if (HasTable(tableName))
                    {
                        _inMemoryTables.Remove(tableName);
                        _metaData.DropTable(tableName, transaction, transactionMode);
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TransactionMode.Try:

                    if (HasTable(tableName))
                    {
                        table = _inMemoryTables.Get(tableName);
                        schema = _metaData.GetTableSchema(tableName);
                        var addresses = _metaData.CacheManager.GetPageAddressesForTree(table.Address);
                        pages = new List<IPage>();

                        // get the pages for the tree (table) from memory before we mark them as deleted
                        // and save them to disk
                        foreach (var address in addresses)
                        {
                            var page = _metaData.CacheManager.UserDataGetPage(address);
                            pages.Add(page as IPage);
                        }

                        // record the data in the transaction entry
                        xact = GetTransactionEntryForDropTable(GetDropTableTransaction(schema, table, pages), transaction);
                        _xEntryManager.AddEntry(xact);

                        // save the transaction to disk
                        storage.LogOpenTransaction(_metaData.Id, xact);

                        // -- remove the actual table
                        // remove from our in memory collection
                        _inMemoryTables.Remove(tableName);
                        // remove all the table infastructure (schema, data pages in cache, data pages on disk)
                        _metaData.DropTable(tableName, transaction, transactionMode);

                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TransactionMode.Rollback:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();

                    // need to get the data back to revert the changes
                    // need to load the pages back into cache
                    // and need to make sure the pages are not deleted on disk
                    var xactData = xact.GetActionAsDropTable();

                    table = xactData.Table as Table;
                    schema = xactData.Schema;
                    pages = xactData.Pages;

                    _inMemoryTables.Add(table as Table);
                    _metaData.AddTable(schema, out _);

                    foreach (var page in pages)
                    {
                        if (page is IBaseDataPage)
                        {
                            // note - we might be duplicating pages; because we just mark the pages as deleted
                            // and here we are trying to save the pages back to disk as undeleted
                            // we might be better off just marking the pages as undeleted directly on disk
                            var dataPage = page as IBaseDataPage;
                            storage.SavePageDataToDisk(new PageAddress
                            {
                                PageId = dataPage.PageId(),
                                DatabaseId = table.Address.DatabaseId,
                                TableId = table.Address.TableId,
                                SchemaId = table.Address.SchemaId
                            }, dataPage.Data, dataPage.Type, dataPage.DataPageType(), page.IsDeleted());
                        }
                    }

                    // load Cache back with data from disk
                    table.BringTreeOnline();

                    // once done restoring data, need to clean up transaction
                    xact.MarkDeleted();
                    storage.RemoveOpenTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    return true;

                case TransactionMode.Commit:

                    xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                    xact.MarkComplete();
                    storage.LogCloseTransaction(_metaData.Id, xact);
                    _xEntryManager.RemoveEntry(xact);

                    break;
                case TransactionMode.Unknown:
                    throw new InvalidOperationException("Unknown transaction type");
                default:
                    throw new InvalidOperationException("Unknown transaction type");
            }

            throw new NotImplementedException();
        }

        public bool SetStoragePolicyForTable(string tableName, LogicalStoragePolicy policy)
        {
            if (HasTable(tableName))
            {
                var table = GetTable(tableName);
                table.SetLogicalStoragePolicy(policy);

                var schema = _metaData.GetSchema(tableName, Name) as TableSchema;
                schema.SetStoragePolicy(policy);
                _metaData.UpdateTableSchema(schema);

                return true;
            }

            return false;
        }

        public bool SetStoragePolicyForTable(string tableName, LogicalStoragePolicy policy, TransactionRequest transaction, TransactionMode transactionMode)
        {
            if (HasTable(tableName))
            {
                var table = GetTable(tableName);
                table.XactSetLogicalStoragePolicy(policy, transaction, transactionMode);

                var schema = _metaData.GetSchema(tableName, Name) as TableSchema;
                schema.SetStoragePolicy(policy);
                _metaData.UpdateTableSchema(schema);

                return true;
            }

            return false;
        }

        public override DatabaseSchemaInfo GetSchemaInformation(string schemaName)
        {
            if (_metaData.HasSchema(schemaName))
            {
                return _metaData.GetSchemaInfo(schemaName);
            }

            return null;
        }

        public override bool XactCreateSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode)
        {
            if (!_metaData.HasSchema(schemaName))
            {
                return _metaData.CreateSchema(schemaName, request, transactionMode);
            }

            return false;
        }

        public override bool HasSchema(string schemaName)
        {
            return _metaData.HasSchema(schemaName);
        }

        public override Table GetTable(int tableId)
        {
            Table result = null;

            if (HasTable(tableId))
            {
                if (_inMemoryTables.Any(table => table.Schema().Id == tableId))
                {
                    return _inMemoryTables.Where(table => table.Schema().Id == tableId).FirstOrDefault();
                }
                else
                {
                    var item = GetTableSchema(tableId);
                    if (_log is not null)
                    {
                        result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager, _log);
                    }
                    else
                    {
                        result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager);
                    }

                    _inMemoryTables.Add(result);
                }
            }

            return result;
        }

        public override int GetMaxTableId()
        {
            return _metaData.GetMaxTableId();
        }

        public override bool HasTable(int tableId)
        {
            return _metaData.HasTable(tableId);
        }

        public override bool HasTable(string tableName, string schemaName)
        {
            return _metaData.HasTable(tableName, schemaName);
        }

        public override Table GetTable(string tableName, string schemaName)
        {
            Table result = null;

            if (HasTable(tableName, schemaName))
            {
                if (_inMemoryTables.Contains(tableName, schemaName))
                {
                    return _inMemoryTables.Get(tableName, schemaName);
                }
                else
                {
                    var item = GetTableSchema(tableName);
                    if (_log is not null)
                    {
                        result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager, _log);
                    }
                    else
                    {
                        result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager);
                    }

                    _inMemoryTables.Add(result);
                }

            }

            return result;
        }

        public override Table GetTable(string tableName)
        {
            Table result = null;

            if (HasTable(tableName))
            {
                if (_inMemoryTables.Contains(tableName))
                {
                    return _inMemoryTables.Get(tableName);
                }
                else
                {
                    var item = GetTableSchema(tableName);
                    if (_log is not null)
                    {
                        result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager, _log);
                    }
                    else
                    {
                        result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager);
                    }

                    _inMemoryTables.Add(result);
                }

            }

            return result;
        }

        public override bool HasUser(string userName, Guid userId)
        {
            return _metaData.HasUser(userName, userId);
        }

        public override bool CreateUser(string userName, string pwInput)
        {
            return _metaData.CreateUser(userName, pwInput);
        }

        public override bool HasUser(string userName)
        {
            return _metaData.HasUser(userName);
        }

        public override bool AddTable(TableSchema schema, out Guid tableObjectId)
        {
            var result = _metaData.AddTable(schema, out tableObjectId);
            if (!_inMemoryTables.Contains(schema.Name))
            {
                Table physicalTable = null;

                if (_log is not null)
                {
                    physicalTable = MakeTable(schema, _log);
                }
                else
                {
                    physicalTable = MakeTable(schema);
                }

                _inMemoryTables.Add(physicalTable);
            }

            return result;
        }

        public override bool XactAddTable(TableSchema schema, TransactionRequest transaction, TransactionMode transactionMode, out Guid tableObjectId)
        {
            var storage = _metaData.StorageManager;

            TransactionEntry xact = null;

            tableObjectId = Guid.Empty;

            switch (transactionMode)
            {
                case TransactionMode.None:
                    _metaData.AddTable(schema, out tableObjectId);
                    return true;

                case TransactionMode.Try:

                    if (!HasTable(schema.Name))
                    {
                        xact = GetTransactionEntryForCreateTable(GetCreateTableTransaction(schema as TableSchema), transaction);
                        _xEntryManager.AddEntry(xact);
                        storage.LogOpenTransaction(_metaData.Id, xact);
                        _metaData.AddTable(schema, out tableObjectId);

                        Table newTable = null;

                        if (_log is not null)
                        {
                            newTable = MakeTable(schema, _log);
                        }
                        else
                        {
                            newTable = MakeTable(schema);
                        }

                        if (!_inMemoryTables.Contains(schema.Name))
                        {
                            _inMemoryTables.Add(newTable);
                        }

                        //storage.SavePageDataToDisk(null, null, PageType.Data);
                        return true;
                    }

                    return false;

                case TransactionMode.Rollback:

                    if (HasTable(schema.Name))
                    {
                        xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                        xact.MarkDeleted();
                        _metaData.DropTable(schema.Name, transaction, transactionMode);
                        storage.RemoveOpenTransaction(_metaData.Id, xact);
                        _xEntryManager.RemoveEntry(xact);

                        return true;
                    }

                    return false;
                case TransactionMode.Commit:

                    if (HasTable(schema.Name))
                    {
                        xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                        xact.MarkComplete();
                        storage.LogCloseTransaction(_metaData.Id, xact);
                        _xEntryManager.RemoveEntry(xact);

                        return true;
                    }

                    return false;
                default:
                    throw new InvalidOperationException("Unknown transaction mode");
            }
        }

        public override bool HasTable(string tableName)
        {
            if (_inMemoryTables is null)
            {
                throw new InvalidOperationException();
            }

            if (_inMemoryTables.Contains(tableName))
            {
                return true;
            }
            else
            {
                return _metaData.HasTable(tableName);
            }
        }

        public override bool ValidateUser(string userName, string pwInput)
        {
            return _metaData.ValidateUser(userName, pwInput);
        }

        public override bool AuthorizeUser(string userName, string pwInput, DbPermission permission, Guid objectId)
        {
            return _metaData.AuthorizeUser(userName, pwInput, permission, objectId);
        }

        public override List<TransactionEntry> GetOpenTransactions()
        {
            throw new NotImplementedException();
        }

        public override bool LogFileHasOpenTransaction(TransactionEntryKey key)
        {
            throw new NotImplementedException();
        }

        public override bool GrantUserPermission(string userName, DbPermission permission, Guid objectId)
        {
            return _metaData.GrantUserPermission(userName, permission, objectId);
        }

        public override Guid GetTableObjectId(string tableName)
        {
            return _metaData.GetTableObjectId(tableName);
        }
        #endregion

        #region Private Methods
        private TableSchema GetTableSchema(string tableName)
        {
            return _metaData.GetTableSchema(tableName);
        }

        private Table MakeTable(TableSchema schema)
        {
            return new Table(schema, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager);
        }

        private Table MakeTable(TableSchema schema, LogService log)
        {
            return new Table(schema, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager, log);
        }

        private TableSchema GetTableSchema(int tableId)
        {
            return _metaData.GetTableSchema(tableId);
        }

        private CreateTableTransaction GetCreateTableTransaction(TableSchema schema)
        {
            return new CreateTableTransaction(schema);
        }

        private DropTableTransaction GetDropTableTransaction(TableSchema schema, Table table, List<IPage> pages)
        {
            return new DropTableTransaction(schema, table, pages);
        }

        private ParticipantAcceptedContractTransaction GetParticipantAcceptedContractTransaction(Participant participant, Contract contract)
        {
            return new ParticipantAcceptedContractTransaction(participant, contract);
        }

        private ParticipantSaveContractTransaction GetParticipantSaveContractTransaction(Participant participant, Contract contract)
        {
            return new ParticipantSaveContractTransaction(participant, contract);
        }

        private AcceptContractTransaction GetAcceptContractTransaction(Contract contract)
        {
            return new AcceptContractTransaction(contract);
        }

        private TransactionEntry GetTransactionEntryForCreateTable(CreateTableTransaction transaction, TransactionRequest request)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(request.TransactionBatchId);

            var entry = new TransactionEntry
                (request.TransactionBatchId, _metaData.Id, TransactionActionType.Schema, Constants.DatabaseVersions.V100,
                transaction, request.UserName, false, sequenceId
                );

            return entry;
        }

        private TransactionEntry GetTransactionEntryForDropTable(DropTableTransaction transaction, TransactionRequest request)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(request.TransactionBatchId);
            var entry = new TransactionEntry(request.TransactionBatchId, _metaData.Id, TransactionActionType.Schema, Constants.DatabaseVersions.V100,
                transaction, request.UserName, false, sequenceId);

            return entry;
        }

        private TransactionEntry GetTransactionEntryForParticipantSaveContract(ParticipantSaveContractTransaction transaction, TransactionRequest request)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(request.TransactionBatchId);
            var entry = new TransactionEntry(request.TransactionBatchId, _metaData.Id, TransactionActionType.Schema, Constants.DatabaseVersions.V100,
                transaction, request.UserName, false, sequenceId);

            return entry;
        }

        /// <summary>
        /// Returns an accept contract transaction (i.e. from participant to host)
        /// </summary>
        /// <param name="transaction">The transaciton</param>
        /// <param name="request">The request</param>
        /// <returns>A transaction entry</returns>
        private TransactionEntry GetTransactionEntryForNotifyAcceptContract(AcceptContractTransaction transaction, TransactionRequest request)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(request.TransactionBatchId);
            var entry = new TransactionEntry(request.TransactionBatchId, _metaData.Id, TransactionActionType.Schema, Constants.DatabaseVersions.V100,
                transaction, request.UserName, false, sequenceId);

            return entry;
        }

        /// <summary>
        /// Returns an accepted contract transaciton (i.e the hosth as recived notice of contract acceptance from a participant)
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private TransactionEntry GetTransactionEntryForParticipantAcceptsContract(ParticipantAcceptedContractTransaction transaction, TransactionRequest request)
        {
            var sequenceId = _xEntryManager.GetNextSequenceNumberForBatchId(request.TransactionBatchId);
            var entry = new TransactionEntry(request.TransactionBatchId, _metaData.Id, TransactionActionType.Schema, Constants.DatabaseVersions.V100,
                transaction, request.UserName, false, sequenceId);

            return entry;
        }

        private void LoadTablesIntoMemory()
        {
            foreach (var table in _metaData.Schemas())
            {
                Table newTable = null;
                if (_log is not null)
                {
                    newTable = new Table(table, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager, _log);
                }
                else
                {
                    newTable = new Table(table, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager); ;
                }

                _inMemoryTables.Add(newTable);
            }
        }
        #endregion

    }
}
