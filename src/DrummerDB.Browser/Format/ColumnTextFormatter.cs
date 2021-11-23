using Drummersoft.DrummerDB.Common.Communication;

namespace Drummersoft.DrummerDB.Browser.Format
{
    public readonly record struct ColumnTextFormatter(ColumnSchema Column, int MaxLength);
}
