using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;

namespace Drummersoft.DrummerDB.Core.Databases.Remote.Interface
{
    interface IRemoteDataManager
    {
        public IRow GetRowFromParticipant(Participant participant, SQLAddress address, string dbName, string tableName, out string errorMessage);
    }
}
