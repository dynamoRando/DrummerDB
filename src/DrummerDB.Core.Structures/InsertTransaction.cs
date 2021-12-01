using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class InsertTransaction : TransactionActionData
    {
        private SQLAddress _address;
        private byte[] _rowData;

        public override TransactionDataOperation Operation => TransactionDataOperation.Insert;
        public override SQLAddress Address => _address;

        public InsertTransaction(Guid databaseId, int tableId, int rowId, int pageId, Guid schemaId, ReadOnlySpan<byte> rowData) : base(databaseId, tableId)
        {
            _address = new SQLAddress { DatabaseId = databaseId, TableId = tableId, PageId = pageId, RowId = rowId, RowOffset = 0, SchemaId = schemaId };
            _rowData = rowData.ToArray();
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>(3);

            arrays.Add(DbBinaryConvert.IntToBinary(Convert.ToInt32(Operation)));
            arrays.Add(Address.ToBinaryFormat());
            arrays.Add(_rowData);

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
