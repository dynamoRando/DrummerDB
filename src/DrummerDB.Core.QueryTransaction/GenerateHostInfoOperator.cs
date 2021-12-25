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
    internal class GenerateHostInfoOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _manager;

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public GenerateHostInfoOperator(IDbManager manager)
        {
            _manager = manager;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            var manager = _manager as DbManager;
            var sysDb = manager.GetSystemDatabase();
            var hostGuid = sysDb.HostGUID();
            var hostName = sysDb.HostName();
            var hostToken = sysDb.HostToken();

            manager.UpdateHostInfoInDatabases(hostGuid, hostName, hostToken);
        }
    }
}
