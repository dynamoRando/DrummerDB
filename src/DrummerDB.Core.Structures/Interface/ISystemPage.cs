using System;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface ISystemPage
    {
        int DatabaseVersion { get; }
        string DatabaseName { get; }
        void SetDatabaseName(string databaseName);
        Guid DatabaseId { get; }
    }
}
