using System;
using System.Collections.Generic;
using System.Text;

namespace Drummersoft.DrummerDB.Common
{
    /// <summary>
    /// Utility class for converting values to binary format and vice versa
    /// </summary>
    static public class DbBinaryConvert
    {
        public static decimal BinaryToDecimal(ReadOnlySpan<byte> span)
        {
            double item = BitConverter.ToDouble(span);
            return Convert.ToDecimal(item);
        }

        /// <summary>
        /// Converts an integer to a byte array
        /// </summary>
        /// <param name="i">The integer to convert</param>
        /// <returns>The byte array</returns>
        public static byte[] IntToBinary(int i)
        {
            return BitConverter.GetBytes(i);
        }

        public static byte[] UIntToBinary(uint ui)
        {
            return BitConverter.GetBytes(ui);
        }
        
        public static byte[] IntToBinary(string value)
        {
            int i;
            i = Convert.ToInt32(value);
            return BitConverter.GetBytes(i);
        }

        /// <summary>
        /// Converts a string value to a binary array (UTF8)
        /// </summary>
        /// <param name="value">The string value to convert to bytes</param>
        /// <returns>A byte array representation of the string</returns>
        public static byte[] StringToBinary(string value, bool includeLengthPrefix = false)
        {
            if (!includeLengthPrefix)
            {
                return Encoding.UTF8.GetBytes(value);
            }
            else
            {
                var bValue = Encoding.UTF8.GetBytes(value);
                int length = bValue.Length;
                var bLength = BitConverter.GetBytes(length);

                var array = new byte[bLength.Length + bValue.Length];
                Array.Copy(bLength, 0, array, 0, bLength.Length);
                Array.Copy(bValue, 0, array, bLength.Length, bValue.Length);
                return array;
            }
        }

        public static byte[] DateTimeToBinary(string value)
        {
            DateTime item;
            byte[] result = null;
            if (DateTime.TryParse(value, out item))
            {
                result = BitConverter.GetBytes(item.ToBinary());
            }
            else
            {
                throw new InvalidOperationException($"could not convert {value} to DATETIME");
            }

            return result;
        }

        /// <summary>
        /// Converts a decimal value to byte array
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The byte array</returns>
        public static byte[] DecimalToBinary(string value)
        {
            double item;
            item = Convert.ToDouble(value);
            return BitConverter.GetBytes(item);
        }

        /// <summary>
        /// Converts a boolean value to binary array
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>A binary array</returns>
        public static byte[] BooleanToBinary(string value)
        {
            bool item;
            if (value.ToUpper() == "TRUE" || value.Equals("1"))
            {
                item = true;
            }
            else
            {
                item = false;
            }

            return BitConverter.GetBytes(item);
        }

        public static byte[] BooleanToBinary(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts a binary array to int
        /// </summary>
        /// <param name="span">The binary array</param>
        /// <returns>The int</returns>
        public static int BinaryToInt(ReadOnlySpan<byte> span)
        {
            return BitConverter.ToInt32(span);
        }

        public static uint BinaryToUInt(ReadOnlySpan<byte> span)
        {
            return BitConverter.ToUInt32(span);
        }

        /// <summary>
        /// Converts bytes to a boolean value
        /// </summary>
        /// <param name="span">The bytes</param>
        /// <returns>The boolean value</returns>
        public static bool BinaryToBoolean(ReadOnlySpan<byte> span)
        {
            return BitConverter.ToBoolean(span);
        }

        /// <summary>
        /// Converts an array of bytes to a DateTime value
        /// </summary>
        /// <param name="bytes">The bytes to be parsed</param>
        /// <returns>A DateTime object</returns>
        public static DateTime BinaryToDateTime(ReadOnlySpan<byte> bytes)
        {
            long item = BitConverter.ToInt64(bytes);
            return DateTime.FromBinary(item);
        }

        /// <summary>
        /// Converts an array of bytes to a DateTime value
        /// </summary>
        /// <param name="bytes">The bytes to be parsed</param>
        /// <returns>A DateTime object</returns>
        public static DateTime BinaryToDateTime(byte[] bytes)
        {
            long item = BitConverter.ToInt64(bytes);
            return DateTime.FromBinary(item);
        }

        /// <summary>
        /// Converts a Guid to binary array
        /// </summary>
        /// <param name="guid">The guid to convert</param>
        /// <returns>The binary representation of the GUID</returns>
        public static byte[] GuidToBinary(Guid guid)
        {
            return guid.ToByteArray();
        }

        /// <summary>
        /// Converts a byte array to a Guid
        /// </summary>
        /// <param name="span">The Guid binary array</param>
        /// <returns>A Guid</returns>
        public static Guid BinaryToGuid(Span<byte> span)
        {
            return new Guid(span);
        }

        /// <summary>
        /// Converts a byte array to a Guid
        /// </summary>
        /// <param name="span">The Guid binary array</param>
        /// <returns>A Guid</returns>
        public static Guid BinaryToGuid(ReadOnlySpan<byte> span)
        {
            return new Guid(span);
        }

        /// <summary>
        /// Returns a string based on the supplied binary array (assumes UTF8)
        /// </summary>
        /// <param name="bytes">The binary array</param>
        /// <returns>The string represented by the byte array</returns>
        public static string BinaryToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Returns a string based on the supplied binary array (assumes UTF8)
        /// </summary>
        /// <param name="bytes">The binary array</param>
        /// <returns>The string represented by the byte array</returns>
        public static string BinaryToString(ReadOnlySpan<byte> bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Combines the supplied arrays together into one array.
        /// </summary>
        /// <param name="arrays">The arrays to combine together.</param>
        /// <returns>A combined array from the List array parameter.</returns>
        public static byte[] ArrayStitch(List<byte[]> arrays)
        {
            int totalLength = 0;
            int runningTotal = 0;

            arrays.ForEach(array => totalLength += array.Length);

            byte[] finalArray = new byte[totalLength];

            foreach (var array in arrays)
            {
                Array.Copy(array, 0, finalArray, runningTotal, array.Length);
                runningTotal += array.Length;
            }

            return finalArray;
        }

        //https://stackoverflow.com/questions/18472867/checking-equality-for-two-byte-arrays
        public static bool BinaryEqual(byte[] a, byte[] b)
        {
            int i;
            if (a.Length == b.Length)
            {
                i = 0;
                while (i < a.Length && (a[i] == b[i]))
                {
                    i++;
                }
                if (i == a.Length)
                {
                    return true;
                }
            }

            return false;
        }

        //https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net/48599119#48599119
        public static bool BinaryEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
    }
}
