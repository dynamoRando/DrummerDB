using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Databases.Interface
{
    internal interface IDbManagerInformation
    {
        int UserDatabaseCount();
        string[] UserDatabaseNames();
        int SystemDatabaseCount();
        string[] SystemDatabaseNames();
    }
}
