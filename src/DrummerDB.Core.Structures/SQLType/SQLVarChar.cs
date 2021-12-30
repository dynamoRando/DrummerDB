using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.SQLType
{
    internal class SQLVarChar : ISQLType
    {
        #region Private Fields
        private uint _maxLength;
        #endregion

        #region Public Properties
        public uint MaxLength => _maxLength;
        #endregion

        #region Constructors
        public SQLVarChar(uint maxLength)
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
