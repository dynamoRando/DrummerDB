using Drummersoft.DrummerDB.Core.Databases.Remote.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Tests.Mocks
{
    internal class MockRemoteDataManager : IRemoteDataManager
    {
        public IRow GetRowFromParticipant(Participant participant, SQLAddress address, string dbName, string tableName, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        TempParticipantRow IRemoteDataManager.GetRowFromParticipant(Participant participant, SQLAddress address, string dbName, string tableName, out string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
