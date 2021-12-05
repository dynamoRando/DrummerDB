using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using structContract = Drummersoft.DrummerDB.Core.Structures.Contract;
using comContract = Drummersoft.DrummerDB.Common.Communication.Contract;
using comColSchema = Drummersoft.DrummerDB.Common.Communication.ColumnSchema;
using comTableSchema = Drummersoft.DrummerDB.Common.Communication.TableSchema;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Core.Structures;

namespace Drummersoft.DrummerDB.Core.Databases.Remote
{
    internal class ContractConverter
    {
        public static comContract ConvertContractForCommunication(structContract contract)
        {
            var comContract = new comContract();
            comContract.AuthorName = contract.AuthorName;
            comContract.ContractGUID = contract.ContractGUID.ToString();
            comContract.Token = ByteString.CopyFrom(contract.Token);
            comContract.Description = contract.Description;
            comContract.ContractVersion = contract.Version.ToString();
            comContract.GeneratedDate = contract.GeneratedDate.ToTimestamp();

            var comSchema = new DatabaseSchema();
            comSchema.DatabaseId = contract.DatabaseId.ToString();
            comSchema.DatabaseName = contract.DatabaseName;

            foreach (var tableSchema in contract.Tables)
            {
                var comTable = new comTableSchema();
                comTable.DatabaseId = contract.DatabaseId.ToString();
                comTable.DatabaseName = contract.DatabaseName;
                comTable.TableId = Convert.ToUInt32(tableSchema.Id);
                comTable.TableName = tableSchema.Name;
                comTable.LogicalStoragePolicy = Convert.ToUInt32(tableSchema.StoragePolicy);

                foreach (var column in tableSchema.Columns)
                {
                    var comColumn = new comColSchema();
                    comColumn.ColumnId = Convert.ToUInt32(column.Id);
                    comColumn.ColumnName = column.Name;
                    comColumn.ColumnType = Convert.ToUInt32(SQLColumnTypeConverter.ConvertToInt(column.DataType, Constants.DatabaseVersions.V100));
                    comColumn.ColumnLength = Convert.ToUInt32(column.Length);
                    comColumn.IsNullable = column.IsNullable;
                    comColumn.Ordinal = Convert.ToUInt32(column.Ordinal);
                    comColumn.TableId = tableSchema.Id.ToString();

                    comTable.Columns.Add(comColumn);
                }

                comSchema.Tables.Add(comTable);
            }


            comContract.Schema = comSchema;

            return comContract;
        }
    }
}
