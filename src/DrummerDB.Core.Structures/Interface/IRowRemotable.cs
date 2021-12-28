using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    /// <summary>
    /// Represents a row that has a reference to it elsewhere from the local database
    /// </summary>
    internal interface IRowRemotable
    {
        public Guid RemoteId { get; set; }
        public bool IsRemoteDeleted { get; set; }
        public DateTime RemoteDeletionUTC { get; set; }
        public uint DataHashLength { get; }
        public byte[] DataHash { get; }
        public RemoteType RemoteType { get; }
    }
}
