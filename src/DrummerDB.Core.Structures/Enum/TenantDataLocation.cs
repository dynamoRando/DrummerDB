namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    // based on Tenant_And_Participant_Db_Design.doc
    internal enum TenantDataLocation
    {
        Unknown,
        InTable,
        ParallelTable,
        ParallelDatabase,
        RemoteDatabase
    }
}
