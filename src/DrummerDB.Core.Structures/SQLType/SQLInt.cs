using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.SQLType
{
    internal struct SQLInt : ISQLType, IFixedSQLType
    {
        #region Private Fields
        private int _value;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public static implicit operator SQLInt(int value)
        {
            return new SQLInt { _value = value };
        }

        public static implicit operator int(SQLInt value)
        {
            return value._value;
        }

        bool ISQLType.IsFixedBinaryLength()
        {
            return true;
        }

        int IFixedSQLType.Size()
        {
            return Constants.SIZE_OF_INT;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
