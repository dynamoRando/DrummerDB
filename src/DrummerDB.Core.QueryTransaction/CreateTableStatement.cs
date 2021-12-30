using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class CreateTableStatement : IStatement, IDDLStatement,

        // antlr specific handles
        IContextTableName,
        IContextId,
        IContextColumnDefinition,
        IContextDataType,
        IContextNullNotNull
    {
        public StatementType Type => StatementType.DDL;
        public bool IsValidated { get; set; }
        public string TableName { get; set; }
        public string FullText { get; set; }
        public List<ColumnSchema> Columns { get; set; }
        public ColumnSchema Current { get; set; }
        public string DatabaseSchemaName { get; set; }

        public CreateTableStatement()
        {
            Columns = new List<ColumnSchema>();
        }

        public CreateTableStatement(string tableName) : this()
        {
            TableName = tableName;
        }

        public uint GetMaxColumnId()
        {
            uint max = 0;
            foreach (var column in Columns)
            {
                if (column.Ordinal > max)
                {
                    max = column.Ordinal;
                }
            }

            return max;
        }

        public bool HasColumn(uint ordinal)
        {
            foreach (var column in Columns)
            {
                if (column.Ordinal == ordinal)
                {
                    return true;
                }
            }

            return false;
        }

        public ColumnSchema GetLastColumn()
        {
            return GetColumnAtOrdinal(GetMaxColumnId());
        }

        public ColumnSchema GetColumnAtOrdinal(uint ordinal)
        {
            foreach (var column in Columns)
            {
                if (column.Ordinal == ordinal)
                {
                    return column;
                }
            }

            return null;
        }

        public void AddColumn(ColumnSchema column)
        {
            Columns.Add(column);
        }

        #region Antlr Wrappers
        public void HandleEnterTableNameOrCreateTable(ContextWrapper context)
        {
            TableName = context.Debug;
            FullText = context.FullText;
        }

        public void HandleEnterId(ContextWrapper context)
        {
            if (Current is not null)
            {
                if (string.IsNullOrEmpty(Current.Name))
                {
                    Current.Name = context.FullText;
                }
            }
            else
            {
                // this is the name of the schema the table should be created in
                if (string.IsNullOrEmpty(DatabaseSchemaName))
                {
                    DatabaseSchemaName = context.FullText;
                }
            }
        }

        public void HandleEnterColumnDefinition(ContextWrapper context)
        {
            uint ordinal = GetMaxColumnId() + 1;
            Current = new ColumnSchema();
            Current.Ordinal = ordinal;
        }

        public void HandleExitColumnDefinition(ContextWrapper context)
        {
            if (!HasColumn(Current.Ordinal))
            {
                AddColumn(Current);
            }
        }

        public void HandleEnterDataType(ContextWrapper context)
        {
            var idText = context.FullText;
            var column = Current;
            if (column is not null)
            {
                if (column.DataType is null)
                {
                    // we need to set the data type here
                    string unparsedDataType = idText;

                    if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.INT))
                    {
                        column.DataType = new SQLInt();
                    }

                    if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.DATETIME))
                    {
                        column.DataType = new SQLDateTime();
                    }

                    if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.BIT))
                    {
                        column.DataType = new SQLBit();
                    }

                    if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.DECIMAL))
                    {
                        column.DataType = new SQLDecimal();
                    }

                    // try to account for VARCHAR containing the word CHAR
                    if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.NVARCHAR) || unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.VARCHAR))
                    {
                        string parsedLength = string.Empty;
                        // need to parse length from the string
                        if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.NVARCHAR))
                        {
                            parsedLength = unparsedDataType.Replace(SQLGeneralKeywords.DataTypes.NVARCHAR, string.Empty);
                        }
                        else
                        {
                            parsedLength = unparsedDataType.Replace(SQLGeneralKeywords.DataTypes.VARCHAR, string.Empty);
                        }
                        
                        parsedLength = parsedLength.Replace("(", string.Empty);
                        parsedLength = parsedLength.Replace(")", string.Empty);

                        uint length = 0;

                        if (uint.TryParse(parsedLength, out length))
                        {
                            column.DataType = new SQLVarChar(length);
                            column.Length = length;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unable to parse column length");
                        }
                    }
                    else if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.CHAR))
                    {
                        // need to parse length from the string
                        var parsedLength = unparsedDataType.Replace(SQLGeneralKeywords.DataTypes.CHAR, string.Empty);
                        parsedLength = parsedLength.Replace("(", string.Empty);
                        parsedLength = parsedLength.Replace(")", string.Empty);

                        uint length = 0;

                        if (uint.TryParse(parsedLength, out length))
                        {
                            column.DataType = new SQLChar(length);
                            column.Length = length;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unable to parse column length");
                        }
                    }

                    if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.VARBINARY))
                    {
                        // need to parse length from the string
                        var parsedLength = unparsedDataType.Replace(SQLGeneralKeywords.DataTypes.VARBINARY, string.Empty);
                        parsedLength = parsedLength.Replace("(", string.Empty);
                        parsedLength = parsedLength.Replace(")", string.Empty);

                        uint length = 0;

                        if (uint.TryParse(parsedLength, out length))
                        {
                            column.DataType = new SQLVarbinary(length);
                            column.Length = length;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unable to parse column length");
                        }
                    }
                    else if (unparsedDataType.Contains(SQLGeneralKeywords.DataTypes.BINARY))
                    {
                        // need to parse length from the string
                        var parsedLength = unparsedDataType.Replace(SQLGeneralKeywords.DataTypes.BINARY, string.Empty);
                        parsedLength = parsedLength.Replace("(", string.Empty);
                        parsedLength = parsedLength.Replace(")", string.Empty);

                        uint length = 0;

                        if (uint.TryParse(parsedLength, out length))
                        {
                            column.DataType = new SQLBinary(length);
                            column.Length = length;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unable to parse column length");
                        }
                    }
                }
            }
        }

        public void HandleEnterNullNotNull(ContextWrapper context)
        {
            var column = Current;
            var nullNotNull = context.FullText;
            if (column is not null)
            {
                if (string.Equals(SQLGeneralKeywords.NOT_NULL, nullNotNull.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    column.IsNullable = false;
                }
                else
                {
                    column.IsNullable = true;
                }
            }
        }

        public void ValidateEnterTableNameOrCreateTable(ContextWrapper context, IDatabase database)
        {
            throw new NotImplementedException();
        }

        public bool TryValidateEnterTableNameOrCreateTable(ContextWrapper context, IDatabase database, out List<string> errors)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
