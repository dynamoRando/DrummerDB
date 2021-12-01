using Drummersoft.DrummerDB.Common.Communication.SQLService;
using Microsoft.Extensions.Configuration;
using PerfJournal.Client;
using System;
using System.IO;
using System.Threading.Tasks;
using drummer = Drummersoft.DrummerDB.Core.Systems;

namespace Drummersoft.DrummerDB.Client.Tests
{
    /// <summary>
    /// Internal DrummerDB.Client.Tests object used to configure temp test folder, etc.
    /// </summary>
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

        public TestHarness()
        {
        }

        /// <summary>
        /// Loads the testSetings.json file into memory for PerfJournal URL, etc
        /// </summary>
        public void LoadJournalSettings()
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testSettings.json").Build();

            var section = config.GetSection(nameof(Settings));
            var _settings = section.Get<Settings>();
            _journalUrl = _settings.JournalUrl;

        }

        /// <summary>
        /// Calls PerfJournal to configure tests for the specified ProjectName. If the ProjectName does not exist, it will ask
        /// PerfJournal to create it.
        /// </summary>
        /// <param name="projectName">The project for which all tests in this harness will record results for.</param>
        /// <returns>A task to be used for async calls</returns>
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

        /// <summary>
        /// Calls PerfJournal to determine the specified Test's Id. If the TestName does not exist, it will ask PerfJournal
        /// to create it.
        /// </summary>
        /// <param name="testName">The name of the test</param>
        /// <returns>The id of the test</returns>
        /// <exception cref="InvalidOperationException">Thrown if any error occurs during configuration</exception>
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

        /// <summary>
        /// Saves a time measurement for a test to PerfJournal
        /// </summary>
        /// <param name="testId">The test id</param>
        /// <param name="totalTimeInMilliseconds">The total test time</param>
        /// <param name="isSuccess">If the test passed or failed</param>
        /// <returns><c>TRUE</c> if the result was saved, otherwise <c>FALSE</c></returns>
        public async Task<bool> SaveResultToJournal(int testId, int totalTimeInMilliseconds, bool isSuccess)
        {
            return await Journal.SaveResultAsync(_journalUrl, _projectId, testId, totalTimeInMilliseconds, isSuccess);
        }

        /// <summary>
        /// A default SELECT * FROM for the default test table
        /// </summary>
        public string SelectQuery => $@"
                SELECT * FROM {_testTableName}
                ";

        /// <summary>
        /// Brings the test DrummerDB.Process object offline and NULLs it
        /// </summary>
        public void StopProcess()
        {
            _process.Stop();
            _process = null;
        }

        /// <summary>
        /// Configures settings for the test function; sets the full temp folder path, configures the default database name, the SQL port number, etc. 
        /// See also <seealso cref="TestConstants.TestPortNumbers"/> for a list of port numbers for the entire test suite.
        /// </summary>
        /// <param name="dbName">The default database to be used in the test</param>
        /// <param name="tableName">The default table to be used in the test</param>
        /// <param name="storageFolder">The temp folder where the databases will be created for the test</param>
        /// <param name="portNumber">The DrummerDB.Process SQL port number to be used. See <seealso cref="TestConstants.TestPortNumbers">TestPortNumbers</seealso> for a list.</param>
        public void SetTestObjectNames(string dbName, string tableName, string storageFolder, int portNumber)
        {
            _testDbName = dbName;
            _testTableName = tableName;
            _storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, storageFolder);

            _setDb = $" USE {_testDbName};";

            _userName = "XUnitTest";
            _password = "XUnit0123";

            _userSessionId = Guid.NewGuid();
            _portNumber = portNumber;
            _userSessionId = Guid.NewGuid();
        }

        /// <summary>
        /// Calls the SQLClient with the specified SQL statement with the previously configued username/password/sessionId
        /// </summary>
        /// <param name="sql">The SQL statement to execute</param>
        /// <returns>A SQL Query Reply object with the results, if any</returns>
        public SQLQueryReply ExecuteSQL(string sql)
        {
            return _sqlClient.ExecuteSQL(sql, _userName, _password, _userSessionId);
        }

        /// <summary>
        /// Calls the SQLClient with the specified SQL statement with the previously configued username/password/sessionId
        /// </summary>
        /// <param name="sql">The SQL statement to execute</param>
        /// <param name="dbName">The name of the database to execute the SQL against</param>
        /// <returns>A SQL Query Reply object with the results, if any</returns>
        public SQLQueryReply ExecuteSQL(string sql, string dbName)
        {
            return _sqlClient.ExecuteSQL(sql, dbName, _userName, _password, _userSessionId);
        }

        /// <summary>
        /// Instantiates the internal DrummerSQLClient object to url = "localhost" and configures it with the previously specified SQL Port Number
        /// </summary>
        public void SetupClient()
        {
            string url = "http://localhost";
            _sqlClient = new DrummerSQLClient(url, _portNumber);
        }

        /// <summary>
        /// Ensures that the temp directory for the tests exists, and if it does, deletes andy previous files in it
        /// </summary>
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

        /// <summary>
        /// Insantiates the internal DrummerDB.Process object, calls the Start() function of it, and configures the 
        /// default admin login and session id
        /// </summary>
        public void SetupProcess()
        {
            StartProcess();
            _process.Test_SetupAdminLogin(_userName, _password, _userSessionId);

        }

        /// <summary>
        /// Instantiates a new DrummerDB.Process object with the specified temp directory folder path and brings online existing
        /// user and system databases
        /// </summary>
        public void StartProcess()
        {
            _process = new drummer.Process(_storageFolder, true, true);
            _process.Start();
        }

        /// <summary>
        /// Brings online the DrummerDB.Process' SQL Service to respond to SQL queries
        /// </summary>
        public void StartNetwork()
        {
            _process.StartSQLServer(_portNumber, false);
        }
    }
}
