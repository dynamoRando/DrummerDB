using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.SQLType
{
    internal class SQLDateTime : ISQLType, IFixedSQLType
    {
        #region Private Fields
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public SQLDateTime()
        {
        }
        #endregion

        #region Public Methods
        bool ISQLType.IsFixedBinaryLength()
        {
            return true;
        }

        uint IFixedSQLType.Size()
        {
            return Constants.SIZE_OF_DATETIME;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
