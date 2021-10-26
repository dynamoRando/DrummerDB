using Drummersoft.DrummerDB.Client;
using System.Collections.Generic;
using System;

namespace Drummersoft.DrummerDB.Browser.Services
{
    public class Connection
    {
        private DrummerDatabaseClient _dbClient;
        private DrummerInfoClient _infoClient;
        private DrummerSQLClient _sqlClient;
        private Guid _userSession;

        public const string DATABASE_QUERY = "SELECT * FROM sys.Databases";
        public const string SYSTEM_DATABASE_NAME = "drumSystem";
        public string IPAddress { get; set; }
        public int SQLPort { get; set; }
        public int InfoPort { get; set; }
        public int DatabasePort { get; set; }
        public DrummerDatabaseClient DatabaseClient => _dbClient;
        public DrummerInfoClient InfoClient => _infoClient;
        public DrummerSQLClient SQLClient => _sqlClient;
        public Guid UserSession => _userSession;

        public string CurrentDatabaseName { get; set; }
        public List<string> CurrentDatabaseNames { get; set; }
        public List<string> CurrrentTableNames { get; set; }

        public Connection() { }

        public Connection(string ipAddress, int sqlPort, int infoPort, int dbPort)
        {
            IPAddress = ipAddress;
            SQLPort = sqlPort;
            InfoPort = infoPort;
            DatabasePort = dbPort;

            _dbClient = new DrummerDatabaseClient(ipAddress, dbPort);
            _infoClient = new DrummerInfoClient(ipAddress, infoPort);
            _sqlClient = new DrummerSQLClient(ipAddress, sqlPort);

            CurrentDatabaseNames = new List<string>();
            CurrentDatabaseName = string.Empty;
            CurrrentTableNames = new List<string>();
        }

        public void Init()
        {
            _dbClient = new DrummerDatabaseClient(IPAddress, DatabasePort);
            _sqlClient = new DrummerSQLClient(IPAddress, SQLPort);
            _infoClient = new DrummerInfoClient(IPAddress, InfoPort);

            _userSession = Guid.NewGuid();

            CurrentDatabaseNames = new List<string>();
            CurrentDatabaseName = string.Empty;
            CurrrentTableNames = new List<string>();
        }
    }
}
