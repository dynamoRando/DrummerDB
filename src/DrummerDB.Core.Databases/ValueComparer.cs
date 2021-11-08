using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Databases
{
    static class ValueComparer
    {
        public static bool IsMatch(ISQLType dataType, byte[] actualValue, byte[] expectedValue, ValueComparisonOperator comparison)
        {
            switch (dataType)
            {
                case SQLInt a:
                    return IsIntMatch(actualValue, expectedValue, comparison);
                case SQLDateTime b:
                    return IsDateTimeMatch(actualValue, expectedValue, comparison);
                case SQLDecimal c:
                    return IsDecimalMatch(actualValue, expectedValue, comparison);
                case SQLBit d:
                    return IsBoolMatch(actualValue, expectedValue, comparison);
                case SQLVarChar e:
                case SQLChar f:
                    return IsStringMatch(actualValue, expectedValue, comparison);
                case SQLBinary g:
                case SQLVarbinary h:
                    return IsBinaryMatch(actualValue, expectedValue, comparison);
                default:
                    throw new InvalidOperationException("Unknown SQL type");
            }

            return false;
        }


        private static bool IsIntMatch(byte[] actualValue, byte[] expectedValue, ValueComparisonOperator comparison)
        {
            int actual = DbBinaryConvert.BinaryToInt(actualValue);
            int expected = DbBinaryConvert.BinaryToInt(expectedValue);

            switch (comparison)
            {
                case ValueComparisonOperator.GreaterThan:
                    return actual > expected;
                case ValueComparisonOperator.GreaterThanOrEqualTo:
                    return actual >= expected;
                case ValueComparisonOperator.LessThan:
                    return actual < expected;
                case ValueComparisonOperator.LessThanOrEqualTo:
                    return actual <= expected;
                case ValueComparisonOperator.Equals:
                    return actual == expected;
                case ValueComparisonOperator.NotEqualTo:
                    return actual != expected;
                default:
                    throw new InvalidOperationException("Unknown comparison type");
            }

            return false;
        }

        private static bool IsDateTimeMatch(byte[] actualValue, byte[] expectedValue, ValueComparisonOperator comparison)
        {
            DateTime actual = DbBinaryConvert.BinaryToDateTime(actualValue);
            DateTime expected = DbBinaryConvert.BinaryToDateTime(expectedValue);

            switch (comparison)
            {
                case ValueComparisonOperator.GreaterThan:
                    return actual > expected;
                case ValueComparisonOperator.GreaterThanOrEqualTo:
                    return actual >= expected;
                case ValueComparisonOperator.LessThan:
                    return actual < expected;
                case ValueComparisonOperator.LessThanOrEqualTo:
                    return actual <= expected;
                case ValueComparisonOperator.Equals:
                    return actual == expected;
                case ValueComparisonOperator.NotEqualTo:
                    return actual != expected;
                default:
                    throw new InvalidOperationException("Unknown comparison type");
            }

            return false;
        }

        private static bool IsDecimalMatch(byte[] actualValue, byte[] expectedValue, ValueComparisonOperator comparison)
        {
            decimal actual = DbBinaryConvert.BinaryToDecimal(actualValue);
            decimal expected = DbBinaryConvert.BinaryToDecimal(expectedValue);

            switch (comparison)
            {
                case ValueComparisonOperator.GreaterThan:
                    return actual > expected;
                case ValueComparisonOperator.GreaterThanOrEqualTo:
                    return actual >= expected;
                case ValueComparisonOperator.LessThan:
                    return actual < expected;
                case ValueComparisonOperator.LessThanOrEqualTo:
                    return actual <= expected;
                case ValueComparisonOperator.Equals:
                    return actual == expected;
                case ValueComparisonOperator.NotEqualTo:
                    return actual != expected;
                default:
                    throw new InvalidOperationException("Unknown comparison type");
            }

            return false;
        }

        private static bool IsBoolMatch(byte[] actualValue, byte[] expectedValue, ValueComparisonOperator comparison)
        {
            bool actual = DbBinaryConvert.BinaryToBoolean(actualValue);
            bool expected = DbBinaryConvert.BinaryToBoolean(expectedValue);

            switch (comparison)
            {
                case ValueComparisonOperator.Equals:
                    return actual == expected;
                case ValueComparisonOperator.NotEqualTo:
                    return actual != expected;
                default:
                    throw new InvalidOperationException("Unknown comparison type");
            }

            return false;
        }

        private static bool IsStringMatch(byte[] actualValue, byte[] expectedValue, ValueComparisonOperator comparison)
        {
            string actual = DbBinaryConvert.BinaryToString(actualValue);
            string expected = DbBinaryConvert.BinaryToString(expectedValue);

            switch (comparison)
            {
                case ValueComparisonOperator.Equals:
                    return actual == expected;
                case ValueComparisonOperator.NotEqualTo:
                    return actual != expected;
                default:
                    throw new InvalidOperationException("Unknown comparison type");
            }

            return false;
        }

        private static bool IsBinaryMatch(byte[] actualValue, byte[] expectedValue, ValueComparisonOperator comparison)
        {
            switch (comparison)
            {
                case ValueComparisonOperator.Equals:
                    return DbBinaryConvert.BinaryEqual(actualValue, expectedValue);
                case ValueComparisonOperator.NotEqualTo:
                    return !DbBinaryConvert.BinaryEqual(actualValue, expectedValue);
                default:
                    throw new InvalidOperationException("Unknown comparison type");
            }

            return false;
        }
    }
}
