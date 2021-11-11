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
    internal class LogicalStoragePolicyOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private HostDb _db;
        private Table _table;
        private LogicalStoragePolicy _policy;
        private string _dbName;

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public string DatabaseName => _dbName;

        public LogicalStoragePolicyOperator(HostDb db, Table table, LogicalStoragePolicy policy, string dbName)
        {
            _db = db;
            _table = table;
            _policy = policy;
            _dbName = dbName;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            _db.SetStoragePolicyForTable(_table.Name, _policy, transaction, transactionMode);
            _table.SetLogicalStoragePolicy(_policy, transaction, transactionMode);
            messages.Add($"Successfully set policy {_policy.ToString()} for table {_table.Name} in database {_db.Name}");
        }
    }
}
