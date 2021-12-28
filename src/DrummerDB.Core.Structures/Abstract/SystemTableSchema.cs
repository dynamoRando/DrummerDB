using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    /// <summary>
    /// An object that represents a system table in a database (in System Data Pages)
    /// </summary>
    /// <seealso cref="Drummersoft.DrummerDB.Core.Structures.Interface.ITableSchema" />
    internal abstract class SystemTableSchema : ITableSchema
    {
        public abstract ColumnSchema[] Columns { get; }
        public abstract uint Id { get; }
        public abstract string Name { get; }
        public abstract Guid DatabaseId { get; }
        public abstract TreeAddress Address { get; }
        public abstract void SortBinaryOrder();
        public abstract bool HasColumn(string columnName);
        public abstract Guid ObjectId { get; }
        public abstract DatabaseSchemaInfo Schema { get; }
        public abstract string DatabaseName { get; set; }
        public abstract LogicalStoragePolicy StoragePolicy { get; }
        public abstract Guid ContractGUID { get; set; }
        public abstract bool HasAllFixedLengthColumns();
    }
}
