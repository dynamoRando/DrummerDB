using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class UpdateTransaction : TransactionActionData
    {
        private SQLAddress _address;
        private IRow _rowBefore;
        private IRow _rowAfter;

        public override TransactionDataOperation Operation => TransactionDataOperation.Update;
        public override SQLAddress Address => _address;
        public IRow Before => _rowBefore;
        public IRow After => _rowAfter;

        public UpdateTransaction(Guid databaseId, int tableId, int rowId, int pageId, IRow rowBefore, IRow rowAfter, Guid schemaId) : base(databaseId, tableId)
        {
            _address = new SQLAddress { DatabaseId = databaseId, TableId = tableId, PageId = pageId, RowId = rowId, RowOffset = 0, SchemaId = schemaId };
            _rowBefore = rowBefore;
            _rowAfter = rowAfter;
        }


        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>(4);

            arrays.Add(DbBinaryConvert.IntToBinary(Convert.ToInt32(Operation)));
            arrays.Add(Address.ToBinaryFormat());
            arrays.Add(_rowBefore.GetRowInTransactionBinaryFormat());
            arrays.Add(_rowAfter.GetRowInTransactionBinaryFormat());

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
