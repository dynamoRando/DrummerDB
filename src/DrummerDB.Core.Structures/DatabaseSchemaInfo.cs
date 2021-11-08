using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal record DatabaseSchemaInfo
    {
        public string SchemaName { get; init; }
        public Guid SchemaGUID { get; init; }

        public DatabaseSchemaInfo(string schemaName, Guid guid)
        {
            SchemaGUID = guid;
            SchemaName = schemaName;
        }
    }
}
