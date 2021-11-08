using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    abstract class TransactionActionData : ITransactionBinary
    {
        public abstract byte[] GetDataInTransactionBinaryFormat();
        public abstract TransactionDataOperation Operation { get; }
        public abstract SQLAddress Address { get; }

        public TransactionActionData(Guid databaseId, int tableId)
        {
        }
    }
}
