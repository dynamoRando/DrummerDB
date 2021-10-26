using Drummersoft.DrummerDB.Core.Memory.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Databases
{
    /// <summary>
    /// An abstraction over the System Page in a database
    /// </summary>
    internal class DbMetaSystemPage
    {
        #region Private Fields
        private ICacheManager _cache;
        private Guid _dbId;
        private int _version;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public DbMetaSystemPage(ICacheManager cache, Guid dbId, int version)
        {
            _cache = cache;
            _dbId = dbId;
            _version = version;
        }
        #endregion

        #region Public Methods
        public string GetDatabaseName(Guid dbId)
        {
            return _cache.GetDatabaseName(dbId);
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
