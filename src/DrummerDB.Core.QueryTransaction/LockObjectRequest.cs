using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class LockObjectRequest
    {
        public Guid Id { get; }
        public string ObjectName { get; set; }
        public Guid ObjectId { get; set; }
        public SQLAddress Address { get; set; }
        public LockType LockType { get; set; }
        public int LockOrder { get; set; }
        public ObjectType ObjectType { get; set; }

        public LockObjectRequest()
        {
            Id = Guid.NewGuid();
        }
    }
}
