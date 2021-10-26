using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using drummer = Drummersoft.DrummerDB.Core.Systems;
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Common.Communication.SQLService;
using System.IO;

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
