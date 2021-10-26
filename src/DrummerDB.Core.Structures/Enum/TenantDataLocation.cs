using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    // based on Tenant_And_Participant_Db_Design.doc
    internal enum TenantDataLocation
    {
        Unknown,
        InTable,
        ParallelTable,
        ParallelDatabase,
        RemoteDatabase
    }
}
