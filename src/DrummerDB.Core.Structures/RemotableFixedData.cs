using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class RemotableFixedData
    {
        public Guid RemoteId;
        public bool IsRemoteDeleted;
        public DateTime RemoteDeletionUTC;
        public RemoteType RemoteType;
        public uint DataHashLength;

        public RemotableFixedData(ReadOnlySpan<byte> data)
        {
            uint runningTotal = 0;
            var bRemoteId = data.Slice((int)runningTotal, RowConstants.RemotableFixedData.SIZE_OF_REMOTE_ID);
            RemoteId = DbBinaryConvert.BinaryToGuid(bRemoteId);

            runningTotal += (uint)RowConstants.RemotableFixedData.SIZE_OF_REMOTE_ID;

            var bIsRemoteDeleted = data.Slice((int)runningTotal, RowConstants.RemotableFixedData.SIZE_OF_IS_REMOTE_DELETED);
            IsRemoteDeleted = DbBinaryConvert.BinaryToBoolean(bIsRemoteDeleted);
            runningTotal += (uint)RowConstants.RemotableFixedData.SIZE_OF_IS_REMOTE_DELETED;

            var bRemoteDeletedUTC = data.Slice((int)runningTotal, RowConstants.RemotableFixedData.SIZE_OF_REMOTE_DELETED_UTC);
            RemoteDeletionUTC = DbBinaryConvert.BinaryToDateTime(bRemoteDeletedUTC);
            runningTotal += (uint)RowConstants.RemotableFixedData.SIZE_OF_REMOTE_DELETED_UTC;

            var bRemoteType = data.Slice((int)runningTotal, RowConstants.RemotableFixedData.SIZE_OF_REMOTE_TYPE);
            RemoteType = (RemoteType)DbBinaryConvert.BinaryToUInt(bRemoteType);
            runningTotal += (uint)RowConstants.RemotableFixedData.SIZE_OF_REMOTE_TYPE;

            var bDataHashLength = data.Slice((int)runningTotal, RowConstants.RemotableFixedData.SIZE_OF_DATA_HASH_LENGTH);
            DataHashLength = DbBinaryConvert.BinaryToUInt(bDataHashLength);
        }
    }
}
