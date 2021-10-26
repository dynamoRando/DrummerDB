using System;

// will be upgraded to record struct
namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A structure for identifying a page's location: DatabaseId, TableId, and PageId
    /// </summary>
    internal record PageAddress 
    {
        public Guid DatabaseId { get; init; }
        public int TableId { get; init; }
        public int PageId { get; init; }
        public Guid SchemaId { get; set; } 

        public TreeAddress TreeAddress => new TreeAddress(DatabaseId, TableId, SchemaId);

        public PageAddress(Guid dbId, int tableId, int pageId, Guid schemaId)
        {
            DatabaseId = dbId;
            TableId = tableId;
            PageId = pageId;
            SchemaId = schemaId;
        }
    }
}
