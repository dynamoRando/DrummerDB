using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.Databases.Interface
{
    internal interface IDbManager
    {
        public IDatabase GetDatabase(string dbName, DatabaseType type);

        public HostDb GetHostDatabase(string dbName);
        public HostDb GetHostDatabase(Guid databaseId);

        HostDb GetHostDb(string dbName);

        PartialDb GetPartialDb(string dbName);

        public SystemDatabase GetSystemDatabase();
        public Table GetTable(TreeAddress address);

        public UserDatabase GetUserDatabase(string dbName, DatabaseType type);
        public UserDatabase GetUserDatabase(Guid dbId);
        public bool HasDatabase(string dbName, DatabaseType type);
        public bool HasDatabase(string dbName);

        public bool HasHostInfo();

        public bool HasSystemDatabase(string name);

        public bool HasTable(TreeAddress address);

        public bool HasUserDatabase(string name, DatabaseType type);
        public bool HasUserDatabase(string name);
        public bool HasUserDatabase(Guid dbId);
        bool IsHostDatabase(string dbName);

        bool IsPartialDatabase(string dbName);
    }
}
