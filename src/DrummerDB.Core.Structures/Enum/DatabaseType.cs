using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /// <summary>
    /// The type of database. See also <seealso cref="DataFileType"/>.
    /// </summary>
    enum DatabaseType
    {
        Unknown,
        Host,
        Partial,
        System
    }
}
