using Drummersoft.DrummerDB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal record struct HostInfo
    {
        public Guid HostGUID { get; set; }
        public string HostName { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }
        public int DatabasePortNumber { get; set; }
        public byte[] Token { get; set; }
        public bool UseHttps { get; set; }

        public byte[] ToBinaryFormat()
        {
            var arrays = new List<byte[]>(13);

            // host guid
            var bHostGuid = DbBinaryConvert.GuidToBinary(HostGUID);
            arrays.Add(bHostGuid);

            // hostname
            var bHostName = DbBinaryConvert.StringToBinary(HostName);
            arrays.Add(bHostName);
            int hostNameLength = bHostName.Length;
            var bHostNameLength = DbBinaryConvert.IntToBinary(hostNameLength);
            arrays.Add(bHostNameLength);

            // ip4 address
            var bIP4Address = DbBinaryConvert.StringToBinary(IP4Address);
            var ip4AddressLength = bIP4Address.Length;
            var bIp4AddressLength = DbBinaryConvert.IntToBinary(bIP4Address.Length);
            arrays.Add(bIp4AddressLength);
            arrays.Add(bIP4Address);

            // ip6 address
            var bIP6Address = DbBinaryConvert.StringToBinary(IP6Address);
            var ip6AddressLength = bIP6Address.Length;
            var bIpAddressLength = DbBinaryConvert.IntToBinary(ip6AddressLength);
            arrays.Add(bIpAddressLength);
            arrays.Add(bIP6Address);

            // database port number
            var bDatabasePortNumber = DbBinaryConvert.IntToBinary(DatabasePortNumber);
            arrays.Add(bDatabasePortNumber);

            // token
            int tokenLength = Token.Length;
            var bTokenLength = DbBinaryConvert.IntToBinary(tokenLength);
            arrays.Add(bTokenLength);
            arrays.Add(Token);

            // get the total size to prefix the array with 
            var totalArray = DbBinaryConvert.ArrayStitch(arrays);
            var totalLength = totalArray.Length;

            var combinedList = new List<byte[]>();
            combinedList.Add(DbBinaryConvert.IntToBinary(totalLength));
            combinedList.Add(totalArray);

            return DbBinaryConvert.ArrayStitch(combinedList);
        }

        public void SetFromBinaryArray(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
