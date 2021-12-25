
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Used to denote that a participant has accepted a contract 
    /// (i.e. host has received notice from participant)
    /// </summary>
    internal class ParticipantAcceptedContractTransaction : TransactionActionSchema
    {
        private Contract _contract;
        private Participant _participant;

        public override TransactionSchemaOperation Operation => TransactionSchemaOperation.ReceivedContractAcceptance;

        public ParticipantAcceptedContractTransaction(Participant participant, Contract contract)
        {
            _contract = contract;
            _participant = participant;
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>(2);

            var bParticipant = _participant.ToBinaryFormat();
            var bContract = _contract.ToBinaryFormat();

            arrays.Add(bParticipant);
            arrays.Add(bContract);

            return DbBinaryConvert.ArrayStitch(arrays);
        }

    }
}
