using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class CreateSchemaOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _db;

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public string SchemaName { get; set; }
        public string DatabaseName { get; set; }

        public CreateSchemaOperator(string schemaName, string dbName, IDbManager dbManager)
        {
            SchemaName = schemaName;
            DatabaseName = dbName;
            _db = dbManager;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            if (_db.HasUserDatabase(DatabaseName))
            {
                if (_db is DbManager)
                {
                    var dbMan = _db as DbManager;
                    UserDatabase db = dbMan.GetUserDatabase(DatabaseName);
                    if (!db.HasSchema(SchemaName))
                    {
                        if (db.TryCreateSchema(SchemaName, transaction, transactionMode))
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
