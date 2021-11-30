using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal static class DbUtil
    {
        public static ColumnSchema GetColumn(ColumnSchema[] columns, string columnName)
        {
            foreach (var column in columns)
            {
                if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }

            return null;
        }

        public static ColumnSchemaStruct ConvertColumnSchemaToStruct(ColumnSchema column)
        {
            SQLColumnType columnType;

            switch (column.DataType)
            {
                case SQLVarChar varchar:
                    columnType = SQLColumnType.Varchar;
                    break;
                case SQLChar item:
                    columnType = SQLColumnType.Char;
                    break;
                case SQLInt foo:
                    columnType = SQLColumnType.Int;
                    break;
                case SQLDateTime a:
                    columnType = SQLColumnType.DateTime;
                    break;
                case SQLBit b:
                    columnType = SQLColumnType.Bit;
                    break;
                case SQLDecimal c:
                    columnType = SQLColumnType.Decimal;
                    break;
                case SQLVarbinary d:
                    columnType = SQLColumnType.Varbinary;
                    break;
                default:
                    throw new UnknownSQLTypeException($"{column.DataType.GetType().ToString()} is unknown");
            }

            return new ColumnSchemaStruct(column.Name, columnType, column.Length, column.Ordinal, column.Length, column.IsNullable);
        }
    }
}
