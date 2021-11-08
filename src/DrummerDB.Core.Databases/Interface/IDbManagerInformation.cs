namespace Drummersoft.DrummerDB.Core.Databases.Interface
{
    internal interface IDbManagerInformation
    {
        int UserDatabaseCount();
        string[] UserDatabaseNames();
        int SystemDatabaseCount();
        string[] SystemDatabaseNames();
    }
}
