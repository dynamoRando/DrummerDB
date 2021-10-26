using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.SQLType
{
    internal class SQLVarbinary : ISQLType
    {
        #region Private Fields
        private int _maxLength;
        #endregion

        #region Public Properties
        public int MaxLength => _maxLength;
        #endregion

        #region Constructors
        public SQLVarbinary(int maxLength)
        {
            _maxLength = maxLength;
        }
        #endregion

        #region Public Methods
        bool ISQLType.IsFixedBinaryLength()
        {
            return false;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
