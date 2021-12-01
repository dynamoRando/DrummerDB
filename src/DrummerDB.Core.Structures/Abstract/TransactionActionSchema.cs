using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    abstract class TransactionActionSchema : ITransactionBinary
    {
        public abstract byte[] GetDataInTransactionBinaryFormat();
        public abstract TransactionSchemaOperation Operation { get; }
    }
}
