using NLog;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;

namespace Drummersoft.DrummerDB.Core.Diagnostics
{
    internal class LogService
    {
        #region Private Fields
        private Logger _logger;
        private bool _enableLogging = false;
        private bool _enablePerformanceLogging = false;
        private const string XACT_BATCH_ID = "XACT_BATCH_ID";
        private const string SQL_STATEMENT = "STATEMENT";
        private const string METHOD_NAME = "METHOD_NAME";
        private const string TOTAL_MILLISECONDS = "TOTAL_MILLISECONDS";
        private const string ASSEMBLY_NAME = "ASSEMBLY_NAME";

        public const string PERFORMANCE = "PERFORMANCE";

        #endregion

        #region Public Properties
        public bool IsEnabled => _enableLogging;
        public bool IsPerformanceLoggingEnabled => _enablePerformanceLogging;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor. Used mainly for tests.
        /// </summary>
        public LogService()
        {
        }

        /// <summary>
        /// Creates a new instance of a log service.
        /// </summary>
        /// <param name="logger">A reference to the underlying NLog object</param>
        /// <param name="enableLogging"><c>TRUE</c> if you want to log to disk, otherwise <c>FALSE</c></param>
        /// <param name="enablePerformanceLogging"><c>TRUE</c> if you want to log performance data, otherwise <c>FALSE</c></param>
        public LogService(Logger logger, bool enableLogging, bool enablePerformanceLogging)
        {
            _logger = logger;
            _enableLogging = enableLogging;
            _enablePerformanceLogging = enablePerformanceLogging;
        }
        #endregion

        #region Public Methods
        public void Error(Exception exception, string message, [CallerMemberName] string callerName = "")
        {
            if (_enableLogging)
            {
                _logger.Error(exception, $"{Assembly.GetCallingAssembly().GetName().Name} : {callerName} :: {message}", exception.Message.ToString());
            }
        }

        public void Info(string message, [CallerMemberName] string callerName = "")
        {
            if (_enableLogging)
            {
                _logger.Info($"{Assembly.GetCallingAssembly().GetName().Name} : {callerName} :: {message}");
            }
        }


        public void Performance(string methodName, double totalTimeInMilliseocnds)
        {
            if (_enableLogging)
            {
                if (_enablePerformanceLogging)
                {
                    string logMessage = $"{PERFORMANCE} :: {METHOD_NAME} : {methodName} - {TOTAL_MILLISECONDS} : {totalTimeInMilliseocnds.ToString()}";
                    _logger.Info(logMessage);
                }
            }
        }

        public void Performance(string assemblyName, string methodName, double totalTimeInMilliseocnds)
        {
            if (_enableLogging)
            {
                if (_enablePerformanceLogging)
                {
                    string logMessage = $"{PERFORMANCE} :: {ASSEMBLY_NAME} : {assemblyName} : {METHOD_NAME} : {methodName} - {TOTAL_MILLISECONDS} : {totalTimeInMilliseocnds.ToString()}";
                    _logger.Info(logMessage);
                }
            }
        }

        public void Performance(string methodName, double totalTimeInMilliseocnds, Guid transactionBatchId, string sqlStatement)
        {
            if (_enableLogging)
            {
                if (_enablePerformanceLogging)
                {
                    string logMessage = $"{PERFORMANCE} :: {XACT_BATCH_ID} : {transactionBatchId.ToString()} : {METHOD_NAME} : {methodName} - {TOTAL_MILLISECONDS} : {totalTimeInMilliseocnds.ToString()} : {SQL_STATEMENT} : {sqlStatement} ";
                    _logger.Info(logMessage);
                }
            }
        }

        public static string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
