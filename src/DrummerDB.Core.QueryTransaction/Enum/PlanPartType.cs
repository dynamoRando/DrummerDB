using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Enum
{
    internal enum PlanPartType
    {
        Unknown,
        LogicalStoragePolicy,
        Insert,
        Update,
        Delete,
        Select,
        CreateHostDb,
        CreatePartDb,
        CreateSchema,
        CreateTable,
        DropDb,
        DropTable,
        ReviewLogicalStoragePolicy,
        RemoteSaveContract,
        GenerateHostInfo,
        ReviewHostInfo,
        RemoteHostNotifyAcceptContract
    }
}
