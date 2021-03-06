namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal static class DrummerKeywords
    {
        // signals a statement block that is not SQL compliant, unique to DrummerDB
        public const string DRUMMER_BEGIN = "DRUMMER BEGIN";
        public const string DRUMMER_END = "DRUMMER END";

        public const string GENERATE_HOST_INFO_AS_HOSTNAME = "GENERATE HOST INFO AS HOSTNAME";
        public const string SET_LOGICAL_STORAGE = "SET LOGICAL STORAGE";
        public const string SET_NOTIFY_HOST_FOR = "SET NOTIFY HOST FOR";
        public const string REVIEW_LOGICAL_STORAGE = "REVIEW LOGICAL STORAGE";
        public const string REVIEW_PENDING_CONTRACTS = "REVIEW PENDING CONTRACTS";
        public const string REVIEW_ACCEPTED_CONTRACTS = "REVIEW ACCEPTED CONTRACTS";
        public const string REVIEW_HOST_INFO = "REVIEW HOST INFO";
        public const string ACCEPT_CONTRACT_BY = "ACCEPT CONTRACT BY";
        public const string REJECT_CONTRACT_BY = "REJECT CONTRACT BY";
        public const string FOR = "FOR";
        public const string ADD_PARTICIPANT = "ADD PARTICIPANT";
        public const string SAVE_CONTRACT = "SAVE CONTRACT";
        public const string GENERATE_CONTRACT_WITH_DESCRIPTION = "GENERATE CONTRACT WITH DESCRIPTION";
        public const string DESCRIPTION = "DESCRIPTION";
        public const string AT = "AT";
        public const string ON = "ON";
        public const string OFF = "OFF";
        public const string SET_REMOTE_DELETE_BEHAVIOR_FOR = "SET REMOTE DELETE BEHAVIOR FOR";
        public const string OPTION = "OPTION";

        // prefix denotes anything that will perform network communication with a participant
        public const string REQUEST_PARTICIPANT = "REQUEST PARTICIPANT";
        // prefix denotes anything that will perform network communication with a host
        public const string REQUEST_HOST = "REQUEST HOST";
        public const string REQUEST_HOST_NOTIFY_ACCEPTED_CONTRACT_BY = "REQUEST HOST NOTIFY ACCEPTED CONTRACT BY";

        internal static class LogicalStoragePolicyKeywords
        {
            public const string NONE = "None";
            public const string HOST_ONLY = "Host_Only";
            public const string PARTICIPANT_OWNED = "Participant_Owned";
            public const string SHARED = "Shared";
            public const string MIRROR = "Mirror";

            public static string[] StoragePolicies = new string[] { NONE, HOST_ONLY, PARTICIPANT_OWNED, SHARED, MIRROR };
        }

        internal static class RemoteDeleteBehaviorKeywords
        {
            public const string UNKNOWN = "Unknown";
            public const string IGNORE = "Ignore";
            public const string AUTO_DELETE = "Auto_Delete";
            public const string UPDATE_STATUS_ONLY = "Update_Status_Only";

            public static string[] Behaviors = new string[] { UNKNOWN, IGNORE, AUTO_DELETE, UPDATE_STATUS_ONLY };
        }

        // tenant keywords are unique to DrummerDB
        internal static class TenantKeywords
        {
            public const string APPLY_TO_TENANT = "APPLY TO TENANT";
            public const string ENABLE_TENANT_FEATURES = "ENABLE TENANT FEATURES";
        }
    }
}
