﻿
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
    internal class AcceptContractTransaction : TransactionActionSchema
    {
        private Contract _contract;
        public override TransactionSchemaOperation Operation => TransactionSchemaOperation.NotifyAcceptContract;

        public AcceptContractTransaction(Contract contract)
        {
            _contract = contract;
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            return _contract.ToBinaryFormat();
        }
    }
}
