using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;

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

            var bDbName = DbBinaryConvert.StringToBinary(_dbName);
            int dbLength = bDbName.Length;

            arrays.Add(DbBinaryConvert.IntToBinary(dbLength));
            arrays.Add(bDbName);
            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
