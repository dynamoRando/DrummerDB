using Drummersoft.DrummerDB.Common.Communication.SQLService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using drummer = Drummersoft.DrummerDB.Core.Systems;


namespace Drummersoft.DrummerDB.Client.Tests
{
    internal class TestMultiHarness
    {
        #region Private Fields
        private string _rootTempDirectory = string.Empty;
        private string _journalUrl = string.Empty;
        private int _startingSQLPort;
        private int _startingDatabasePort;
        private string _url = "http://localhost";
        private string _userName = "Test";
        private string _password = "Test";
        private Guid _userSessionId = Guid.NewGuid();
        #endregion

        #region Public Properties
        public List<TestProcess> TestProcessList { get; set; }
        #endregion

        #region Constructors
        public TestMultiHarness(string testFolderName, int startingSQLPort, int startingDatabasePort)
        {
            _startingSQLPort = startingSQLPort;
            _startingDatabasePort = startingDatabasePort;

            _rootTempDirectory = Path.Combine(TestConstants.TEST_TEMP_FOLDER, testFolderName);
            TestProcessList = new List<TestProcess>();
            SetupBaseDirectory();
        }
        #endregion

        #region Public Methods
        public void LoadJournalSettings()
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testSettings.json").Build();

            var section = config.GetSection(nameof(Settings));
            var _settings = section.Get<Settings>();
            _journalUrl = _settings.JournalUrl;
        }

        public TestProcessInfo InstantiateNewProcess(string alias)
        {
            int processId = GetMaxProcessId() + 1;
            var process = new TestProcess();
            process.Alias = alias;
            process.ProcessId = processId;

            var directoryPath = Path.Combine(_rootTempDirectory, processId.ToString());
            var directory = new DirectoryInfo(directoryPath);

            if (directory.Exists)
            {
                directory.Delete(true);
            }

            directory.Create();

            process.DatabaseFolder = directoryPath;
            process.Process = new drummer.Process(directoryPath, true, true);
            process.SQLPort = GetMaxSQLPort() + 1;
            process.DatabasePort = GetMaxDbPort() + 1;
            process.UserName = _userName;
            process.Password = _password;
            process.UserSessionId = _userSessionId;

            process.Process.Start();
            process.Process.Test_SetupAdminLogin(process.UserName, process.Password, process.UserSessionId);
            process.Process.StartSQLServer(process.SQLPort, false);
            process.Process.StartDbServer(process.DatabasePort, false);
            process.SQLClient = new DrummerSQLClient(_url, process.SQLPort);
           

            TestProcessList.Add(process);

            return process.GetTestProcessInfo();
        }

        public SQLQueryReply ExecuteSQL(TestProcessInfo testProcessInfo, string sql, string dbName)
        {
            var process = GetTestProcess(testProcessInfo.ProcessId);
            var sqlClient = process.SQLClient;
            return sqlClient.ExecuteSQL(sql, dbName, process.UserName, process.Password, process.UserSessionId);
        }

        public SQLQueryReply ExecuteSQL(TestProcessInfo testProcessInfo, string sql)
        {
            var process = GetTestProcess(testProcessInfo.ProcessId);
            var sqlClient = process.SQLClient;
            return sqlClient.ExecuteSQL(sql, process.UserName, process.Password, process.UserSessionId);
        }

        public SQLQueryReply ExecuteSQL(int testProcessId, string sql, string dbName)
        {
            var process = GetTestProcess(testProcessId);
            var sqlClient = process.SQLClient;
            return sqlClient.ExecuteSQL(sql, dbName, process.UserName, process.Password, process.UserSessionId);
        }

        public SQLQueryReply ExecuteSQL(int testProcessId, string sql)
        {
            var process = GetTestProcess(testProcessId);
            var sqlClient = process.SQLClient;
            return sqlClient.ExecuteSQL(sql, process.UserName, process.Password, process.UserSessionId);
        }

        public int GetDbPort(int testProcessId)
        {
            foreach (var process in TestProcessList)
            {
                if (process.ProcessId == testProcessId)
                {
                    return process.DatabasePort;
                }
            }

            return 0;
        }

        public int GetSQLPort(int testProcessId)
        {
            foreach (var process in TestProcessList)
            {
                if (process.ProcessId == testProcessId)
                {
                    return process.SQLPort;
                }
            }

            return 0;
        }
        #endregion

        #region Private Methods
        private void SetupBaseDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(_rootTempDirectory);

            if (directory.Exists)
            {
                directory.Delete(true);
            }

            directory.Create();
        }

        private TestProcess GetTestProcess(int processId)
        {
            foreach (var process in TestProcessList)
            {
                if (process.ProcessId == processId)
                {
                    return process;
                }
            }

            return null;
        }

        private drummer.Process GetDrummerProcess(int processId)
        {
            foreach (var process in TestProcessList)
            {
                if (process.ProcessId == processId)
                {
                    return process.Process;
                }
            }

            return null;
        }

        private int GetMaxProcessId()
        {
            int maxProcessId = 0;
            foreach (var process in TestProcessList)
            {
                if (process.ProcessId > maxProcessId)
                {
                    maxProcessId = process.ProcessId;
                }
            }

            return maxProcessId;
        }

        private int GetMaxSQLPort()
        {
            int maxSQLPort = 0;
            foreach (var process in TestProcessList)
            {
                if (process.SQLPort > maxSQLPort)
                {
                    maxSQLPort = process.SQLPort;
                }
            }

            if (maxSQLPort == 0)
            {
                return _startingSQLPort;
            }

            return maxSQLPort;
        }

        private int GetMaxDbPort()
        {
            int maxDbPort = 0;
            foreach (var process in TestProcessList)
            {
                if (process.DatabasePort > maxDbPort)
                {
                    maxDbPort = process.DatabasePort;
                }
            }

            if (maxDbPort == 0)
            {
                return _startingDatabasePort;
            }

            return maxDbPort;
        }
        #endregion
    }
}
