using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Structures;
using System;

namespace Drummersoft.DrummerDB.Core.Databases.Interface
{
    internal interface IDbManager
    {
        public SystemDatabase GetSystemDatabase();
        public UserDatabase GetUserDatabase(string dbName);
        public UserDatabase GetUserDatabase(Guid dbId);
        public bool HasUserDatabase(string name);
        public bool HasUserDatabase(Guid dbId);
        public bool HasSystemDatabase(string name);
        public bool HasTable(TreeAddress address);
        public Table GetTable(TreeAddress address);
        public IDatabase GetDatabase(string dbName);
        public bool HasDatabase(string dbName); 
    }
}
