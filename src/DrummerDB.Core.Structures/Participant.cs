using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal record struct Participant
    {
        public Guid Id { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }
        public int PortNumber { get; set; }
        public string Url { get; set; }
        public bool UseHttps { get; set; }
        public string Alias { get; set; }

        public byte[] ToBinaryFormat()
        {
            var arrays = new List<byte[]>(11);

            var bId = DbBinaryConvert.GuidToBinary(Id);
            arrays.Add(bId);

            var bIp4Address = DbBinaryConvert.StringToBinary(IP4Address);
            int ip4AddressLength = bIp4Address.Length;
            var bIp4AddresLength = DbBinaryConvert.IntToBinary(ip4AddressLength);

            arrays.Add(bIp4AddresLength);
            arrays.Add(bIp4Address);


            var bIp6Address = DbBinaryConvert.StringToBinary(IP6Address);
            int ip6AddressLength = bIp6Address.Length;
            var bIp6AddresLength = DbBinaryConvert.IntToBinary(ip6AddressLength);

            arrays.Add(bIp6AddresLength);
            arrays.Add(bIp6Address);

            var bPortNumber = DbBinaryConvert.IntToBinary(PortNumber); ;
            arrays.Add(bPortNumber);

            var bUrl = DbBinaryConvert.StringToBinary(Url);
            int urlLength = bUrl.Length;
            var bUrlLength = DbBinaryConvert.IntToBinary(urlLength);

            arrays.Add(bUrlLength);
            arrays.Add(bUrl);

            var bUseHttps = DbBinaryConvert.BooleanToBinary(UseHttps);
            arrays.Add(bUseHttps);

            var bAlias = DbBinaryConvert.StringToBinary(Alias);
            int aliasLength = bAlias.Length;
            var bAliasLength = DbBinaryConvert.IntToBinary(aliasLength);

            arrays.Add(bAliasLength);
            arrays.Add(bAlias);

            return DbBinaryConvert.ArrayStitch(arrays);
        }
    }
}
