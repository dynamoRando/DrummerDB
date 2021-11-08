using Drummersoft.DrummerDB.Common.Communication.SQLService;
using Microsoft.Extensions.Configuration;
using PerfJournal.Client;
using System;
using System.IO;
using System.Threading.Tasks;
using drummer = Drummersoft.DrummerDB.Core.Systems;

namespace Drummersoft.DrummerDB.Client.Tests
{
    internal class TestHarness
    {
        private DrummerSQLClient _sqlClient;
        private drummer.Process _process;
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private Guid _userSessionId;
        private Guid _loginGuid;
        private int _portNumber;
        private string _testDbName = string.Empty;
        private string _testTableName = string.Empty;
        private string _storageFolder = string.Empty;
        private SQLQueryReply _selectResult;
        private string _sqlSelectQuery = string.Empty;
        private string _setDb = string.Empty;
        private Settings _settings;

        // perf journal
        string _journalUrl = string.Empty;
        string _projectName = string.Empty;
        int _projectId = 0;

        public Settings Settings => _settings;

        /// <summary>
        /// Configures a test harness without use of a <see cref="Journal"/>.
        /// </summary>
        public TestHarness()
        {
            // default
        }

        public void LoadJournalSettings()
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testSettings.json").Build();

            var section = config.GetSection(nameof(Settings));
            var _settings = section.Get<Settings>();
            _journalUrl = _settings.JournalUrl;

        }

        public async Task ConfigureJournalForProjectAsync(string projectName)
        {
            _projectName = projectName;

            var hasProject = await Journal.HasProjectAsync(_journalUrl, projectName);

            if (!hasProject)
            {
                var result = await Journal.CreateProjectAsync(_journalUrl, projectName);
                if (result)
                {
                    _projectId = await Journal.GetProjectIdAsync(_journalUrl, projectName);
                }
            }
            else
            {
                _projectId = await Journal.GetProjectIdAsync(_journalUrl, projectName);
            }
        }

        public async Task<int> ConfigureJournalForTestAsync(string testName)
        {
            int testId = 0;
            var hasTest = await Journal.HasTestAsync(_journalUrl, _projectId, testName);
            if (!hasTest)
            {
                var isSuccess = await Journal.CreateTestAsync(_journalUrl, _projectId, testName);
                if (!isSuccess)
                {
                    throw new InvalidOperationException($"Unable to create test {testName}");
                }
            }
            else
            {
                testId = await Journal.GetTestIdAsync(_journalUrl, _projectId, testName);
            }

            return testId;
        }

        public async Task<bool> SaveResultToJournal(int testId, int totalTimeInMilliseconds, bool isSuccess)
        {
            return await Journal.SaveResultAsync(_journalUrl, _projectId, testId, totalTimeInMilliseconds, isSuccess);
        }

        public string SelectQuery => $@"
                SELECT * FROM {_testTableName}
                ";

        public void StopProcess()
        {
            _process.Stop();
            _process = null;
        }

        public void SetTestObjectNames(string dbName, string tableName, string storageFolder, int portNumber)
        {
            _testDbName = dbName;
            _testTableName = tableName;
            _storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, storageFolder);

            _setDb = $" USE {_testDbName};";

            _sqlSelectQuery =

            _userName = "XUnitTest";
            _password = "XUnit0123";

            _userSessionId = Guid.NewGuid();
            _portNumber = portNumber;
            _userSessionId = Guid.NewGuid();
        }

        public SQLQueryReply ExecuteSQL(string sql)
        {
            return _sqlClient.ExecuteSQL(sql, _userName, _password, _userSessionId);
        }

        public SQLQueryReply ExecuteSQL(string sql, string dbName)
        {
            return _sqlClient.ExecuteSQL(sql, dbName, _userName, _password, _userSessionId);
        }

        public void SetupClient()
        {
            string url = "http://localhost";
            _sqlClient = new DrummerSQLClient(url, _portNumber);
        }

        public void SetupTempDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(_storageFolder);

            if (directory.Exists)
            {
                foreach (var file in directory.GetFiles())
                {
                    file.Delete();
                }
            }
            else
            {
                directory.Create();
            }
        }

        public void SetupProcess()
        {
            StartProcess();
            _process.Test_SetupAdminLogin(_userName, _password, _userSessionId);

        }

        public void StartProcess()
        {
            _process = new drummer.Process(_storageFolder, true, true);
            _process.Start();
        }

        public void StartNetwork()
        {
            _process.StartSQLServer(_portNumber, false);
        }
    }
}
