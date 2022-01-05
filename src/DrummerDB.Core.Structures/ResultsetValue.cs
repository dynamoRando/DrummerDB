using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal struct ResultsetValue
    {
        public byte[] Value { get; set; }
        public bool IsNullValue { get; set; }
        public bool IsRemotable { get; set; }
        public bool IsRemoteOutOfSyncWithHost { get; set; }
        public bool IsLocalDeleted { get; set; }
        public bool IsRemoteDeleted { get; set; }
        public bool IsHashOutOfSyncWithHost { get; set; }
        public DateTime RemoteDeletedDateUTC { get; set; } 
    }
}
