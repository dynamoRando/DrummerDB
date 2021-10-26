using System;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface IParticipant
    {
        public Guid Id { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }
        public string Url { get; set; }
        public bool UseHttps { get; set; }
        public int PortNumber { get; set; }
        public string Alias { get; set; }
    }
}
