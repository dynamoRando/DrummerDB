using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class ParticipantSaveContractTransaction : TransactionActionSchema
    {
        private Contract _contract;
        private Participant _participant;

        public override TransactionSchemaOperation Operation => TransactionSchemaOperation.ParticipantSaveContract;

        public ParticipantSaveContractTransaction(Participant participant, Contract contract)
        {
            _contract = contract;
            _participant = participant;
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>();

            var bContract = BinaryStructHelper.Contract.ToBinary(_contract);
            var bParticpant = BinaryStructHelper.Participant.ToBinary(_participant);
            
            arrays.Add(bContract);
            arrays.Add(bParticpant);

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
