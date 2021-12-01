namespace Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum
{
    public enum DbPermission
    {
        Unknown,
        Select,
        Insert,
        Update,
        Delete,
        Create_Table,
        Drop_Table,
        Alter,
        ViewDefinition,
        FullAccess,
        Create_Schema,
        Set_Logical_Storage_Policy,
        Review_Logical_Storage_Policy
    }
}
