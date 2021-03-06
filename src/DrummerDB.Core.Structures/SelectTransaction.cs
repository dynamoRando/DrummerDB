using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class SelectTransaction : TransactionActionData
    {
        private SQLAddress _addresss;

        public SelectTransaction(Guid databaseId, uint tableId) : base(databaseId, tableId)
        {
            _addresss = new SQLAddress { DatabaseId = databaseId, TableId = tableId, PageId = 0, RowId = 0, RowOffset = 0 };
        }

        public override TransactionDataOperation Operation => TransactionDataOperation.Select;
        public override SQLAddress Address => _addresss;

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            throw new NotImplementedException();
        }
    }
}
