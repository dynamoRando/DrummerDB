using Drummersoft.DrummerDB.Core.Structures.Exceptions;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal static class TransactionConstants
    {
        internal static int TransactionPreambleSize(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_IS_COMPLETED(version) +
                        SIZE_OF_TRANSACTION_SEQUENCE(version) +
                        SIZE_OF_TRANSACTION_BATCH_ID(version) +
                        SIZE_OF_AFFECTED_OBJECT_ID(version) +
                        SIZE_OF_ENTRY_TIME_UTC(version) +
                        SIZE_OF_COMPLETED_TIME_UTC(version) +
                        SIZE_OF_TRANSACTION_ACTION_TYPE(version) +
                        SIZE_OF_TRANSACTION_ACTION_VERSION(version) +
                        SIZE_OF_ACTION_BINARY_LENGTH(version) + 
                        SIZE_OF_IS_DELETED(version) +
                        SIZE_OF_USER_NAME_LENGTH(version)
                        ;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_IS_COMPLETED(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_BOOL;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int IsCompletedOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return 0;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_TRANSACTION_SEQUENCE(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int TransactionSequenceOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_IS_COMPLETED(version);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_TRANSACTION_BATCH_ID(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_GUID;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int TransactionBatchIdOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version) + SIZE_OF_IS_COMPLETED(version);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_AFFECTED_OBJECT_ID(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_GUID;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int AffectedObjectIdOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                        + SIZE_OF_TRANSACTION_BATCH_ID(version)
                        + SIZE_OF_IS_COMPLETED(version);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_ENTRY_TIME_UTC(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_DATETIME;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int EntryTimeUTCOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                        + SIZE_OF_TRANSACTION_BATCH_ID(version) +
                        SIZE_OF_AFFECTED_OBJECT_ID(version) + SIZE_OF_IS_COMPLETED(version);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_COMPLETED_TIME_UTC(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_DATETIME;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int CompletedTimeUTCOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                       + SIZE_OF_TRANSACTION_BATCH_ID(version) +
                       SIZE_OF_AFFECTED_OBJECT_ID(version) +
                       SIZE_OF_ENTRY_TIME_UTC(version) + SIZE_OF_IS_COMPLETED(version);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_TRANSACTION_ACTION_TYPE(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int TransactionActionTypeOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                       + SIZE_OF_TRANSACTION_BATCH_ID(version) +
                       SIZE_OF_AFFECTED_OBJECT_ID(version) +
                       SIZE_OF_ENTRY_TIME_UTC(version) +
                       SIZE_OF_COMPLETED_TIME_UTC(version) + SIZE_OF_IS_COMPLETED(version);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_TRANSACTION_ACTION_VERSION(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int TransactionActionVersionOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                       + SIZE_OF_TRANSACTION_BATCH_ID(version) +
                       SIZE_OF_AFFECTED_OBJECT_ID(version) +
                       SIZE_OF_ENTRY_TIME_UTC(version) +
                       SIZE_OF_COMPLETED_TIME_UTC(version) +
                       SIZE_OF_TRANSACTION_ACTION_TYPE(version) + SIZE_OF_IS_COMPLETED(version);
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_ACTION_BINARY_LENGTH(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int ActionBinaryLengthOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                       + SIZE_OF_TRANSACTION_BATCH_ID(version) +
                       SIZE_OF_AFFECTED_OBJECT_ID(version) +
                       SIZE_OF_ENTRY_TIME_UTC(version) +
                       SIZE_OF_COMPLETED_TIME_UTC(version) +
                       SIZE_OF_TRANSACTION_ACTION_TYPE(version) +
                       SIZE_OF_TRANSACTION_ACTION_VERSION(version) + SIZE_OF_IS_COMPLETED(version)
                       ;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_IS_DELETED(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_BOOL;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int IsDeletedOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                       + SIZE_OF_TRANSACTION_BATCH_ID(version) +
                       SIZE_OF_AFFECTED_OBJECT_ID(version) +
                       SIZE_OF_ENTRY_TIME_UTC(version) +
                       SIZE_OF_COMPLETED_TIME_UTC(version) +
                       SIZE_OF_TRANSACTION_ACTION_TYPE(version) +
                       SIZE_OF_TRANSACTION_ACTION_VERSION(version) + SIZE_OF_IS_COMPLETED(version)  +
                       SIZE_OF_ACTION_BINARY_LENGTH(version)
                       ;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int SIZE_OF_USER_NAME_LENGTH(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return Constants.SIZE_OF_INT;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }

        internal static int UserNameLengthOffset(int version = Constants.MAX_DATABASE_VERSION)
        {
            switch (version)
            {
                case Constants.DatabaseVersions.V100:
                    return SIZE_OF_TRANSACTION_SEQUENCE(version)
                       + SIZE_OF_TRANSACTION_BATCH_ID(version) +
                       SIZE_OF_AFFECTED_OBJECT_ID(version) +
                       SIZE_OF_ENTRY_TIME_UTC(version) +
                       SIZE_OF_COMPLETED_TIME_UTC(version) +
                       SIZE_OF_TRANSACTION_ACTION_TYPE(version) +
                       SIZE_OF_TRANSACTION_ACTION_VERSION(version) + SIZE_OF_IS_COMPLETED(version) +
                       SIZE_OF_ACTION_BINARY_LENGTH(version) +
                       SIZE_OF_IS_DELETED(version)
                       ;
                default:
                    throw new UnknownDbVersionException(version);
            }
        }
    }
}
