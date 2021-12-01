using Drummersoft.DrummerDB.Core.Databases;
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
    internal class DropTableOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDatabase _db;
        private string _tableName;

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public IDatabase Database => _db;
        public string TableName => _tableName;

        public DropTableOperator(IDatabase db, string tableName)
        {
            _db = db;
            _tableName = tableName;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            if (transactionMode != TransactionMode.Commit)
            {
                if (_db.HasTable(_tableName))
                {
                    if (_db is HostDb)
                    {
                        var db = _db as HostDb;
                        if (db.TryDropTable(_tableName, transaction, transactionMode))
                        {
                            messages.Add($"Table {_tableName} was dropped successfully");
                        }
                        else
                        {
                            errorMessages.Add($"Unable to remove table {_tableName} from {_db.Name}");
                        }
                    }
                }
                else
                {
                    errorMessages.Add($"Database {_db.Name} does not have table {_tableName}");
                }
            }

            if (transactionMode == TransactionMode.Commit)
            {
                if (!_db.HasTable(_tableName))
                {
                    messages.Add($"Table {_tableName} was dropped successfully");
                }
                else
                {
                    errorMessages.Add($"Unable to remove table {_tableName} from {_db.Name}");
                }
            }
        }
    }
}
