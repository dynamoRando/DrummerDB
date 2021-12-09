﻿using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class CreateTableOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _db;
        public string DatabaseName { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public int Order { get; set; }
        public string TableName { get; set; }
        public List<ColumnSchema> Columns { get; set; }
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            Guid tableId;

            if (_db.HasUserDatabase(DatabaseName, DatabaseType))
            {
                if (_db is DbManager)
                {
                    var db = _db as DbManager;
                    UserDatabase targetDb = db.GetUserDatabase(DatabaseName, DatabaseType);

                    int id = targetDb.GetMaxTableId() + 1;

                    if (TableName.Contains('.'))
                    {
                        var values = TableName.Split('.');
                        string schemaName = values[0];
                        string tableName = values[1];

                        if (targetDb.HasSchema(schemaName))
                        {
                            DatabaseSchemaInfo schemaInfo = targetDb.GetSchemaInformation(schemaName);

                            TableSchema schema = new TableSchema(id, tableName, targetDb.Id, Columns, schemaInfo);

                            if (!targetDb.HasTable(tableName) && transactionMode == TransactionMode.Try)
                            {
                                if (targetDb.XactAddTable(schema, transaction, transactionMode, out tableId))
                                {
                                    messages.Add($"Table {tableName} created with Id {tableId.ToString()}");
                                }
                            }
                            else if (targetDb.HasTable(tableName) && (transactionMode == TransactionMode.Commit || transactionMode == TransactionMode.Rollback))
                            {
                                if (targetDb.XactAddTable(schema, transaction, transactionMode, out tableId))
                                {
                                    messages.Add($"Table {tableName} created with Id {tableId.ToString()}");
                                }
                            }
                            else
                            {
                                errorMessages.Add($"Table {schemaName}.{tableName} already exists in database {DatabaseName}");
                            }
                        }
                        else
                        {
                            errorMessages.Add($"Schema {schemaName} not in database {DatabaseName}");
                        }
                    }
                    else
                    {
                        TableSchema schema = new TableSchema(id, TableName, targetDb.Id, Columns);

                        if (!targetDb.HasTable(TableName) && transactionMode == TransactionMode.Try)
                        {
                            if (targetDb.XactAddTable(schema, transaction, transactionMode, out tableId))
                            {
                                messages.Add($"Table {TableName} created with Id {tableId.ToString()}");
                            }
                        }
                        else if (targetDb.HasTable(TableName) && (transactionMode == TransactionMode.Commit || transactionMode == TransactionMode.Rollback))
                        {
                            if (targetDb.XactAddTable(schema, transaction, transactionMode, out tableId))
                            {
                                messages.Add($"Table {TableName} created with Id {tableId.ToString()}");
                            }
                        }
                        else
                        {
                            errorMessages.Add($"Table {TableName} already exists in database {DatabaseName}");
                        }
                    }
                }
            }
        }

        public CreateTableOperator(string dbName, IDbManager dbManager, string tableName)
        {
            DatabaseName = dbName;
            _db = dbManager;
            TableName = tableName;
            Columns = new List<ColumnSchema>();
        }
    }
}
