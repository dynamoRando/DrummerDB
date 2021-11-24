using System;

namespace Drummersoft.DrummerDB.Core.Databases.Interface
{
    internal interface IDatabase
    {
        public Guid Id { get; }
        public string Name { get; }
        public bool HasTable(string tableName);
        public Table GetTable(int tableId);
        public Table GetTable(string tableName);

        /// <summary>
        /// Validates that user tables in the database have a logical storage policy set on them
        /// </summary>
        /// <returns><c>TRUE</c> if all tables have a logical storage policy, otherwise <c>FALSE</c></returns>
        public bool IsReadyForCooperation();
    }
}
