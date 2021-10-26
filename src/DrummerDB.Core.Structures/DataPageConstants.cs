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
        public static int SIZE_OF_TOTAL_BYTES_USED(int version = Constants.MAX_DATABASE_VERSION)
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
        /// Returns the byte size for the Total Rows
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Total Rows</returns>
        public static int SIZE_OF_TOTAL_ROWS(int version = Constants.MAX_DATABASE_VERSION)
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
        /// Returns the byte size for the Database Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Database Id</returns>
        public static int SIZE_OF_DATABASE_ID(int version = Constants.MAX_DATABASE_VERSION)
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
        public static int SIZE_OF_TABLE_ID(int version = Constants.MAX_DATABASE_VERSION)
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
        public static int SIZE_OF_DATA_PAGE_TYPE(int version = Constants.MAX_DATABASE_VERSION)
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
        public static int TotalBytesUsedOffset(int version = Constants.MAX_DATABASE_VERSION)
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
        public static int TotalRowsOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return TotalBytesUsedOffset(version) + SIZE_OF_TOTAL_BYTES_USED(version); ;
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
        public static int RowDataStartOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return PageConstants.SIZE_OF_PAGE_PREAMBLE(version) +
                        SIZE_OF_TOTAL_BYTES_USED(version) +
                        SIZE_OF_TOTAL_ROWS(version) +
                        SIZE_OF_DATABASE_ID(version) +
                        SIZE_OF_TABLE_ID(version) +
                        SIZE_OF_DATA_PAGE_TYPE(version);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Database Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Database Id</returns>
        public static int DatabaseIdOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_TOTAL_BYTES_USED(version) + SIZE_OF_TOTAL_ROWS(version);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Table Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Table Id</returns>
        public static int TableIdOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_TOTAL_BYTES_USED(version) + SIZE_OF_TOTAL_ROWS(version) + SIZE_OF_DATABASE_ID(version);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The offset for the Data Page Type
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The offset for the Data Page Type</returns>
        public static int DataPageTypeOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return PageConstants.SIZE_OF_PAGE_PREAMBLE(version) +
                        SIZE_OF_TOTAL_BYTES_USED(version) +
                        SIZE_OF_TOTAL_ROWS(version) +
                        SIZE_OF_DATABASE_ID(version) +
                        SIZE_OF_TABLE_ID(version);
                default:
                    return 0;
            }
        }
    }
}
