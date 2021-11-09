using Drummersoft.DrummerDB.Core.Databases;
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
    // do we still need this? Maybe we should instead use a SEELCT operator
    internal class ReviewLogicalStoragePolicyOperator : IQueryPlanPartOperator, ISQLQueryable
    {
        private ValueAddressCollection _result;
        private HostDb _db;
        private Table _table;

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public ValueAddressCollection Result => _result;
        public int Order { get; set; }

        public ReviewLogicalStoragePolicyOperator(HostDb db, Table table)
        {
            _result = new ValueAddressCollection();

            _db = db;
            _table = table;
        }

        public List<ValueAddress> Execute(TransactionRequest transaction, TransactionMode transactionMode)
        {
            var result = _table.GetLogicalStoragePolicy();
            // this needs to translate to a SELECT LogicalStoragePolicy FROM sys.UserTables
            // in the target HostDb
            
            throw new NotImplementedException();
        }
    }
}
