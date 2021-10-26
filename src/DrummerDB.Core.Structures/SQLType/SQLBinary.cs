﻿using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.SQLType
{
    internal class SQLBinary : ISQLType
    {
        #region Private Fields
        private int _length;
        #endregion

        #region Public Properties
        public int Length => _length;
        #endregion

        #region Constructors
        public SQLBinary(int length)
        {
            _length = length;
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
