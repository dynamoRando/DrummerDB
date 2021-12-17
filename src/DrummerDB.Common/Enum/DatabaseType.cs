namespace Drummersoft.DrummerDB.Common
{
    /// <summary>
    /// The type of database. See also <seealso cref="DataFileType"/>.
    /// </summary>
    public enum DatabaseType
    {
        Unknown,
        Host,
        Partial,
        System,
        Embedded, // to be made
        Tenant // to be made
    }
}
