using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Storage.Version;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;

namespace Drummersoft.DrummerDB.Core.Storage.Factory
{
    internal static class DbDataFileWriterFactory
    {
        public static IDbDataFileWriter GetDbDataFileWriter(string fileName, int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return new DbDataFileWriter100(fileName);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }
    }
}
