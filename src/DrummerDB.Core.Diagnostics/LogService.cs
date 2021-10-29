using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Drummersoft.DrummerDB.Core.Diagnostics
{
    internal class LogService
    {
        #region Private Fields
        private Logger _logger;
        private bool _enableLogging = false;
        private bool _enablePerformanceLogging = false;
        #endregion

        #region Public Properties
        public bool IsEnabled => _enableLogging;
        public bool IsPerformanceLoggingEnabled => _enablePerformanceLogging;
        #endregion

        #region Constructors
        public LogService(Logger logger, bool enableLogging, bool enablePerformanceLogging)
        {
            _logger = logger;
            _enableLogging = enableLogging;
            _enablePerformanceLogging = enablePerformanceLogging;
        }
        #endregion

        #region Public Methods
        public void Info(string message)
        {
            if (_enableLogging)
            {
                _logger.Info(message);
            }
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
