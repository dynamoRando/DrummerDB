using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal record TreeAddressFriendly (string Database, string Table, string Schema, TreeAddress address)
    {
    }
}
