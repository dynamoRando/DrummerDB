namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /// <summary>
    /// The type of database. See also <seealso cref="DataFileType"/>.
    /// </summary>
    internal enum DatabaseType
    {
        Unknown,
        Host,
        Partial,
        System,
        Embedded // to be made
    }
}
