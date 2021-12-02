﻿using Drummersoft.DrummerDB.Core.Databases.Abstract;
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
    internal class PartialDb : UserDatabase
    {
        #region Private Fields
        private BaseUserDatabase _baseDb;
        #endregion

        #region Public Properties
        public override string Name => _baseDb.Name;
        public override int Version => _baseDb.Version;
        public override Guid Id => _baseDb.Id;
        public override DatabaseType DatabaseType => DatabaseType.Partial;
        #endregion

        #region Constructors
        public PartialDb(DatabaseMetadata metadata) : base(metadata)
        {
        }
        #endregion

        #region Public Methods
        public override bool TryDropTable(string tableName, TransactionRequest transaction, TransactionMode transactionMode)
        {
            return _baseDb.TryDropTable(tableName, transaction, transactionMode);
        }

        public override DatabaseSchemaInfo GetSchemaInformation(string schemaName)
        {
            return _baseDb.GetSchemaInformation(schemaName);
        }

        public override bool TryCreateSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode)
        {
            return _baseDb.TryCreateSchema(schemaName, request, transactionMode);
        }

        public override bool HasSchema(string schemaName)
        {
            return _baseDb.HasSchema(schemaName);
        }

        public override Table GetTable(int tableId)
        {
            return _baseDb.GetTable(tableId);
        }

        public override int GetMaxTableId()
        {
            return _baseDb.GetMaxTableId();
        }

        public override bool HasTable(int tableId)
        {
            return _baseDb.HasTable(tableId);
        }

        public override bool HasTable(string tableName, string schemaName)
        {
            return _baseDb.HasTable(tableName, schemaName);
        }

        public override Table GetTable(string tableName, string schemaName)
        {
            return _baseDb.GetTable(tableName, schemaName);
        }

        public override Table GetTable(string tableName)
        {
            return _baseDb.GetTable(tableName);
        }

        public override bool HasUser(string userName, Guid userId)
        {
            return _baseDb.HasUser(userName, userId);
        }

        public override bool CreateUser(string userName, string pwInput)
        {
            return _baseDb.CreateUser(userName, pwInput);
        }

        public override bool HasUser(string userName)
        {
            return _baseDb.HasUser(userName);
        }

        public override bool AddTable(TableSchema schema, out Guid tableObjectId)
        {
            return _baseDb.AddTable(schema, out tableObjectId);
        }

        public override bool TryAddTable(TableSchema schema, TransactionRequest transaction, TransactionMode transactionMode, out Guid tableObjectId)
        {
            return _baseDb.TryAddTable(schema, transaction, transactionMode, out tableObjectId);
        }

        public override bool HasTable(string tableName)
        {
            return _baseDb.HasTable(tableName);
        }

        public override bool ValidateUser(string userName, string pwInput)
        {
            return _baseDb.ValidateUser(userName, pwInput);
        }

        public override bool AuthorizeUser(string userName, string pwInput, DbPermission permission, Guid objectId)
        {
            return _baseDb.AuthorizeUser(userName, pwInput, permission, objectId);
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
            return _baseDb.GrantUserPermission(userName, permission, objectId);
        }

        public override Guid GetTableObjectId(string tableName)
        {
            return _baseDb.GetTableObjectId(tableName);
        }
        #endregion

        #region Private Methods
        #endregion




    }
}
