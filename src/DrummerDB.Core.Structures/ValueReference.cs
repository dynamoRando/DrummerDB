namespace Drummersoft.DrummerDB.Core.Structures
{
    internal struct ValueReference
    {
        public int TableId;
        public string TableName;
        public ColumnSchemaStruct Column;
        public ValueAddress Address;
    }
}
