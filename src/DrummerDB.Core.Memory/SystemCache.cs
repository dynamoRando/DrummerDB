using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Concurrent;

namespace Drummersoft.DrummerDB.Core.Memory
{
    /// <summary>
    /// A cache to hold system pages (from each User Database)
    /// </summary>
    internal class SystemCache
    {
        #region Private Fields
        private ConcurrentDictionary<Guid, ISystemPage> _systemCache;
        private LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public SystemCache()
        {
            _systemCache = new ConcurrentDictionary<Guid, ISystemPage>();
        }

        public SystemCache(LogService log)
        {
            _systemCache = new ConcurrentDictionary<Guid, ISystemPage>();
            _log = log;
        }
        #endregion

        #region Public Methods
        public string GetDatabaseName(Guid dbId)
        {
            ISystemPage page;
            _systemCache.TryGetValue(dbId, out page);

            return page.DatabaseName;
        }

        public bool HasDatabase(Guid dbId)
        {
            return _systemCache.ContainsKey(dbId);
        }

        public void AddSystemPage(Guid dbId, ISystemPage page)
        {
            _systemCache.TryAdd(dbId, page);
        }

        public int GetDbVersion(Guid dbId)
        {
            int result = 0;
            ISystemPage page;
            _systemCache.TryGetValue(dbId, out page);

            if (page is not null)
            {
                result = page.DatabaseVersion;
            }

            return result;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
