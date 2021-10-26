using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.SQLType
{
    internal class SQLDecimal : ISQLType, IFixedSQLType
    {
        #region Private Fields
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        bool ISQLType.IsFixedBinaryLength()
        {
            return true;
        }

        int IFixedSQLType.Size()
        {
            return Constants.SIZE_OF_DECIMAL;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
