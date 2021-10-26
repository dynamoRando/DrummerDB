using Drummersoft.DrummerDB.Core.Storage.Abstract;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Storage.Version;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Storage.Factory
{
    /// <summary>
    /// Generates a Data File for the specified version
    /// </summary>
    internal static class DbDataFileFactory
    {
        /// <summary>
        /// Generates a Host Data File for the specified version
        /// </summary>
        /// <param name="version">The database version</param>
        /// <param name="fileName">The file name on disk</param>
        /// <param name="pages">The inital pages to save to disk</param>
        /// <param name="dbName">The database id</param>
        /// <param name="cache">A reference to cache</param>
        /// <param name="crypt">A reference to crypt</param>
        /// <returns>A Host Data File for the specified version</returns>
        /// <remarks>Use this method when constructing a new object in memory to be saved to disk</remarks>
        public static IDbDataFile GetHostDbDataFileVersion(int version, string fileName, List<IPage> pages, string dbName)
        {
            IDbDataFile result = null;

            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    result = new DbHostDataFile100(fileName, pages, dbName);
                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return result;
        }

        /// <summary>
        /// Generates a Host data file for the specified version
        /// </summary>
        /// <param name="version">The database version</param>
        /// <param name="fileName">The file name on disk</param>
        /// <param name="cache">The cache manager for metadata objects to use</param>
        /// <param name="dbId">The name of the database</param>
        /// <returns>A Host Data File for the specified version</returns>
        /// <remarks>Use this method when reading data from disk and constructing the appropriate objects in memory</remarks>
        public static IDbDataFile GetHostDbDataFileVersion(int version, string fileName, Guid dbId)
        {
            IDbDataFile result = null;

            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    result = new DbHostDataFile100(fileName, dbId);
                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return result;
        }

        public static DbSystemDataFile GetSystemDbDataFileVersion(int version, string fileName, List<IPage> pages, string dbName)
        {
            DbSystemDataFile result = null;

            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    result = new DbSystemDataFile100(fileName, pages, dbName);
                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return result;
        }

        public static DbSystemDataFile GetSystemDbDataFileVersion(int version, string fileName, Guid dbId)
        {
            DbSystemDataFile result = null;

            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    result = new DbSystemDataFile100(fileName, dbId);
                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return result;
        }

    }
}
