using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Databases
{
    /// <summary>
    /// Represents a database hosted by a Process
    /// </summary>
    internal class HostDb : UserDatabase
    {
        #region Private Fields
        // NOTE: Use _cache here only for actions related to User Data, for any meta data actions, use the corresponding MetaData object
        // NOTE: May need to seperate out User Data actions (i.e. things that pertain to tables) into a different sub-object?
        private DatabaseMetadata _metaData;
        private ITransactionEntryManager _xEntryManager;
        private List<Table> _inMemoryTables;

        private ProcessUserDatabaseSettings _settings;
        #endregion

        #region Public Properties
        public override string Name => _metaData.Name;
        public override int Version => _metaData.Version;
        public override Guid Id => _metaData.Id;
        #endregion

        #region Constructors
        // TODO - this entire class needs a bit more thoughtfulness.
        internal HostDb(DatabaseMetadata metadata, ITransactionEntryManager xEntryManager) : base(metadata)
        {
            _metaData = metadata;
            _xEntryManager = xEntryManager;
            _inMemoryTables = new List<Table>();
        }

        internal HostDb(DatabaseMetadata metadata, ProcessUserDatabaseSettings settings, ITransactionEntryManager xEntryManager) : 
            this(metadata, xEntryManager)
        {
            _settings = settings;
        }
        #endregion

        #region Public Methods
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

        public override DatabaseSchemaInfo GetSchemaInformation(string schemaName)
        {
            if (_metaData.HasSchema(schemaName))
            {
                return _metaData.GetSchemaInfo(schemaName);
            }

            return null;
        }

        public override bool TryCreateSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode)
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
                    result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager);
                    _inMemoryTables.Add(result);
                }
            }

            return result;
        }

        public override int GetMaxTableId()
        {
            int maxId = 0;

            foreach (var table in _metaData.Tables)
            {
                if (table.Id > maxId)
                {
                    maxId = table.Id;
                }
            }

            return maxId;
        }

        public override bool HasTable(int tableId)
        {
            foreach (var item in _metaData.Tables)
            {
                if (item.Address.TableId == tableId)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool HasTable(string tableName, string schemaName)
        {
            foreach (var table in _metaData.Tables)
            {
                if (table.Schema is not null)
                {
                    if (string.Equals(tableName, table.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(schemaName, table.Schema.SchemaName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override Table GetTable(string tableName)
        {
            Table result = null;

            if (HasTable(tableName))
            {
                if (_inMemoryTables.Any(table => string.Equals(table.Schema().Name, tableName, StringComparison.OrdinalIgnoreCase)))
                {
                    return _inMemoryTables.Where(table => string.Equals(table.Schema().Name, tableName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }
                else
                {
                    var item = GetTableSchema(tableName);
                    result = new Table(item, _metaData.CacheManager, _metaData.RemoteDataManager, _metaData.StorageManager, _xEntryManager);
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

        public override bool AddTable(ITableSchema schema, out Guid tableObjectId)
        {
            return _metaData.AddTable(schema, out tableObjectId);
        }

        public override bool TryAddTable(ITableSchema schema, TransactionRequest transaction, TransactionMode transactionMode, out Guid tableObjectId)
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

                        //storage.SavePageDataToDisk(null, null, PageType.Data);
                        return true;
                    }

                    return false;

                case TransactionMode.Rollback:

                    if (HasTable(schema.Name))
                    {
                        xact = _xEntryManager.GetBatch(transaction.TransactionBatchId).First();
                        xact.MarkDeleted();
                        _metaData.DropTable(schema.Name);
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
            foreach (var item in _metaData.Tables)
            {
                if (string.Equals(tableName, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
        private ITableSchema GetTableSchema(string tableName)
        {
            foreach (var item in _metaData.Tables)
            {
                if (string.Equals(tableName, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(item.DatabaseName))
                    {
                        item.DatabaseName = _metaData.Name;
                    }

                    return item;
                }
            }

            return null;
        }

        private ITableSchema GetTableSchema(int tableId)
        {
            foreach (var item in _metaData.Tables)
            {
                if (item.Address.TableId == tableId)
                {
                    return item;
                }
            }

            return null;
        }

        private CreateTableTransaction GetCreateTableTransaction(TableSchema schema)
        {
            return new CreateTableTransaction(schema);
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
        #endregion

    }
}
