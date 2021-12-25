using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class SaveContractDbAction : IDatabaseServiceAction
    {
        private Guid _id;
        private DbManager _dbManager;
        private Contract _contract;
        
        public Guid Id => _id;

        public SaveContractDbAction(Contract contract, DbManager dbManager)
        {
            _id = Guid.NewGuid();
            _contract = contract;
            _dbManager = dbManager;
        }

        public bool Execute(TransactionRequest transaction, TransactionMode transactionMode, out string errorMessage)
        {
            errorMessage = string.Empty;
            var sysDb = _dbManager.GetSystemDatabase();

            if (!sysDb.HasContractInHostsTable(_contract))
            {
                if (!sysDb.XactSaveContractToHostsTable(_contract, transaction, transactionMode))
                {
                    errorMessage = "Unable to save contract to system db";
                    return false;
                }
            }
            else
            {
                errorMessage = "This host is unknown.";
                return false;
            }


            return true;
        }
    }
}
