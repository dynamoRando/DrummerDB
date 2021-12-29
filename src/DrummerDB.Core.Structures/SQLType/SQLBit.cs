using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.SQLType
{
    internal class SQLBit : ISQLType, IFixedSQLType
    {
        #region Private Fields
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        uint IFixedSQLType.Size()
        {
            return Constants.SIZE_OF_BOOL;
        }

        bool ISQLType.IsFixedBinaryLength()
        {
            return true;
        }
        #endregion

        #region Private Methods
        #endregion



    }
}
