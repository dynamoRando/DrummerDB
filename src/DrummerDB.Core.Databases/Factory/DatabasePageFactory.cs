using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.Factory;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases.Factory
{
    internal static class DatabasePageFactory
    {
        /// <summary>
        /// Returns the pages needed when creating a new datbase.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        /// <param name="type">The type of data file being created</param>
        /// <param name="dbId">The database identifier.</param>
        /// <param name="version">The version of the database.</param>
        /// <returns>A list of pages for a new database</returns>
        public static List<IPage> GetNewDatabasePages(string dbName, DataFileType type, Guid dbId, int version)
        {
            // TODO: need to make sure this is in sync with DbMetaSystemDataPages.cs
            // probably need to add Users and UserObjects

            var pages = new List<IPage>();

            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    SystemPage systemPage = SystemPageFactory.GetSystemPage100(dbName, type, dbId);
                    pages.Add(systemPage);

                    SystemSchema100 generator = SystemSchemaFactory.GetSystemSchema100();

                    PageAddress systemUserTableAddress = new PageAddress(dbId, SystemSchemaConstants100.Tables.UserTable.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUserTablePage = SystemDataPageFactory.GetSystemDataPage100(systemUserTableAddress, generator.GetUserTableSchema(dbId, dbName));

                    pages.Add(systemUserTablePage);

                    PageAddress systemUserTableSchemaAddress = new PageAddress(dbId, SystemSchemaConstants100.Tables.UserTableSchema.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUserTableSchemaPage = SystemDataPageFactory.GetSystemDataPage100(systemUserTableSchemaAddress, generator.GetUserTableSchemaSchema(dbId, dbName));

                    pages.Add(systemUserTableSchemaPage);

                    PageAddress systemUserObjectsAddress = new PageAddress(dbId, SystemSchemaConstants100.Tables.UserObjects.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUserObjectsPage = SystemDataPageFactory.GetSystemDataPage100(systemUserObjectsAddress, generator.GetUserObjectsSchema(dbId, dbName));

                    pages.Add(systemUserObjectsPage);

                    PageAddress systemUsers = new PageAddress(dbId, SystemSchemaConstants100.Tables.Users.TABLE_ID, 1, Guid.Parse(Constants.SYS_SCHEMA_GUID));
                    SystemDataPage systemUsersPage = SystemDataPageFactory.GetSystemDataPage100(systemUsers, generator.GetUsersSchema(dbId, dbName));

                    pages.Add(systemUsersPage);

                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return pages;
        }

    }
}
