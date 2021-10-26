using System;

// will be upgraded to record struct
namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying a database and table: DatabaseId, TableId, SchemaId
    /// </summary>
    internal record TreeAddress(Guid DatabaseId, int TableId, Guid SchemaId)
    {
    }
}
