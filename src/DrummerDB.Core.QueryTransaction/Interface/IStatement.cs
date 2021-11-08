using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    interface IStatement
    {
        StatementType Type { get; }
        bool IsValidated { get; set; }
    }
}
