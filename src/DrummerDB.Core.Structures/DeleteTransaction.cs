using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class DeleteTransaction : TransactionActionData
    {
        private SQLAddress _address;
        private IRow _rowToBeDeleted;

        public override TransactionDataOperation Operation => TransactionDataOperation.Delete;
        public override SQLAddress Address => _address;
        public IRow DeletedRow => _rowToBeDeleted;

        public DeleteTransaction(Guid databaseId, uint tableId, uint rowId, uint pageId, uint rowOffset, IRow rowToBeDeleted, Guid schemaId) : base(databaseId, tableId)
        {
            _address = new SQLAddress { DatabaseId = databaseId, TableId = tableId, PageId = pageId, RowId = rowId, RowOffset = rowOffset, SchemaId = schemaId };
            _rowToBeDeleted = rowToBeDeleted;
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>(3);

            arrays.Add(DbBinaryConvert.IntToBinary(Convert.ToInt32(Operation)));
            arrays.Add(Address.ToBinaryFormat());
            arrays.Add(_rowToBeDeleted.GetRowInTransactionBinaryFormat());

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
