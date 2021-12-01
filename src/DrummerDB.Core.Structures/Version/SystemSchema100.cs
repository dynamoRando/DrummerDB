using Drummersoft.DrummerDB.Core.Structures.Abstract;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    /// <summary>
    /// Used to generate appropriate system schemas (Version 100)
    /// </summary>
    /// <remarks>This object is used in populating a<seealso cref="SystemDataPage"/>.
    /// This action is the inverse of what happens in <seealso cref="DbMetaSystemDataPages"/>, where
    /// instead of populating a SystemDataPage, you are populating a Table from the SystemDataPage.</remarks>
    internal class SystemSchema100 : SystemSchema
    {
        /// <summary>
        /// Returns the schema for the System User Table. This schema holds all the tables metadata (table id, table name, number of rows) in the database.
        /// </summary>
        /// <returns>A system table schema</returns>
        public override SystemTableSchema GetUserTableSchema(Guid dbId)
        {
            return new SystemTableSchema100(SystemSchemaConstants100.Tables.UserTable.TABLE_ID,
                SystemSchemaConstants100.Tables.UserTable.TABLE_NAME,
                dbId,
                SystemSchemaConstants100.Tables.UserTable.GetColumns().List);
        }

        public override SystemTableSchema GetUserTableSchema(Guid dbId, string dbName)
        {
            var schema = new SystemTableSchema100(SystemSchemaConstants100.Tables.UserTable.TABLE_ID,
                SystemSchemaConstants100.Tables.UserTable.TABLE_NAME,
                dbId,
                SystemSchemaConstants100.Tables.UserTable.GetColumns().List);
            schema.DatabaseName = dbName;
            return schema;
        }

        /// <summary>
        /// Returns the schema for the System User Table Schema. This schema holds the schema information for all the tables (column and data types) in the database.
        /// </summary>
        /// <returns>A system user table schema schema</returns>
        public override SystemTableSchema GetUserTableSchemaSchema(Guid dbId)
        {
            return new SystemTableSchema100(SystemSchemaConstants100.Tables.UserTableSchema.TABLE_ID,
                SystemSchemaConstants100.Tables.UserTableSchema.TABLE_NAME,
                dbId,
                SystemSchemaConstants100.Tables.UserTableSchema.GetColumns().List);
        }

        public override SystemTableSchema GetUserTableSchemaSchema(Guid dbId, string dbName)
        {
            var schema = new SystemTableSchema100(SystemSchemaConstants100.Tables.UserTableSchema.TABLE_ID,
                SystemSchemaConstants100.Tables.UserTableSchema.TABLE_NAME,
                dbId,
                SystemSchemaConstants100.Tables.UserTableSchema.GetColumns().List);
            schema.DatabaseName = dbName;
            return schema;
        }

        public override SystemTableSchema GetUserObjectsSchema(Guid dbId)
        {
            return new SystemTableSchema100(SystemSchemaConstants100.Tables.UserObjects.TABLE_ID,
                SystemSchemaConstants100.Tables.UserObjects.TABLE_NAME, dbId, SystemSchemaConstants100.Tables.UserObjects.GetColumns().List);
        }

        public override SystemTableSchema GetUserObjectsSchema(Guid dbId, string dbName)
        {
            var schema = new SystemTableSchema100(SystemSchemaConstants100.Tables.UserObjects.TABLE_ID,
                SystemSchemaConstants100.Tables.UserObjects.TABLE_NAME, dbId, SystemSchemaConstants100.Tables.UserObjects.GetColumns().List);
            schema.DatabaseName = dbName;
            return schema;
        }

        public override SystemTableSchema GetUsersSchema(Guid dbId)
        {
            return new SystemTableSchema100(SystemSchemaConstants100.Tables.Users.TABLE_ID,
                SystemSchemaConstants100.Tables.Users.TABLE_NAME, dbId, SystemSchemaConstants100.Tables.Users.GetColumns().List);
        }

        public override SystemTableSchema GetUsersSchema(Guid dbId, string dbName)
        {
            var schema = new SystemTableSchema100(SystemSchemaConstants100.Tables.Users.TABLE_ID,
                SystemSchemaConstants100.Tables.Users.TABLE_NAME, dbId, SystemSchemaConstants100.Tables.Users.GetColumns().List);
            schema.DatabaseName = dbName;
            return schema;
        }

    }
}
