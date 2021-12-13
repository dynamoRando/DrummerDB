using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class CreatePartialDbTransaction : TransactionActionSchema
    {
        private Contract _contract;

        public override TransactionSchemaOperation Operation => TransactionSchemaOperation.CreatePartDatabase;

        public CreatePartialDbTransaction(Contract contract)
        {
            _contract = contract;
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>(2);
            arrays.Add(DbBinaryConvert.IntToBinary((int)Operation));
            arrays.Add(_contract.ToBinaryFormat());

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
