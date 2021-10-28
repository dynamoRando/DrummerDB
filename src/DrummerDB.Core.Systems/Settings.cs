using System.IO;
using System.Reflection;

namespace Drummersoft.DrummerDB.Core.Systems
{
    /// <summary>
    /// A representation of the settings in appsettings.json
    /// </summary>
    internal class Settings
    {
        #region Private Fields
        private string _dbFolder = string.Empty;
        #endregion

        #region Public Properties
        /// <summary>
        /// The file extension for a host database
        /// </summary>
        public string HostDbExtension { get; set; }

        /// <summary>
        /// The file extension for a partial database
        /// </summary>
        public string PartialDbExtension { get; set; }

        /// <summary>
        /// The name of the database folder
        /// </summary>
        public string DatabaseFolderName { get; set; }

        public string IP4Adress { get; set; }

        public int SQLServicePort { get; set; }

        public int InfoServicePort { get; set; }

        public int DatabaseServicePort { get; set; }

        public string SystemDbExtension { get; set; }
        public bool LogSelectStatementsForHost { get; set; }
        public bool LogPerformanceMetrics { get; set; }
        public bool EnableLogging { get; set; }
        public string LogFileName { get; set; }

        /// <summary>
        /// The location for the database files. If blank, will return the app path + database folder name
        /// </summary>
        public string DatabaseFolder
        {
            get
            {
                return GetDatabaseFolder();
            }
            set
            {
                _dbFolder = value;
            }
        }

        /// <summary>
        /// The file extension for a database's log file
        /// </summary>
        public string DatabaseLogExtension { get; set; }

        public bool UseHttpsForConnections { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private string GetDatabaseFolder()
        {
            if (string.IsNullOrEmpty(_dbFolder))
            {
                var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var path = Path.Join(rootPath, DatabaseFolderName);
                return path;
            }
            else
            {
                return _dbFolder;
            }
        }
        #endregion
    }
}
