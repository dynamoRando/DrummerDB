using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

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
    }
}
