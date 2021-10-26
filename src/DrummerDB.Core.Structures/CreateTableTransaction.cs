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
    class CreateTableTransaction : TransactionActionSchema
    {
        private TableSchema _schema;

        public override TransactionSchemaOperation Operation => TransactionSchemaOperation.CreateTable;

        public CreateTableTransaction(TableSchema schema)
        {
            _schema = schema;
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>();
            arrays.Add(DbBinaryConvert.IntToBinary((int)Operation));
            arrays.Add(_schema.ToBinaryFormat());
            
            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
