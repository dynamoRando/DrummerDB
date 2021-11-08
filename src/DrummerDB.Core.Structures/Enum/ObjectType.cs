namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /// <summary>
    /// An enum defining the type of database object (Table, Column, StoredProcedure, etc.)
    /// </summary>
    internal enum ObjectType
    {
        Unknown,
        Table,
        Column,
        StoredProcedure,
        View,
        Row
    }
}
