using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents the schema of a table
    /// </summary>
    internal class TableSchema : ITableSchema
    {
        #region Private Fields
        private ColumnSchema[] _columns;
        private uint _Id;
        private string _name;
        private Guid _dbId;
        private TreeAddress _address => new TreeAddress(DatabaseId, _Id, _schema.SchemaGUID);
        private Guid _objectId;
        private DatabaseSchemaInfo _schema;
        private LogicalStoragePolicy _storagePolicy;
        private string _dbName;
        #endregion

        #region Public Properties
        /// <summary>
        /// The columns of a table
        /// </summary>
        public ColumnSchema[] Columns => _columns;
        /// <summary>
        /// The local id of the table
        /// </summary>
        public uint Id => _Id;
        /// <summary>
        /// The name of the table
        /// </summary>
        public string Name => _name;
        /// <summary>
        /// The local database id that the table belongs to
        /// </summary>
        public Guid DatabaseId => _dbId;
        /// <summary>
        /// The address for this table (database id, table id)
        /// </summary>
        public TreeAddress Address => _address;

        public Guid ObjectId => _objectId;
        public DatabaseSchemaInfo Schema => _schema;

        public string DatabaseName => _dbName;
        public LogicalStoragePolicy StoragePolicy => _storagePolicy;
        public Guid ContractGUID { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new table in the specified schema and with the specified logical storage policy
        /// </summary>
        /// <param name="id">The id of the table</param>
        /// <param name="name">The name of the table</param>
        /// <param name="dbId">The database id the table is in</param>
        /// <param name="columns">The columns of the table</param>
        /// <param name="schema">The database schema the table belongs to</param>
        public TableSchema(uint id, string name, Guid dbId, List<ColumnSchema> columns, DatabaseSchemaInfo schema, LogicalStoragePolicy policy)
        {
            _Id = id;
            _name = name;
            _dbId = dbId;
            _columns = columns.ToArray();
            _schema = schema;
            _storagePolicy = policy;
            ContractGUID = Guid.Empty;
        }


        /// <summary>
        /// Creates a new table in the specified schema, defaults storage policy to none
        /// </summary>
        /// <param name="id">The id of the table</param>
        /// <param name="name">The name of the table</param>
        /// <param name="dbId">The database id the table is in</param>
        /// <param name="columns">The columns of the table</param>
        /// <param name="schema">The database schema the table belongs to</param>
        public TableSchema(uint id, string name, Guid dbId, List<ColumnSchema> columns, DatabaseSchemaInfo schema, string databaseName)
        {
            _Id = id;
            _name = name;
            _dbId = dbId;
            _columns = columns.ToArray();
            _schema = schema;
            _storagePolicy = LogicalStoragePolicy.None;
            ContractGUID = Guid.Empty;
            _dbName = databaseName;
        }

        /// <summary>
        /// Creates a new table in default schema dbo, defaults storage policy to none
        /// </summary>
        /// <param name="id">The id of the table</param>
        /// <param name="name">The name of the table</param>
        /// <param name="dbId">The database id the table is in</param>
        /// <param name="columns">The columns of the table</param>
        public TableSchema(uint id, string name, Guid dbId, List<ColumnSchema> columns, string databaseName)
        {
            _Id = id;
            _name = name;
            _dbId = dbId;
            _columns = columns.ToArray();
            _storagePolicy = LogicalStoragePolicy.None;
            _dbName = databaseName;

            _schema = new DatabaseSchemaInfo(Constants.DBO_SCHEMA, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            ContractGUID = Guid.Empty;

        }

        public TableSchema(uint id, string name, Guid dbId, List<ColumnSchema> columns, Guid objectId, string databaseName) : this(id, name, dbId, columns, databaseName)
        {
            _objectId = objectId;
        }

        public TableSchema(ReadOnlySpan<byte> binaryData)
        {
            uint currentOffset = 0;
            var columns = new List<ColumnSchema>();
            string tableSchemaName = string.Empty;
            Guid tableSchemaGuid = Guid.Empty;
            uint colMaxLength = 0;

            /*
            * Database Guid
            * Database Name Length
            * Database Name
            * Table Int
            * Table Name Length
            * Table Name
            * Table Schema GUID
            * Table Schema Name
            * Table Total Columns (INT)
            * Table Columns Total Length Binary Length
            * Table Columns
            *      - Column Ordinal (INT)
            *      - Column Name Length
            *      - Column Name
            *      - Column Data Type (see SQLColumnTypeConverter.ConvertToInt())
            *      - Column Max Length (as an INT)
            *      - Column Bool IsNullable
            */

            // database guid
            _dbId = DbBinaryConvert.BinaryToGuid(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_GUID));

            currentOffset += Constants.SIZE_OF_GUID;

            uint nameLength = 0;

            // db name length
            nameLength = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
            currentOffset += Constants.SIZE_OF_INT;

            // dbNamee
            _name = DbBinaryConvert.BinaryToString(binaryData.Slice((int)currentOffset, (int)nameLength));

            currentOffset += nameLength;

            // table int
            _Id = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
            currentOffset += Constants.SIZE_OF_INT;

            // table name length
            uint tableNameLength = 0;
            tableNameLength = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
            currentOffset += Constants.SIZE_OF_INT;

            // table name
            _name = DbBinaryConvert.BinaryToString(binaryData.Slice((int)currentOffset, (int)tableNameLength));
            currentOffset += tableNameLength;

            // table schema guid 

            tableSchemaGuid = DbBinaryConvert.BinaryToGuid(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_GUID));
            currentOffset += Constants.SIZE_OF_GUID;

            // table schema name length
            uint tableSchemaNameLength = 0;
            tableSchemaNameLength = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
            currentOffset += Constants.SIZE_OF_INT;

            // table schema name
            tableSchemaName = DbBinaryConvert.BinaryToString(binaryData.Slice((int)currentOffset, (int)tableSchemaNameLength));
            currentOffset += tableSchemaNameLength;

            // table total columns
            uint totalColumns = 0;
            totalColumns = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
            currentOffset += Constants.SIZE_OF_INT;

            // total columns total binary length
            uint tableTotalColumnsBinaryLength = 0;
            tableTotalColumnsBinaryLength = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
            currentOffset += Constants.SIZE_OF_INT;

            // parse each column
            int currentColumnCount = 0;
            while (currentColumnCount < totalColumns)
            {
                // ordinal
                uint columnOrdinal = 0;
                columnOrdinal = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
                currentOffset += Constants.SIZE_OF_INT;

                // name length
                uint columnNameLength = 0;
                columnNameLength = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
                currentOffset += Constants.SIZE_OF_INT;

                // column name
                string columnName = string.Empty;
                columnName = DbBinaryConvert.BinaryToString(binaryData.Slice((int)currentOffset, (int)columnNameLength));
                currentOffset += columnNameLength;

                // column data type
                uint columnDataType = 0;
                columnDataType = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
                currentOffset += Constants.SIZE_OF_INT;

                SQLColumnType columnType = (SQLColumnType)columnDataType;

                // col max length
                colMaxLength = DbBinaryConvert.BinaryToUInt(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_INT));
                currentOffset += Constants.SIZE_OF_INT;

                // is nullable
                bool isNullable = false;
                isNullable = DbBinaryConvert.BinaryToBoolean(binaryData.Slice((int)currentOffset, Constants.SIZE_OF_BOOL));
                currentOffset += Constants.SIZE_OF_BOOL;

                ISQLType colType;

                switch (columnType)
                {
                    case SQLColumnType.Int:
                        colType = new SQLInt();
                        break;
                    case SQLColumnType.Bit:
                        colType = new SQLBit();
                        break;
                    case SQLColumnType.Char:
                        colType = new SQLChar(colMaxLength);
                        break;
                    case SQLColumnType.DateTime:
                        colType = new SQLDateTime();
                        break;
                    case SQLColumnType.Decimal:
                        colType = new SQLDecimal();
                        break;
                    case SQLColumnType.Varchar:
                        colType = new SQLVarChar(colMaxLength);
                        break;
                    case SQLColumnType.Unknown:
                        throw new InvalidOperationException("Unknown column type");
                    default:
                        throw new InvalidOperationException("Unknown column type");
                }

                var column = new ColumnSchema(columnName, colType, columnOrdinal, isNullable);
                columns.Add(column);

                currentColumnCount++;
            }

            _columns = columns.ToArray();
            _schema = new DatabaseSchemaInfo(tableSchemaName, tableSchemaGuid);
        }
        #endregion

        #region Public Methods
        public bool HasAllFixedLengthColumns()
        {
            foreach (var column in _columns)
            {
                if (!column.IsFixedBinaryLength)
                {
                    return false;
                }
            }

            return true;
        }
        public void SetStoragePolicy(LogicalStoragePolicy storagePolicy)
        {
            _storagePolicy = storagePolicy;
        }

        public void SortBinaryOrder()
        {
            _columns = _columns.ToList().OrderBy(c => !c.IsFixedBinaryLength).ThenBy(c => c.Ordinal).ToArray();
        }

        public bool HasColumn(string columnName)
        {
            foreach (var column in Columns)
            {
                if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Overwrites the database id for this schema
        /// </summary>
        /// <param name="dbId">The database id</param>
        /// <remarks>This function exists when creating a new partial database, which
        /// has a different id than the host one. This is likely a design flaw and needs to be changed.</remarks>
        public void ResetDbId(Guid dbId)
        {
            _dbId = dbId;
        }

        public byte[] ToBinaryFormat()
        {
            SortBinaryOrder();

            /*
             * Database Guid
             * Database Name Length
             * Database Name
             * Table Int
             * Table Name Length
             * Table Name
             * Table Schema GUID
             * Table Schema Name Length
             * Table Schema Name
             * Table Total Columns (INT)
             * Table Columns Total Length Binary Length
             * Table Columns
             *      - Column Ordinal (INT)
             *      - Column Name Length
             *      - Column Name
             *      - Column Data Type (see SQLColumnTypeConverter.ConvertToInt())
             *      - Column Max Length (as an INT)
             *      - Column Bool IsNullable
             */

            var arrays = new List<byte[]>();

            var dbGuid = DbBinaryConvert.GuidToBinary(_dbId);
            arrays.Add(dbGuid);

            var dbNameLength = DbBinaryConvert.IntToBinary(_name.Length);
            arrays.Add(dbNameLength);

            var dbName = DbBinaryConvert.StringToBinary(_name);
            arrays.Add(dbName);

            var tableId = DbBinaryConvert.UIntToBinary(_Id);
            arrays.Add(tableId);

            var tableNameLength = DbBinaryConvert.IntToBinary(_name.Length);
            arrays.Add(tableNameLength);

            var tableName = DbBinaryConvert.StringToBinary(_name);
            arrays.Add(tableName);

            var tableSchemaGuid = DbBinaryConvert.GuidToBinary(_schema.SchemaGUID);
            arrays.Add(tableSchemaGuid);

            var tableSchemaNameLength = DbBinaryConvert.IntToBinary(_schema.SchemaName.Length);
            arrays.Add(tableSchemaNameLength);

            var tableSchemaName = DbBinaryConvert.StringToBinary(_schema.SchemaName);
            arrays.Add(tableSchemaName);

            var tableColumnCount = DbBinaryConvert.IntToBinary(_columns.Length);
            arrays.Add(tableColumnCount);

            var columnArrays = new List<byte[]>();
            foreach (var column in Columns)
            {
                columnArrays.Add(ColumnToBinary(column));
            }

            int totalColumnLength = 0;

            foreach (var array in columnArrays)
            {
                totalColumnLength += array.Length;
            }

            var totalColLength = DbBinaryConvert.IntToBinary(totalColumnLength);

            arrays.Add(totalColLength);
            arrays.AddRange(columnArrays);

            byte[] result = DbBinaryConvert.ArrayStitch(arrays);
            return result;
        }
        #endregion

        #region Private Methods
        private byte[] ColumnToBinary(ColumnSchema column)
        {
            /*
             * Table Columns
             *      - Column Ordinal (INT)
             *      - Column Name Length
             *      - Column Name
             *      - Column Data Type (see SQLColumnTypeConverter.ConvertToInt())
             *      - Column Max Length (as an INT)
             *      - Column Bool IsNullable
             */

            var arrays = new List<byte[]>();

            var columnOrdinal = DbBinaryConvert.UIntToBinary(column.Id);
            arrays.Add(columnOrdinal);

            var nameLength = DbBinaryConvert.IntToBinary(column.Name.Length);
            arrays.Add(nameLength);

            var name = DbBinaryConvert.StringToBinary(column.Name);
            arrays.Add(name);

            var dataType = DbBinaryConvert.IntToBinary(SQLColumnTypeConverter.ConvertToInt(column.DataType, Constants.DatabaseVersions.V100));
            arrays.Add(dataType);

            var columnLength = DbBinaryConvert.UIntToBinary(column.Length);
            arrays.Add(columnLength);

            var isNullable = DbBinaryConvert.BooleanToBinary(column.IsNullable);
            arrays.Add(isNullable);

            byte[] result = DbBinaryConvert.ArrayStitch(arrays);
            return result;

        }
        #endregion
    }
}