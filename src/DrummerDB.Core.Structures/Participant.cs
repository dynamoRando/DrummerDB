using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class Participant : IParticipant
    {
        public Guid Id { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }
        public int PortNumber { get; set; }
        public string Url { get; set; }
        public bool UseHttps { get; set; }
        public string Alias { get; set; }
    }
}
