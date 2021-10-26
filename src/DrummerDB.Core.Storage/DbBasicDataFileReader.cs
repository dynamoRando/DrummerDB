using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.IO;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// Standard class for reading any data file. This class is intended to be version agnostic.
    /// </summary>
    internal static class DbBasicDataFileReader
    {
        #region Private Fields
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        /// <summary>
        /// Reads the specified data file and attempts to read the system page to get the database version
        /// </summary>
        /// <param name="fileName">The data file</param>
        /// <returns>The database version from the database's file system page</returns>
        public static int GetDatabaseVersion(string fileName)
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position;
            int result = 0;

            using (var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                position = binaryReader.Read(span);
            }

            if (position == Constants.PAGE_SIZE)
            {
                result = DbBinaryConvert.BinaryToInt(span.Slice(SystemPageConstants.DatabaseVersionOffset(), SystemPageConstants.SIZE_OF_DATABASE_VERSION()));
            }

            return result;
        }

        public static Guid GetDatabaseId(string fileName)
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position;
            Guid result = Guid.Empty;

            using (var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                position = binaryReader.Read(span);
            }

            if (position == Constants.PAGE_SIZE)
            {
                result = DbBinaryConvert.BinaryToGuid(span.Slice(SystemPageConstants.DatabaseIdOffset(), SystemPageConstants.SIZE_OF_DATABASE_ID()));
            }

            return result;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
