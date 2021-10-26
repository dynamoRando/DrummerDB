using Drummersoft.DrummerDB.Common;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal readonly struct ColumnSchemaStruct
    {
        public readonly string Name;
        public readonly SQLColumnType DataType;
        public readonly int Length;
        public readonly int Ordinal;
        public readonly int MaxLength;
        public readonly bool IsNullable;

        public bool IsFixedBinaryLength()
        {
            // see Constants.cs
            switch (DataType)
            {
                // 4 bytes
                case SQLColumnType.Int:
                // 1 byte
                case SQLColumnType.Bit:
                // 8 byte
                case SQLColumnType.DateTime:
                // 8 byte (implemented as a Double, normally this is 16)
                case SQLColumnType.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        public ColumnSchemaStruct(string name, SQLColumnType type, int length, int ordinal, int maxLength, bool isNullable)
        {
            {
                Name = name;
                DataType = type;
                Length = length;
                Ordinal = ordinal;
                MaxLength = maxLength;
                IsNullable = isNullable;
            }
        }

    }
}
