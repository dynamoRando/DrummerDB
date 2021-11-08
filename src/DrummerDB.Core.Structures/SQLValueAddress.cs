using Drummersoft.DrummerDB.Common;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal struct SQLValueAddress : IEquatable<SQLValueAddress>
    {
        public Guid DatabaseId { get; set; }
        public int TableId { get; set; }
        public int PageId { get; set; }
        public int RowId { get; set; }
        public int RowOffset { get; set; }
        public int ValueOffset { get; set; }

        public bool Equals(SQLValueAddress other)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return (this.ValueOffset == other.ValueOffset) &&
                (this.RowId == other.RowId) &&
                (this.PageId == other.PageId) &&
                (this.RowOffset == other.RowOffset) &&
                 (this.DatabaseId == other.DatabaseId) &&
                 (this.TableId == other.TableId)
                ;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, this);
        }

        public static bool operator ==(SQLAddress lhs, SQLValueAddress rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SQLAddress lhs, SQLValueAddress rhs)
        {
            return !(lhs == rhs);
        }

        public byte[] ToBinaryFormat()
        {
            var arrays = new List<byte[]>(6);

            arrays.Add(DbBinaryConvert.GuidToBinary(DatabaseId));
            arrays.Add(DbBinaryConvert.IntToBinary(TableId));
            arrays.Add(DbBinaryConvert.IntToBinary(PageId));
            arrays.Add(DbBinaryConvert.IntToBinary(RowId));
            arrays.Add(DbBinaryConvert.IntToBinary(RowOffset));
            arrays.Add(DbBinaryConvert.IntToBinary(ValueOffset));

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
