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
        private PreviousRemoteAction _previousAction;
        private string _previousErrorMessage = string.Empty;
        private string _previousSucessMessage = string.Empty;

        private enum PreviousRemoteAction
        {
            Unknown,
            Failed,
            Success
        }

        public string DatabaseName => _database.Name;

        public RemoteSaveContractOperator(HostDb database, Participant participant)
        {
            _database = database;
            _participant = participant;
            _previousAction = PreviousRemoteAction.Unknown;
        }

        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            string errorMessage = string.Empty;
            var isSaved = _database.RequestParticipantSaveLatestContract(transaction, transactionMode, _participant, out errorMessage);

            // check to see if we've already done this before
            // we will skip saving if we've already asked the participant to save the existing contract
            if (transactionMode == TransactionMode.Commit)
            {
                if (_previousAction != PreviousRemoteAction.Unknown)
                {
                    if (_previousAction == PreviousRemoteAction.Failed)
                    {
                        errorMessages.Add(_previousErrorMessage);
                        return;
                    }
                    else
                    {
                        messages.Add(_previousSucessMessage);
                        return;
                    }
                }
            }
          

            if (!isSaved)
            {
                _previousErrorMessage = errorMessage;
                errorMessages.Add(errorMessage);
                _previousAction = PreviousRemoteAction.Failed;
            }
            else
            {
                string successMessage = $"Request sent to Participant Alias {_participant.Alias} at {_participant.IP4Address}:{_participant.PortNumber}";
                messages.Add(successMessage);
                _previousSucessMessage = successMessage;
                _previousAction = PreviousRemoteAction.Success;
            }
        }
    }
}
