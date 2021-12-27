using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface IRowRemotable
    {
        public Guid RemoteId { get; set; }
        public bool IsRemoteDeleted { get; set; }
        public DateTime RemoteDeletionUTC { get; set; }
        public uint DataHashLength { get; set; }
        public byte[] DataHash { get; set; }
    }
}
