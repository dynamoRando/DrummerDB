using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /// <summary>
    /// Defines what kind of remotable data is held in the row
    /// </summary>
    internal enum RemoteType
    {
        None,

        /// <summary>
        /// The remote data identifies the id of a Host
        /// </summary>
        Host,

        /// <summary>
        /// The remote data identfies the id of a Participant
        /// </summary>
        Participant
    }
}
