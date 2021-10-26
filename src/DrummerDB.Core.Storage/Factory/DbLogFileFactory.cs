using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Storage.Version;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;

namespace Drummersoft.DrummerDB.Core.Storage.Factory
{
    internal class DbLogFileFactory
    {
        public static IDbLogFile GetDbLogFileVersion(int version, string fileName)
        {
            IDbLogFile result = null;

            switch (version)
            {
                case 100:
                    result = new DbLogFileVersion100(fileName);
                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            return result;
        }
    }
}
