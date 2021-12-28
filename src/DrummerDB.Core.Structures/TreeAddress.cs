using System;


namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying a database and table: DatabaseId, TableId, SchemaId
    /// </summary>
    internal record struct TreeAddress(Guid DatabaseId, uint TableId, Guid SchemaId)
    {
    }
}
