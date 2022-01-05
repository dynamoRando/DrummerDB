namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Constants for the System Page
    /// </summary>
    internal static class SystemPageConstants
    {
        public static ushort SIZE_OF_DATABASE_VERSION(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    return 0;
            }
        }

        public static ushort SIZE_OF_DATABASE_NAME(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return 30;
                default:
                    return 0;
            }
        }

        public static ushort SIZE_OF_CREATED_DATE(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_DATETIME;
                default:
                    return 0;
            }
        }

        public static ushort SIZE_OF_MAX_SYSTEM_DATA_PAGE(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    return 0;
            }
        }

        public static ushort SIZE_OF_DATA_FILE_TYPE(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    return 0;
            }
        }

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

        public static ushort DatabaseVersionOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return PageConstants.SIZE_OF_PAGE_PREAMBLE(version);
                default:
                    return 0;
            }
        }

        public static ushort DatabaseIdOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    ushort result = (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_DATABASE_VERSION(version));
                    return result;
                default:
                    return 0;
            }
        }

        public static ushort DatabaseNameOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_DATABASE_VERSION(version)
                        + SIZE_OF_DATABASE_ID(version));
                default:
                    return 0;
            }
        }

        public static ushort CreatedDateOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_DATABASE_VERSION(version) + SIZE_OF_DATABASE_NAME(version)
                        + SIZE_OF_DATABASE_ID(version));
                default:
                    return 0;
            }
        }

        public static ushort MaxSystemDataPageOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_DATABASE_VERSION(version) + SIZE_OF_DATABASE_NAME(version) + SIZE_OF_CREATED_DATE(version)
                        + SIZE_OF_DATABASE_ID(version));
                default:
                    return 0;
            }
        }

        public static ushort DataFileTypeOffset(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return (ushort)(PageConstants.SIZE_OF_PAGE_PREAMBLE(version) + SIZE_OF_DATABASE_VERSION(version) + SIZE_OF_DATABASE_NAME(version) + SIZE_OF_CREATED_DATE(version) +
                        SIZE_OF_MAX_SYSTEM_DATA_PAGE(version) + SIZE_OF_DATABASE_ID(version));
                default:
                    return 0;
            }
        }

        public static ushort MAX_LENGTH_DB_NAME(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return 30;
                default:
                    return 0;
            }
        }

        public static ushort SIZE_OF_SYSTEM_PAGE(ushort version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    ushort totalSize = 0;

                    /*
                     
                    // this is currently breaking the read back out because on read we make the assumption that all pages are the page constant size
                    // not sure what to do atm, will see if we can get away with just sticking with a default page size

                        totalSize += PageConstants.SIZE_OF_PAGE_PREAMBLE(version);
                        totalSize += SIZE_OF_DATABASE_VERSION(version);
                        totalSize += SIZE_OF_DATABASE_ID(version);
                        totalSize += SIZE_OF_DATABASE_NAME(version);
                        totalSize += SIZE_OF_CREATED_DATE(version);
                        totalSize += SIZE_OF_MAX_SYSTEM_DATA_PAGE(version);
                        totalSize += SIZE_OF_DATA_FILE_TYPE(version);
                    */

                    totalSize = Constants.PAGE_SIZE;

                    return totalSize;
                default:
                    return 0;
            }
        }
    }
}
