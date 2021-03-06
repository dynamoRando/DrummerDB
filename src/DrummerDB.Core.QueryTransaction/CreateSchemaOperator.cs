using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class CreateSchemaOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _db;

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public string SchemaName { get; set; }
        public string DatabaseName { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public CreateSchemaOperator(string schemaName, string dbName, IDbManager dbManager, DatabaseType type)
        {
            SchemaName = schemaName;
            DatabaseName = dbName;
            _db = dbManager;
            DatabaseType = type;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            if (_db.HasUserDatabase(DatabaseName, DatabaseType))
            {
                if (_db is DbManager)
                {
                    var dbMan = _db as DbManager;
                    UserDatabase db = dbMan.GetUserDatabase(DatabaseName, DatabaseType);
                    if (!db.HasSchema(SchemaName))
                    {
                        if (db.XactCreateSchema(SchemaName, transaction, transactionMode))
                        {
                            messages.Add($"Successfully created {SchemaName} in Database {DatabaseName}");
                        }
                        else
                        {
                            errorMessages.Add($"Unable to create schema {SchemaName} in Database {DatabaseName}");
                        }
                    }
                }
            }
        }
    }
}
