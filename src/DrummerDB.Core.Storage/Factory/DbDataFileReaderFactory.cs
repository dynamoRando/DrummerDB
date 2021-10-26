using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Storage.Version;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;

namespace Drummersoft.DrummerDB.Core.Storage.Factory
{
    /// <summary>
    /// Generates a Db Data File Reader object for the specified versions
    /// </summary>
    internal static class DbDataFileReaderFactory
    {
        /// <summary>
        /// Generates a Db Data File Reader for the specified versions
        /// </summary>
        /// <param name="fileName">The path to the file to be read on disk</param>
        /// <param name="version">The database version</param>
        /// <returns>A Db Data File Reader object for the specified version</returns>
        public static IDbDataFileReader GetDbDataFileReader(string fileName, int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return new DbDataFileReader100(fileName);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }
    }
}
