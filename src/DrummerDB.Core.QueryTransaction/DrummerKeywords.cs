﻿namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal static class DrummerKeywords
    {
        public const string DRUMMER_BEGIN = "DRUMMER BEGIN";
        public const string DRUMMER_END = "DRUMMER END";
        public const string SET_LOGICAL_STORAGE = "SET LOGICAL STORAGE";
        public const string REVIEW_LOGICAL_STORAGE = "REVIEW LOGICAL STORAGE";
        public const string FOR = "FOR";
        public const string ADD_PARTICIPANT = "ADD PARTICIPANT";
        public const string REQUEST_PARTICIPANT = "REQUEST PARTICIPANT";
        public const string ACCEPT_CONTRACT = "ACCEPT CONTRACT";
        public const string GENERATE_CONTRACT_AS_AUTHOR = "GENERATE CONTRACT AS AUTHOR";
        public const string DESCRIPTION = "DESCRIPTION";

        internal static class LogicalStoragePolicyKeywords
        {
            public const string NONE = "None";
            public const string HOST_ONLY = "Host_Only";
            public const string PARTICIPANT_OWNED = "Participant_Owned";
            public const string SHARED = "Shared";
            public const string MIRROR = "Mirror";

            public static string[] StoragePolicies = new string[] { NONE, HOST_ONLY, PARTICIPANT_OWNED, SHARED, MIRROR };
        }
    }
}
