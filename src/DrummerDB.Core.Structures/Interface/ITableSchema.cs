using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface ITableSchema
    {
        ColumnSchema[] Columns { get; }

        /// <summary>
        /// The INT Id of the table.
        /// </summary>
        /// <value>
        /// The INT Id of the table.
        /// </value>
        int Id { get; }

        string Name { get; }

        Guid DatabaseId { get; }

        TreeAddress Address { get; }

        DatabaseSchemaInfo Schema { get; }

        string DatabaseName { get; }

        /// <summary>
        /// Sorts the schema in binary order (fixed binary sizes first, then variable sizes afterwards with int size prefixes).
        /// </summary>
        /// <remarks>For more information on binary order, see the markdown file Row.md</remarks>
        void SortBinaryOrder();

        /// <summary>
        /// Checks if this schema has a column with the specified name
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>
        ///   <c>true</c> if the specified column is in this schema; otherwise, <c>false</c>.
        /// </returns>
        bool HasColumn(string columnName);

        Guid ObjectId { get; }

        LogicalStoragePolicy StoragePolicy { get; }

        Guid ContractGUID { get; set; }
        bool HasAllFixedLengthColumns();
    }
}
