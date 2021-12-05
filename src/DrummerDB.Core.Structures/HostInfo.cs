using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal record struct HostInfo
    {
        public Guid HostGUID { get; set; }
        public string HostName { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }
        public int DatabasePortNumber { get; set; }
        public byte[] Token { get; set; }
    }
}
