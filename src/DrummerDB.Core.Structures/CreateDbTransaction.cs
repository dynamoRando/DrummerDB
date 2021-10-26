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
    internal class CreateDbTransaction : TransactionActionSchema
    {
        private string _dbName;
        public override TransactionSchemaOperation Operation => TransactionSchemaOperation.CreateDatabase;
                
        public CreateDbTransaction(string dbName)
        {
            _dbName = dbName;
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>();
            arrays.Add(DbBinaryConvert.IntToBinary((int)Operation));
            arrays.Add(DbBinaryConvert.StringToBinary(_dbName));
            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
