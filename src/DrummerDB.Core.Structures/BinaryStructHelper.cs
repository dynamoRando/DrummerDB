using Drummersoft.DrummerDB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal static class BinaryStructHelper
    {
        public static class Contract
        {
            public static byte[] ToBinary(Structures.Contract contract)
            {
                // byte layout:

                /*
                 * HostInfo (object)
                 * Guid hostGuid
                 * string hostName
                 * string ip4Address
                 * string ip6Address
                 * int databasePortNumber
                 * byte[] token
                 * 
                 * (rest of fields)
                 * Guid contractGuid
                 * DateTime generatedDate
                 * string description
                 * string databaseName
                 * Guid databaseId
                 * List<TableSchema> Tables
                 * Guid version
                 * ContractStatus (enum) Status
                 */

                var arrays = new List<byte[]>();

                var bHostGuid = DbBinaryConvert.GuidToBinary(contract.Host.HostGUID);
                arrays.Add(bHostGuid);

                var bHostNameWithLength = DbBinaryConvert.StringToBinary(contract.Host.HostName, true);
                arrays.Add(bHostNameWithLength);

                var bIp4AddressWithLength = DbBinaryConvert.StringToBinary(contract.Host.IP4Address, true);
                arrays.Add(bIp4AddressWithLength);

                var bIp6AddressWithLength = DbBinaryConvert.StringToBinary(contract.Host.IP6Address, true);
                arrays.Add(bIp6AddressWithLength);

                var bDatabasePortNumber = DbBinaryConvert.IntToBinary(contract.Host.DatabasePortNumber);
                arrays.Add(bDatabasePortNumber);

                var bTokenLength = DbBinaryConvert.IntToBinary(contract.Host.Token.Length);
                arrays.Add(bTokenLength);
                arrays.Add(contract.Host.Token);

                var bContractGuid = DbBinaryConvert.GuidToBinary(contract.ContractGUID);
                arrays.Add(bContractGuid);

                var bGeneratedDate = DbBinaryConvert.DateTimeToBinary(contract.GeneratedDate.ToString());
                arrays.Add(bGeneratedDate);

                var bDescriptionWithLength = DbBinaryConvert.StringToBinary(contract.Description, true);
                arrays.Add(bDescriptionWithLength);

                var bDatabaseId = DbBinaryConvert.GuidToBinary(contract.DatabaseId);
                arrays.Add(bDatabaseId);

                // need to loop thru each schema and seralize
                var binaryTables = new List<byte[]>();

                foreach (var table in contract.Tables)
                {
                    var cTable = table as TableSchema;
                    var data = cTable.ToBinaryFormat();

                    binaryTables.Add(data);
                }

                // for each table, we need to prefix with an INT to 
                // specify how long the byte data for each table schema is
                // when we are parsing.

                // first, add the total amount of bytes to account for the 
                // total table schema
                int totalTableSchemaLengh = 0;

                foreach (var table in binaryTables)
                {
                    totalTableSchemaLengh += table.Length;
                }

                // add the total binary length of all table arrays
                arrays.Add(DbBinaryConvert.IntToBinary(totalTableSchemaLengh));

                // next, add each table, including an int perfix so we know
                // how long each table is
                foreach (var bTable in binaryTables)
                {
                    int iBTableLength = bTable.Length;
                    var bTableLength = DbBinaryConvert.IntToBinary(iBTableLength);
                    arrays.Add(bTableLength);
                    arrays.Add(bTable);
                }

                arrays.Add(DbBinaryConvert.GuidToBinary(contract.Version));
                arrays.Add(DbBinaryConvert.IntToBinary((int)contract.Status));

                return DbBinaryConvert.ArrayStitch(arrays);
            }

            public static Structures.Contract ToContract(byte[] data)
            {
                throw new NotImplementedException();
            }

            public static Structures.Contract ToContract(ReadOnlySpan<byte> data)
            {
                throw new NotImplementedException();
            }
        }

        public static class Participant
        {
            public static byte[] ToBinary(Structures.Participant participant)
            {
                var arrays = new List<byte[]>();

                arrays.Add(DbBinaryConvert.GuidToBinary(participant.Id));
                arrays.Add(DbBinaryConvert.StringToBinary(participant.IP4Address, true));
                arrays.Add(DbBinaryConvert.StringToBinary(participant.IP6Address, true));
                arrays.Add(DbBinaryConvert.IntToBinary(participant.PortNumber));
                arrays.Add(DbBinaryConvert.StringToBinary(participant.Url, true));
                arrays.Add(DbBinaryConvert.BooleanToBinary(participant.UseHttps));
                arrays.Add(DbBinaryConvert.StringToBinary(participant.Alias, true));

                return DbBinaryConvert.ArrayStitch(arrays);
            }

            public static Structures.Participant ToParticpant(byte[] data)
            {
                throw new NotImplementedException();
            }

            public static Structures.Participant ToParticipant(ReadOnlySpan<byte> data)
            {
                throw new NotImplementedException();
            }
        }

    }
}
