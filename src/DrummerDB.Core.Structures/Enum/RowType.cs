using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
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
        Temp,

        /// <summary>
        /// A row that is locally saved to the table, but for a specific tenant
        /// </summary>
        /// <remarks>This row has values AND a TenantId</remarks>
        Tenant

        // a tenant remote row is just a Remotable row
        // it will be identified by the RemoteType enum
    }
}
