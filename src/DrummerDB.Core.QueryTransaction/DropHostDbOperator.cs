using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System;
using Drummersoft.DrummerDB.Common;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class DropHostDbOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private bool _wasDroppedInTry = false;
        private IDbManager _db;
        public string DatabaseName { get; set; }
        public int Order { get; set; }
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            if (transactionMode != TransactionMode.Try && _wasDroppedInTry)
            {
                messages.Add($"Database {DatabaseName} was removed successfully");
                return;
            }

            if (_db.HasDatabase(DatabaseName))
            {
                if (_db is DbManager)
                {
                    var db = _db as DbManager;
                    if (db.DeleteHostDatabase(DatabaseName))
                    {
                        messages.Add($"Database {DatabaseName} was removed successfully");

                        if (transactionMode == TransactionMode.Try)
                        {
                            _wasDroppedInTry = true;
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Database {DatabaseName} was not found!");
            }
        }

        public DropHostDbOperator(string dbName, IDbManager dbManager)
        {
            DatabaseName = dbName;
            _db = dbManager;
        }
    }
}
