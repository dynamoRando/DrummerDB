using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Factory
{

    /// <summary>
    /// Returns version appropriate System Page objects
    /// </summary>
    internal static class SystemPageFactory
    {
        /// <summary>
        /// Returns an instance of a System Page (Version 100) based on a byte array passed in from disk
        /// </summary>
        /// <param name="data">A byte array of a system page in 100 format</param>
        /// <returns>A new System Page (Version 100)</returns>
        public static SystemPage GetSystemPage100(byte[] data)
        {
            return new SystemPage100(data);
        }

        /// <summary>
        /// Returns an instance of a System Page (Version 100) based on the supplied database name
        /// </summary>
        /// <param name="databaseName">The database name</param>
        /// <param name="type">The data type file</param>
        /// <param name="dbId">The database Id</param>
        /// <returns>A new system Page (Version 100)</returns>
        public static SystemPage GetSystemPage100(string databaseName, DataFileType type, Guid dbId)
        {
            return new SystemPage100(databaseName, type, dbId);
        }
    }
}
