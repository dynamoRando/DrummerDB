using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using System;

namespace Drummersoft.DrummerDB.Core.Storage.Factory
{
    internal class DbLogFileWriterFactory
    {
        public static IDbLogFileWriter GetDbLogFileWriter(string fileName,
            int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    break;
                default:
                    throw new UnknownDbVersionException(version);
            }

            throw new NotImplementedException();
        }
    }
}
