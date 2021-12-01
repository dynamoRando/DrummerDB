namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface IContextColumnDefinition
    {
        void HandleEnterColumnDefinition(ContextWrapper context);
        void HandleExitColumnDefinition(ContextWrapper context);
    }
}
