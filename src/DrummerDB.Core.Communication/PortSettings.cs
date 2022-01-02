
namespace Drummersoft.DrummerDB.Core.Communication
{
    /// <summary>
    /// Represents an IP Address and a Port Number
    /// </summary>
    record struct PortSettings
    {
        public string IPAddress { get; set; }
        public int PortNumber { get; set; }
    }
}
