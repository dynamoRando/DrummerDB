using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.Databases.Interface
{
    internal interface IDbManager
    {
        public SystemDatabase GetSystemDatabase();
        public UserDatabase GetUserDatabase(string dbName, DatabaseType type);
        public UserDatabase GetUserDatabase(Guid dbId);
        public bool HasUserDatabase(string name, DatabaseType type);
        public bool HasUserDatabase(Guid dbId);
        public bool HasSystemDatabase(string name);
        public bool HasTable(TreeAddress address);
        public Table GetTable(TreeAddress address);
        public IDatabase GetDatabase(string dbName, DatabaseType type);
        public bool HasDatabase(string dbName, DatabaseType type);
        public HostDb GetHostDatabase(string dbName);
        public bool HasHostInfo();
    }
}
