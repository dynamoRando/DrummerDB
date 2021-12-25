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
    internal class AcceptContractDbAction : IDatabaseServiceAction
    {
        private Guid _id;
        private Participant _participant;
        private Contract _contract;
        private DbManager _dbManager;

        public Guid Id => _id;

        public AcceptContractDbAction(Participant participant, Contract contract, DbManager dbManager)
        {
            _id = Guid.NewGuid();
            _participant = participant;
            _contract = contract;
            _dbManager = dbManager; 
        }

        public bool Execute(TransactionRequest transaction, TransactionMode transactionMode, out string errorMessage)
        {
            bool isSuccessful = false;
            var db = _dbManager.GetHostDatabase(_contract.DatabaseName);

            if (db is not null)
            {
                isSuccessful = db.XactUpdateParticipantAcceptsContract(_participant, _contract.ContractGUID, transaction, transactionMode, out errorMessage);
            }
            else
            {
                errorMessage = $"Database {_contract.DatabaseName} was not found as a local host database";
                return false;
            }

            errorMessage = string.Empty;
            return isSuccessful;
        }
    }
}
