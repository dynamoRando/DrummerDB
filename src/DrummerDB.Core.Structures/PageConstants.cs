namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Constants that apply to all page types
    /// </summary>
    internal static class PageConstants
    {
        /// <summary>
        /// Returns the byte size for the Page Id
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Page Id</returns>
        public static ushort SIZE_OF_PAGE_ID(ushort version = Constants.MAX_DATABASE_VERSION)
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
        /// Returns the byte size for the page type
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Page Type</returns>
        public static ushort SIZE_OF_PAGE_TYPE(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_UINT;
                default:
                    return 0;
            }
        }

        public static ushort SIZE_OF_IS_DELETED(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_BOOL;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the byte size for the Page Preamble
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte size for the Page Preamble</returns>
        public static ushort SIZE_OF_PAGE_PREAMBLE(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(SIZE_OF_PAGE_ID(version) + SIZE_OF_PAGE_TYPE(version) + SIZE_OF_IS_DELETED(version));
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the byte offset for the Page Id
        /// </summary>
        /// <returns>The byte offset for the Page Id</returns>
        public static ushort PageIdOffset()
        {
            return 0;
        }

        /// <summary>
        /// Returns the byte offset for the Page Type
        /// </summary>
        /// <param name="version">The database version</param>
        /// <returns>The byte offset for the Page Type</returns>
        public static ushort PageTypeOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_PAGE_ID(version);
                default:
                    return 0;
            }
        }

        public static ushort PageIsDeletedOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(SIZE_OF_PAGE_ID(version) + SIZE_OF_PAGE_TYPE(version));
                default:
                    return 0;
            }
        }
    }
}
