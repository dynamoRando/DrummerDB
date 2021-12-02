using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal static class SQLColumnTypeConverter
    {
        public static SQLColumnType Convert(ISQLType type, int databaseVersion)
        {
            switch (databaseVersion)
            {
                case Constants.DatabaseVersions.V100:

                    switch (type)
                    {
                        case SQLInt:
                            return SQLColumnType.Int;
                        case SQLBit:
                            return SQLColumnType.Bit;
                        case SQLChar:
                            return SQLColumnType.Char;
                        case SQLDateTime:
                            return SQLColumnType.DateTime;
                        case SQLDecimal:
                            return SQLColumnType.Decimal;
                        case SQLVarChar:
                            return SQLColumnType.Varchar;
                        default:
                            throw new UnknownSQLTypeException($"SQLType: {type.GetType().ToString()} is unknown");
                    }

                default:
                    throw new UnknownDbVersionException();
            }
        }

        public static int ConvertToInt(ISQLType type, int databaseVersion)
        {
            return (int)Convert(type, databaseVersion);
        }

        public static ISQLType Convert(SQLColumnType type, int length)
        {
            switch (type)
            {
                case SQLColumnType.Int:
                    return new SQLInt();
                case SQLColumnType.Bit:
                    return new SQLBit();
                case SQLColumnType.Char:
                    return new SQLChar(length);
                case SQLColumnType.DateTime:
                    return new SQLDateTime();
                case SQLColumnType.Decimal:
                    return new SQLDecimal();
                case SQLColumnType.Varchar:
                    return new SQLVarChar(length);
                case SQLColumnType.Binary:
                    return new SQLBinary(length);
                case SQLColumnType.Varbinary:
                    return new SQLVarbinary(length);
                default:
                    throw new InvalidOperationException("Unknown data type");
            }
        }
    }
}
