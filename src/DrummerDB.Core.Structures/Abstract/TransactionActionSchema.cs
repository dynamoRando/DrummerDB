using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    abstract class TransactionActionSchema : ITransactionBinary
    {
        public abstract byte[] GetDataInTransactionBinaryFormat();
        public abstract TransactionSchemaOperation Operation { get; }
    }
}
