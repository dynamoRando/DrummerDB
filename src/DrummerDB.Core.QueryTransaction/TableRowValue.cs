using Drummersoft.DrummerDB.Core.Structures;
using System;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class TableRowValue
    {
        public RowValue RowValue { get; set; }
        public uint TableId { get; set; }
        public Guid DatabaseId { get; set; }
        public Guid SchemaId { get; init; }

        public TableRowValue(RowValue value, uint tableId, Guid dbId, Guid schemaId)
        {
            RowValue = value;
            TableId = tableId;
            DatabaseId = dbId;
            SchemaId = schemaId;
        }
    }
}
