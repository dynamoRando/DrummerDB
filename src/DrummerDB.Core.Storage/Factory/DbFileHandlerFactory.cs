using Drummersoft.DrummerDB.Core.Storage.Abstract;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Storage.Version;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using System;

namespace Drummersoft.DrummerDB.Core.Storage.Factory
{
    internal static class DbFileHandlerFactory
    {
        public static UserDbFileHandler GetUserDbFileHandlerForVersion(string dbName, string storageFolderPath, string dataFileExtension, string logFileExtension, IDbDataFile dataFile, IDbLogFile logFile, Guid dbId, int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return new UserDbFileHandler100(dbName, storageFolderPath, dataFileExtension, logFileExtension, dataFile, logFile, dbId);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        public static SystemDbFileHandler GetSystemDbFileHandlerForVersion(string dbName, string storageFolderPath, string dataFileExtension, string logFileExtension, DbSystemDataFile dataFile, IDbLogFile logFile, Guid dbId, int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return new SystemDbFileHandler100(dbName, storageFolderPath, dataFileExtension, logFileExtension, dataFile, logFile, dbId);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }
    }
}
