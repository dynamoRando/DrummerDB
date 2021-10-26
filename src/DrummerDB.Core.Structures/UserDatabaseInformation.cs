using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class UserDatabaseInformation
    {
        internal string DatabaseName { get; set; }
        internal Guid DatabaseId { get; set; }
        internal int DatabaseVersion { get; set; }

    }
}
