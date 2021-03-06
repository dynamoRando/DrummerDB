using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class CreateHostDbOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _db;
        public string DatabaseName { get; set; }
        public int Order { get; set; }
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public CreateHostDbOperator(string dbName, IDbManager dbManager)
        {
            DatabaseName = dbName;
            _db = dbManager;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            if (!_db.HasUserDatabase(DatabaseName, DatabaseType.Host))
            {
                if (_db is DbManager)
                {
                    var db = _db as DbManager;
                    Guid dbId;
                    if (db.XactCreateNewHostDatabase(DatabaseName, transaction, transactionMode, out dbId))
                    {
                        messages.Add($"Database {DatabaseName} created with Id {dbId.ToString()}");
                    }
                }
            }
        }
    }
}
