using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying a page's location: DatabaseId, TableId, and PageId
    /// </summary>
    internal record struct PageAddress
    {
        public Guid DatabaseId { get; init; }
        public uint TableId { get; init; }
        public uint PageId { get; init; }
        public Guid SchemaId { get; set; }

        public TreeAddress TreeAddress => new TreeAddress(DatabaseId, TableId, SchemaId);

        public PageAddress(Guid dbId, uint tableId, uint pageId, Guid schemaId)
        {
            DatabaseId = dbId;
            TableId = tableId;
            PageId = pageId;
            SchemaId = schemaId;
        }
    }
}
