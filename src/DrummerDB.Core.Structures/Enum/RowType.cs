using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drummersoft.DrummerDB.Core.Structures.Abstract;

namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /// <summary>
    /// Identifies the type of row
    /// </summary>
    /// <remarks>This is useful to determine the shape of the binary array
    /// on the <see cref="BaseDataPage"/> </remarks>
    internal enum RowType
    {
        /// <summary>
        /// An unknown row type
        /// </summary>
        Unknown,

        /// <summary>
        /// A row that has values local to the database (i.e. like any other row in a RMDBS).
        /// </summary>
        /// <remarks>Should only be used in tables with a <see cref="LogicalStoragePolicy"/> of either
        /// <see cref="LogicalStoragePolicy.HostOnly"/> or <see cref="LogicalStoragePolicy.None"/></remarks>
        Local,

        /// <summary>
        /// A row that has values stored elsewhere (remotable only data)
        /// </summary>
        /// <remarks>Used for Host Rows</remarks>
        Remoteable,

        /// <summary>
        /// A row that has data locally and has a reference elsewhere (values + remotable)
        /// </summary>
        /// <remarks>Used for Partial Rows</remarks>
        RemotableAndLocal,

        /// <summary>
        /// A row that has values that is not persisted to xact log/disk/cache 
        /// </summary>
        /// <remarks>Used when creating a row for Cooperative Insert to send data to 
        /// partial database</remarks>
        TempParticipantRow
    }
}
