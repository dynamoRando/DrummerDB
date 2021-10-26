using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    internal abstract class SystemSchema : ISystemSchema
    {
        /// <summary>
        /// Returns the schema for the System User Table. This schema holds all the tables metadata (table id, table name, number of rows) in the database.
        /// </summary>
        /// <param name="dbId">The database id</param>
        /// <returns>A system table schema</returns>
        public abstract SystemTableSchema GetUserTableSchema(Guid dbId);
        public abstract SystemTableSchema GetUserTableSchema(Guid dbId, string dbName);

        /// <summary>
        /// Returns the schema for the System User Table Schema. This schema holds the schema information for all the tables (column and data types) in the database.
        /// </summary>
        /// <param name="dbId">The database id</param>
        /// <returns>A system user table schema schema</returns>
        public abstract SystemTableSchema GetUserTableSchemaSchema(Guid dbId);
        public abstract SystemTableSchema GetUserTableSchemaSchema(Guid dbId, string dbName);

        public abstract SystemTableSchema GetUserObjectsSchema(Guid dbId);
        public abstract SystemTableSchema GetUserObjectsSchema(Guid dbId, string dbName);
        public abstract SystemTableSchema GetUsersSchema(Guid dbId);
        public abstract SystemTableSchema GetUsersSchema(Guid dbId, string dbName);
    }
}
