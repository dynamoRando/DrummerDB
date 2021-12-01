using Drummersoft.DrummerDB.Core.Structures;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal static class RowValueMaker
    {
        internal static RowValue Create(Table table, string columnNameOfTable, string value, bool padIfNeeded = false)
        {
            var result = new RowValue();
            result.SetColumn(table.GetColumn(columnNameOfTable));
            result.SetValue(value, padIfNeeded);
            return result;
        }
    }
}
