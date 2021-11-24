using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Common.Communication.SQLService;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Drummersoft.DrummerDB.Common.Communication;

namespace Drummersoft.DrummerDB.Browser.Format
{
    public static class ResultTableFormatter
    {
        #region Private Fields
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public static string ToTextTable(SQLQueryReply reply)
        {
            var builder = new StringBuilder();
            if (reply.Results.Count > 0)
            {
                SQLResultset result = reply.Results.First();
                int totalColumns = GetNumberOfColumns(result);
                if (totalColumns > 0)
                {
                    List<ColumnSchema> columns = GetColumns(result);
                    List<ColumnTextFormatter> formattedColumns = GetColumnTextFormatters(columns, result);
                    string formatter = GetFormatterString(formattedColumns);
                    string header = BuildHeader(formattedColumns, formatter);
                    List<string> rows = GetRowsText(result, formatter);
                    builder.AppendLine(header);
                    foreach (var row in rows)
                    {
                        builder.AppendLine(row);
                    }
                }
            }

            return builder.ToString();
        }
        #endregion

        #region Private Methods
        public static List<string> GetRowsText(SQLResultset result, string formatter)
        {
            var rows = new List<string>();

            foreach (var row in result.Rows)
            {
                var rowItems = new List<string>(row.Values.Count);
                foreach (var value in row.Values)
                {
                    rowItems.Add(GetValueInString(value.Column, value));
                }
                string rowText = string.Format(formatter, rowItems.ToArray()); ;
                rows.Add(rowText);
            }

            return rows;
        }

        private static string GetValueInString(ColumnSchema column, RowValue value)
        {
            SQLColumnType type = (SQLColumnType)column.ColumnType;
            byte[] binaryValue = value.Value.ToByteArray();

            if (!column.IsNullable)
            {
                switch (type)
                {
                    case SQLColumnType.Int:
                        return DbBinaryConvert.BinaryToInt(binaryValue).ToString();
                    case SQLColumnType.Bit:
                        return DbBinaryConvert.BinaryToBoolean(binaryValue).ToString();
                    case SQLColumnType.Char:
                        return DbBinaryConvert.BinaryToString(binaryValue);
                    case SQLColumnType.DateTime:
                        return DbBinaryConvert.BinaryToDateTime(binaryValue).ToString();
                    case SQLColumnType.Decimal:
                        return DbBinaryConvert.BinaryToDecimal(binaryValue).ToString();
                    case SQLColumnType.Varchar:
                        return DbBinaryConvert.BinaryToString(binaryValue).ToString();
                    case SQLColumnType.Binary:
                        throw new InvalidOperationException("How do I convert binary to text?");
                    case SQLColumnType.Varbinary:
                        throw new InvalidOperationException("How do I convert binary to text?");
                    default:
                        throw new InvalidOperationException("Unknown data type to convert to string");
                }
            }

            if (column.IsNullable && !value.IsNullValue)
            {
                var span = new ReadOnlySpan<byte>(binaryValue);
                var revisedSpan = span.Slice(1, span.Length - 1);
                switch (type)
                {
                    case SQLColumnType.Int:
                        return DbBinaryConvert.BinaryToInt(revisedSpan.ToArray()).ToString();
                    case SQLColumnType.Bit:
                        return DbBinaryConvert.BinaryToBoolean(revisedSpan).ToString();
                    case SQLColumnType.Char:
                        return DbBinaryConvert.BinaryToString(revisedSpan);
                    case SQLColumnType.DateTime:
                        return DbBinaryConvert.BinaryToDateTime(revisedSpan).ToString();
                    case SQLColumnType.Decimal:
                        return DbBinaryConvert.BinaryToDecimal(revisedSpan).ToString();
                    case SQLColumnType.Varchar:
                        return DbBinaryConvert.BinaryToString(revisedSpan).ToString();
                    case SQLColumnType.Binary:
                        throw new InvalidOperationException("How do I convert binary to text?");
                    case SQLColumnType.Varbinary:
                        throw new InvalidOperationException("How do I convert binary to text?");
                    default:
                        throw new InvalidOperationException("Unknown data type to convert to string");
                }
            }

            if (column.IsNullable && value.IsNullValue)
            {
                return "NULL";
            }

            return string.Empty;
        }

        private static string GetFormatterString(List<ColumnTextFormatter> columns)
        {

            int i = 0;
            string parameterString = string.Empty;

            foreach (var column in columns)
            {
                parameterString += $"{{{i.ToString()},-{column.MaxLength.ToString()}}}";
                i++;
            }

            return parameterString;
        }

        private static string BuildHeader(List<ColumnTextFormatter> columns, string formatter)
        {
            int totalColumns = columns.Count;
            var columnNames = new List<string>(totalColumns);

            foreach (var column in columns)
            {
                columnNames.Add(column.Column.ColumnName);
            }
            var result = string.Format(formatter, columnNames.ToArray());

            return result;
        }

        private static List<ColumnTextFormatter> GetColumnTextFormatters(List<ColumnSchema> columns, SQLResultset result)
        {
            var formattedColumns = new List<ColumnTextFormatter>();
            foreach (var column in columns)
            {
                formattedColumns.Add(new ColumnTextFormatter(column, GetMaxCharLengthValueFromColumn(column, result)));
            }

            return formattedColumns;
        }

        private static int GetNumberOfColumns(SQLResultset result)
        {
            int columns = 0;
            var firstRow = result.Rows.FirstOrDefault();
            if (firstRow is not null)
            {
                foreach (var value in firstRow.Values)
                {
                    columns++;
                }
            }

            return columns;
        }

        private static List<ColumnSchema> GetColumns(SQLResultset result)
        {
            var columns = new List<ColumnSchema>();
            var firstRow = result.Rows.First();
            if (firstRow is not null)
            {
                foreach (var value in firstRow.Values)
                {
                    columns.Add(value.Column);
                }
            }

            return columns;
        }

        private static int GetMaxCharLengthValueFromColumn(ColumnSchema column, SQLResultset result)
        {
            int maxLength = 0;
            int tempLength = 0;
            var specifiedColumntype = (SQLColumnType)column.ColumnType;
            int columnNameLength = 0;

            var firstRow = result.Rows.First();
            if (firstRow is not null)
            {
                foreach (var value in firstRow.Values)
                {
                    if (string.Equals(value.Column.ColumnName, column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        columnNameLength = column.ColumnName.Length;
                        var columnType = (SQLColumnType)value.Column.ColumnType;
                        switch (columnType)
                        {
                            case SQLColumnType.Int:
                                // max value is 2147483647
                                maxLength = 10;
                                break;
                            case SQLColumnType.Bit:
                                // values are character string 'True' or 'False'
                                maxLength = 5;
                                break;
                            case SQLColumnType.Char:
                                maxLength = (int)column.ColumnLength;
                                break;
                            case SQLColumnType.DateTime:
                                maxLength = DateTime.Now.ToString().Length;
                                break;
                            case SQLColumnType.Decimal:
                                // 1.79769313486232e308
                                maxLength = 20;
                                break;
                            case SQLColumnType.Varchar:
                                string stringValue = DbBinaryConvert.BinaryToString(value.Value.ToByteArray());
                                if (stringValue.Length > tempLength)
                                {
                                    tempLength = stringValue.Length;
                                }
                                break;
                            case SQLColumnType.Binary:
                                maxLength = (int)column.ColumnLength;
                                break;
                            case SQLColumnType.Varbinary:
                                var binaryValue = value.Value.ToByteArray();
                                if (binaryValue.Length > tempLength)
                                {
                                    tempLength = binaryValue.Length;
                                }
                                break;
                            default:
                                maxLength = 0;
                                break;
                        }
                    }
                }
            }

            // if the column was a variable length type, assign the max found length to maxlength
            if (specifiedColumntype == SQLColumnType.Varchar || specifiedColumntype == SQLColumnType.Varbinary)
            {
                maxLength = tempLength;
            }

            if (columnNameLength > maxLength)
            {
                maxLength = columnNameLength;
            }

            if (specifiedColumntype == SQLColumnType.Varchar || specifiedColumntype == SQLColumnType.Varbinary)
            {
                if (columnNameLength > tempLength)
                {
                    maxLength = columnNameLength;
                }
            }

            maxLength++;
            return maxLength;
        }
        #endregion
    }
}
