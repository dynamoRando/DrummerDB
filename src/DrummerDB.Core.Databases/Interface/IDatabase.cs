using System;

namespace Drummersoft.DrummerDB.Core.Databases.Interface
{
    internal interface IDatabase
    {
        public Guid Id { get; }
        public string Name { get; }
        public bool HasTable(string tableName);
        public Table GetTable(uint tableId);
        public Table GetTable(string tableName);
        public bool HasTable(string tableName, string schemaName);
    }
}
