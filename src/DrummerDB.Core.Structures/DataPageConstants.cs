namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Constants for a Data Page
    /// </summary>
    internal static class DataPageConstants
    {
        /// <summary>
        /// Returns the byte size for the Total Bytes Used (normally an INT)
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Total Bytes Used</returns>
        public static ushort SIZE_OF_TOTAL_BYTES_USED(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_UINT;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the byte size for the Total Rows
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Total Rows</returns>
        public static ushort SIZE_OF_TOTAL_ROWS(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_UINT;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the byte size for the Database Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Database Id</returns>
        public static ushort SIZE_OF_DATABASE_ID(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_GUID;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the byte size for the Table Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Table Id</returns>
        public static ushort SIZE_OF_TABLE_ID(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the byte size for the Data Page Type
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Data Page Type</returns>
        public static ushort SIZE_OF_DATA_PAGE_TYPE(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Total Bytes Used
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Total Bytes Used</returns>
        public static ushort TotalBytesUsedOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return PageConstants.SIZE_OF_PAGE_PREAMBLE(version);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Total Rows
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Total Rows</returns>
        public static ushort TotalRowsOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(TotalBytesUsedOffset(version) + SIZE_OF_TOTAL_BYTES_USED(version)); ;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset where the row data starts on the page
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset where row data starts on the page</returns>
        /// <remarks>This is basically where the Data Page's Preamble ends</remarks>
        public static ushort RowDataStartOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) +
                        SIZE_OF_TOTAL_BYTES_USED(version) +
                        SIZE_OF_TOTAL_ROWS(version) +
                        SIZE_OF_DATABASE_ID(version) +
                        SIZE_OF_TABLE_ID(version) +
                        SIZE_OF_DATA_PAGE_TYPE(version));
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Database Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Database Id</returns>
        public static ushort DatabaseIdOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_TOTAL_BYTES_USED(version) + SIZE_OF_TOTAL_ROWS(version));
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Table Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Table Id</returns>
        public static ushort TableIdOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_TOTAL_BYTES_USED(version) + SIZE_OF_TOTAL_ROWS(version) + SIZE_OF_DATABASE_ID(version));
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Data Page Type
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Data Page Type</returns>
        public static ushort DataPageTypeOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) +
                        SIZE_OF_TOTAL_BYTES_USED(version) +
                        SIZE_OF_TOTAL_ROWS(version) +
                        SIZE_OF_DATABASE_ID(version) +
                        SIZE_OF_TABLE_ID(version));
                default:
                    return 0;
            }
        }
    }
}
