using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents attributes of a column
    /// </summary>
    internal class ColumnSchema
    {
        #region Public Properties
        /// <summary>
        /// The name of the column
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The SQL data type of the column
        /// </summary>
        public ISQLType DataType { get; set; }
        /// <summary>
        /// The ordinal index of the column
        /// </summary>
        public int Ordinal { get; set; }
        /// <summary>
        /// Specifies if the column allows NULLs or not
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// The Id of the column. In most cases, this is the same as the <seealso cref="Ordinal"/>
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Returns the varchar or char length of the field; if not this data type, then 0
        /// </summary>
        public int Length { get; set; }

        public bool IsFixedBinaryLength => DataType.IsFixedBinaryLength();
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ColumnSchema() { }

        /// <summary>
        /// Constructs an instance of a column schema. Defaults "IsNullable" to false.
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="dataType">The SQL data type of the column</param>
        /// <param name="ordinal">The order of the column</param>
        public ColumnSchema(string name, ISQLType dataType, int ordinal)
        {
            Name = name;
            DataType = dataType;
            Ordinal = ordinal;
            Id = Ordinal;
            SetLength(dataType);
            IsNullable = false;
        }

        /// <summary>
        /// Constructs an instance of a column schema
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="dataType">The SQL data type of the column</param>
        /// <param name="ordinal">The order of the column</param>
        /// <param name="isNullable">If the column can contain a NULL value or not</param>
        public ColumnSchema(string name, ISQLType dataType, int ordinal, bool isNullable)
        {
            Name = name;
            DataType = dataType;
            Ordinal = ordinal;
            Id = Ordinal;
            SetLength(dataType);
            IsNullable = isNullable;
        }
        #endregion

        #region Private Methods
        private void SetLength(ISQLType type)
        {
            switch (type)
            {
                case SQLVarChar:
                    var convertedVar = type as SQLVarChar;
                    Length = convertedVar.MaxLength;
                    break;
                case SQLChar:
                    var convertedChar = type as SQLChar;
                    Length = convertedChar.Length;
                    break;
                case SQLInt:
                    Length = Constants.SIZE_OF_INT;
                    break;
                case SQLDateTime:
                    Length = Constants.SIZE_OF_DATETIME;
                    break;
                case SQLBit:
                    Length = Constants.SIZE_OF_BOOL;
                    break;
                case SQLDecimal:
                    Length = Constants.SIZE_OF_DECIMAL;
                    break;
                case SQLVarbinary:
                    var convertedVarBin = type as SQLVarbinary;
                    Length = convertedVarBin.MaxLength;
                    break;
                default:
                    Length = 0;
                    break;
            }
        }
        #endregion
    }
}
