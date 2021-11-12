using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    internal partial class SystemSchemaConstants100
    {
        /// <summary>
        /// Contains the definitions for all system data tables
        /// </summary>
        internal static class Tables
        {
            /// <summary>
            /// A table holding all the user defined tables in the database
            /// </summary>
            public static class UserTable
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.USER_TABLES;
                public const string TABLE_NAME = "UserTables";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string TableId = "TableId";
                    public const string TableName = "TableName";
                    public const string TotalRows = "TotalRows";
                    public const string TotalLogicalRows = "TotalLogicalRows";
                    public const string IsDeleted = "IsDeleted";
                    public const string UserObjectId = "UserObjectId";
                    public const string SchemaGUID = "SchemaGUID";
                    public const string ContractGUID = "ContractGUID";
                    public const string LogicalStoragePolicy = "LogicalStoragePolicy";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(9);

                    var tableId = new ColumnSchema(Columns.TableId, new SQLInt(), 1);
                    result.Add(tableId);

                    var tableName = new ColumnSchema(Columns.TableName, new SQLChar(Constants.FIXED_LENGTH_OF_OBJECT_NAME), 2);
                    result.Add(tableName);

                    var totalRows = new ColumnSchema(Columns.TotalRows, new SQLInt(), 3);
                    result.Add(totalRows);

                    var totalLogicalRows = new ColumnSchema(Columns.TotalLogicalRows, new SQLInt(), 4);
                    result.Add(totalLogicalRows);

                    var isDeleted = new ColumnSchema(Columns.IsDeleted, new SQLBit(), 5);
                    result.Add(isDeleted);

                    var columnObjectId = new ColumnSchema(Columns.UserObjectId, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 6);
                    result.Add(columnObjectId);

                    var schemaGuid = new ColumnSchema(Columns.SchemaGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 7, true);
                    result.Add(schemaGuid);

                    var contractGuid = new ColumnSchema(Columns.ContractGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 8, true);
                    result.Add(contractGuid);

                    var storagePolicy = new ColumnSchema(Columns.LogicalStoragePolicy, new SQLInt(), 9, true);
                    result.Add(storagePolicy);

                    return result;
                }
            }

            /// <summary>
            /// A table holding the schema information (column name, data type, etc.) for all the user defined tables in the database
            /// </summary>
            public static class UserTableSchema
            {

                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.USER_TABLE_SCHEMAS;
                public const string TABLE_NAME = "UserTableSchemas";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    /// <summary>
                    /// The TableId the column is attached to
                    /// </summary>
                    public const string TableId = "TableId";

                    /// <summary>
                    /// An integer identifier of the column (used for address purposes, TableId (int), ColumnId (int)) etc.
                    /// </summary>
                    public const string ColumnId = "ColumnId";

                    /// <summary>
                    /// The name of the column
                    /// </summary>
                    public const string ColumnName = "ColumnName";

                    /// <summary>
                    /// The data type of the column
                    /// </summary>
                    /// <remarks>See <see cref="ColumnTypes"/> for ENUM information</remarks>
                    public const string ColumnType = "ColumnType";

                    /// <summary>
                    /// The max or fixed length of the column, if applicable
                    /// </summary>
                    public const string ColumnLength = "ColumnLength";

                    /// <summary>
                    /// The ordinal value of the column 
                    /// </summary>
                    public const string ColumnOrdinal = "ColumnOrdinal";

                    /// <summary>
                    /// If the column is a NULLABLE field
                    /// </summary>
                    public const string ColumnIsNullable = "ColumnIsNullable";

                    /// <summary>
                    /// The binary sort order of the column on disk
                    /// </summary>
                    public const string ColumnBinaryOrder = "ColumnBinaryOrder";

                    /// <summary>
                    /// A unique GUID representing the column. Used for security purposes, etc.
                    /// </summary>
                    public const string UserObjectId = "UserObjectId";

                    /// <summary>
                    /// The contract version currently applied to the table
                    /// </summary>
                    public const string ContractGUID = "ContractGUID";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(10);

                    var tableId = new ColumnSchema(Columns.TableId, new SQLInt(), 1);
                    result.Add(tableId);

                    var columnId = new ColumnSchema(Columns.ColumnId, new SQLInt(), 2);
                    result.Add(columnId);

                    var columnName = new ColumnSchema(Columns.ColumnName, new SQLChar(Constants.FIXED_LENGTH_OF_OBJECT_NAME), 3);
                    result.Add(columnName);

                    // Use the ColumnTypes enum when saving off
                    var columnType = new ColumnSchema(Columns.ColumnType, new SQLInt(), 4);
                    result.Add(columnType);

                    var columnLength = new ColumnSchema(Columns.ColumnLength, new SQLInt(), 5);
                    result.Add(columnLength);

                    var columnOrdinal = new ColumnSchema(Columns.ColumnOrdinal, new SQLInt(), 6);
                    result.Add(columnOrdinal);

                    var columnIsNullable = new ColumnSchema(Columns.ColumnIsNullable, new SQLBit(), 7);
                    result.Add(columnIsNullable);

                    var columnBinaryOrder = new ColumnSchema(Columns.ColumnBinaryOrder, new SQLInt(), 8);
                    result.Add(columnBinaryOrder);

                    var columnUserObjectId = new ColumnSchema(Columns.UserObjectId, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 9);
                    result.Add(columnUserObjectId);

                    var contractGuid = new ColumnSchema(Columns.ContractGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 10, true);
                    result.Add(contractGuid);

                    return result;
                }
            }

            /// <summary>
            /// A table holding all the object ids for every user defined object in the database
            /// </summary>
            public static class UserObjects
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.USER_OBJECTS;
                public const string TABLE_NAME = "UserObjects";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string ObjectId = "ObjectId";
                    public const string ObjectType = "ObjectType";
                    public const string ObjectName = "ObjectName";
                    public const string ContractGUID = "ContractGUID";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(4);

                    var objectId = new ColumnSchema(Columns.ObjectId, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 1);
                    result.Add(objectId);

                    var objectType = new ColumnSchema(Columns.ObjectType, new SQLInt(), 2);
                    result.Add(objectType);

                    var objectName = new ColumnSchema(Columns.ObjectName, new SQLChar(Constants.FIXED_LENGTH_OF_OBJECT_NAME), 3);
                    result.Add(objectName);

                    var contractGuid = new ColumnSchema(Columns.ContractGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 4, true);
                    result.Add(contractGuid);

                    return result;
                }
            }

            public static class Users
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.USERS;
                public const string TABLE_NAME = "Users";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string UserName = "UserName";
                    public const string UserGUID = "UserGUID";
                    public const string ByteLength = "ByteLength";
                    public const string Salt = "Salt";
                    public const string Hash = "Hash";
                    public const string Workfactor = "Workfactor";
                    public const string IsBanned = "IsBanned";
                    public const string Token = "Token";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    //var result = new List<ColumnSchema>(8);
                    var result = new List<ColumnSchema>(7);

                    var userName = new ColumnSchema(Columns.UserName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(userName);

                    var userGuid = new ColumnSchema(Columns.UserGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 2);
                    result.Add(userGuid);

                    var byteLength = new ColumnSchema(Columns.ByteLength, new SQLInt(), 3);
                    result.Add(byteLength);

                    var salt = new ColumnSchema(Columns.Salt, new SQLVarbinary(Constants.MAX_LENGTH_OF_SECURITY_ARRAY), 4);
                    result.Add(salt);

                    var hash = new ColumnSchema(Columns.Hash, new SQLVarbinary(Constants.MAX_LENGTH_OF_SECURITY_ARRAY), 5);
                    result.Add(hash);

                    var work = new ColumnSchema(Columns.Workfactor, new SQLInt(), 6);
                    result.Add(work);

                    var isBanned = new ColumnSchema(Columns.IsBanned, new SQLBit(), 7);
                    result.Add(isBanned);

                    //var token = new ColumnSchema(Columns.Token, new SQLVarbinary(Constants.MAX_LENGTH_OF_SECURITY_ARRAY), 8);
                    //result.Add(token);

                    return result;
                }
            }

            public static class UserObjectPermissions
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.USER_OBJECT_PERMISSIONS;
                public const string TABLE_NAME = "UserObjectPermissions";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string UserName = "UserName";
                    public const string UserGUID = "UserGUID";
                    public const string ObjectId = "ObjectId";
                    public const string DbPermission = "DbPermission";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(4);

                    var userName = new ColumnSchema(Columns.UserName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(userName);

                    var userGuid = new ColumnSchema(Columns.UserGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 2);
                    result.Add(userGuid);

                    var objectId = new ColumnSchema(Columns.ObjectId, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 3);
                    result.Add(objectId);

                    var objectType = new ColumnSchema(Columns.DbPermission, new SQLInt(), 4);
                    result.Add(objectType);

                    return result;
                }
            }

            // contains the list of schemas held in this database, i.e. "sys", "dbo", and any other user created schemas
            public static class DatabaseSchemas
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.DATABASE_SCHEMAS;
                public const string TABLE_NAME = "DatabaseSchemas";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string SchemaGUID = "SchemaGUID";
                    public const string SchemaName = "SchemaName";
                    public const string ContractGUID = "ContractGUID";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(3);

                    var schemaGUID = new ColumnSchema(Columns.SchemaGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 1);
                    result.Add(schemaGUID);

                    var schemaName = new ColumnSchema(Columns.SchemaName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 2);
                    result.Add(schemaName);

                    var contractGuid = new ColumnSchema(Columns.ContractGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 3, true);
                    result.Add(contractGuid);

                    return result;
                }
            }

            public static class DatabaseSchemaPermissions
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.DATABASE_SCHEMA_PERMISSIONS;
                public const string TABLE_NAME = "DatabaseSchemaPermissions";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string UserName = "UserName";
                    public const string UserGUID = "UserGUID";
                    public const string SchemaGUID = "SchemaGUID";
                    public const string DbPermission = "DbPermission";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(4);

                    var userName = new ColumnSchema(Columns.UserName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(userName);

                    var userGuid = new ColumnSchema(Columns.UserGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 2);
                    result.Add(userGuid);

                    var objectId = new ColumnSchema(Columns.SchemaGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 3);
                    result.Add(objectId);

                    var objectType = new ColumnSchema(Columns.DbPermission, new SQLInt(), 4);
                    result.Add(objectType);

                    return result;
                }
            }

            public static class Participants
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.PARTICIPANTS;
                public const string TABLE_NAME = "Participants";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string ParticpantGUID = "ParticpantGUID";
                    public const string Alias = "Alias";
                    public const string IP4Address = "IP4Address";
                    public const string IP6Address = "IP6Address";
                    public const string PortNumber = "PortNumber";
                    public const string LastCommunicationUTC = "LastCommunicationUTC";
                    public const string HasAcceptedContract = "HasAcceptedContract";
                    public const string AcceptedContractVersion = "AcceptedContractVersion";
                    public const string AcceptedContractDateTimeUTC = "AcceptedContractDateTimeUTC";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(9);

                    var participantId = new ColumnSchema(Columns.ParticpantGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 1);
                    result.Add(participantId);

                    var alias = new ColumnSchema(Columns.Alias, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 2);
                    result.Add(alias);

                    var ip4 = new ColumnSchema(Columns.IP4Address, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 3);
                    result.Add(ip4);

                    var ip6 = new ColumnSchema(Columns.IP6Address, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 4);
                    result.Add(ip6);

                    var port = new ColumnSchema(Columns.PortNumber, new SQLInt(), 5);
                    result.Add(port);

                    var lastComm = new ColumnSchema(Columns.LastCommunicationUTC, new SQLDateTime(), 6, true);
                    result.Add(lastComm);

                    var acceptedContract = new ColumnSchema(Columns.HasAcceptedContract, new SQLBit(), 7, true);
                    result.Add(acceptedContract);

                    var acceptedContractVersion = new ColumnSchema(Columns.AcceptedContractVersion, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 8, true);
                    result.Add(acceptedContractVersion);

                    var acceptedContractTime = new ColumnSchema(Columns.AcceptedContractDateTimeUTC, new SQLDateTime(), 9, true);
                    result.Add(acceptedContractTime);

                    return result;
                }
            }

            public static class Tenants
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.TENANTS;
                public const string TABLE_NAME = "Tenants";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string TenantGUID = "TenantGUID";
                    public const string Alias = "Alias";

                    // see TenantDataLocation.cs
                    public const string DataLocation = "DataLocation";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(3);

                    var participantId = new ColumnSchema(Columns.TenantGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 1);
                    result.Add(participantId);

                    var alias = new ColumnSchema(Columns.Alias, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 2);
                    result.Add(alias);

                    var dataLocation = new ColumnSchema(Columns.DataLocation, new SQLInt(), 3);
                    result.Add(dataLocation);

                    return result;
                }
            }

            public static class DatabaseContracts
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.DATABASE_CONTRACTS;
                public const string TABLE_NAME = "DatabaseContracts";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string ContractGUID = "ContractGUID";
                    public const string GeneratedDate = "GeneratedDate";
                    public const string Author = "Author";
                    public const string Token = "Token";
                    public const string Description = "Description";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(5);

                    var participantId = new ColumnSchema(Columns.ContractGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 1);
                    result.Add(participantId);

                    var alias = new ColumnSchema(Columns.GeneratedDate, new SQLDateTime(), 2);
                    result.Add(alias);

                    var author = new ColumnSchema(Columns.Author, new SQLVarChar(50), 3);
                    result.Add(author);

                    var token = new ColumnSchema(Columns.Token, new SQLVarbinary(128), 4, true);
                    result.Add(token);

                    var description = new ColumnSchema(Columns.Description, new SQLVarChar(128), 5, true);
                    result.Add(description);

                    return result;
                }
            }
        }

        internal partial class Maps
        {
            /// <summary>
            /// Represents a page item in a Page Map
            /// </summary>
            public record PageItem
            {
                public int Order;
                public int PageId;
                public int TableId;
                public PageType Type;
                public DataPageType DataPageType;
                public int Offset;
                public bool IsDeleted;

                public PageItem(int pageId, PageType type, DataPageType dataPageType, int order, int tableId, int offset, bool isDelted)
                {
                    PageId = pageId;
                    Type = type;
                    Order = order;
                    DataPageType = dataPageType;
                    TableId = tableId;
                    Offset = offset;
                    IsDeleted = isDelted;
                }
            }
        }
    }
}
