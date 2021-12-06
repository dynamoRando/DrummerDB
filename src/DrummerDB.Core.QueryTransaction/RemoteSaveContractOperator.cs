using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class RemoteSaveContractOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private HostDb _database;
        private Participant _participant;

        public RemoteSaveContractOperator(HostDb database, Participant participant)
        {
            _database = database;
            _participant = participant;
        }

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            string errorMessage = string.Empty;
            var isError = _database.RequestParticipantSaveLatestContract(transaction, transactionMode, _participant, out errorMessage);

            if (isError)
            {
                errorMessages.Add(errorMessage);
            }
            else
            {
                messages.Add($"Request sent to Participant Alias {_participant.Alias} at {_participant.IP4Address}:{_participant.PortNumber}");
            }
        }
    }
}
