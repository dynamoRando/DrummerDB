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

    /// <summary>
    /// Represents a SELECT action against an entire table
    /// </summary>
    /// <remarks>The binary data for this transaction will be a 1 byte <c>BOOL</c> of <c>FALSE</c></remarks>
    internal class SelectTableTransaction : TransactionActionData
    {
        private SQLAddress _address;
     
        public override TransactionDataOperation Operation => TransactionDataOperation.SelectEntireTableOrView;
        public override SQLAddress Address => _address;

        /// <summary>
        /// Represents a SELECT action against an entire table
        /// </summary>
        /// <param name="databaseId">The database the SELECT operation is being performed on</param>
        /// <param name="tableId">The table the SELECT operation is being performed on</param>
        public SelectTableTransaction(Guid databaseId, int tableId) : base(databaseId, tableId)
        {
            _address = new SQLAddress { DatabaseId = databaseId, TableId = tableId, PageId = 0, RowId = 0, RowOffset = 0 };   
        }

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            var arrays = new List<byte[]>(3);

            arrays.Add(DbBinaryConvert.IntToBinary(Convert.ToInt32(Operation)));
            arrays.Add(Address.ToBinaryFormat());
            arrays.Add(Data());

            return DbBinaryConvert.ArrayStitch(arrays);
        }

        private byte[] Data()
        {
            return DbBinaryConvert.BooleanToBinary(false);
        }
    }
}
