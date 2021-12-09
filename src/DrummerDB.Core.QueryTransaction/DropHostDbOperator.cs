using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class DropHostDbOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _db;
        public string DatabaseName { get; set; }
        public int Order { get; set; }
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            if (_db.HasUserDatabase(DatabaseName, DatabaseType))
            {
                if (_db is DbManager)
                {
                    var db = _db as DbManager;
                    if (db.DeleteHostDatabase(DatabaseName))
                    {
                        messages.Add($"Database {DatabaseName} was removed successfully");
                    }
                }
            }
        }

        public DropHostDbOperator(string dbName, IDbManager dbManager)
        {
            DatabaseName = dbName;
            _db = dbManager;
        }
    }
}
