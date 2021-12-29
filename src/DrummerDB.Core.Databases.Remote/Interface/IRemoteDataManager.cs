using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Interface;

namespace Drummersoft.DrummerDB.Core.Databases.Remote.Interface
{
    interface IRemoteDataManager
    {
        public TempParticipantRow GetRowFromParticipant(Participant participant, SQLAddress address, string dbName, string tableName, out string errorMessage);
    }
}
