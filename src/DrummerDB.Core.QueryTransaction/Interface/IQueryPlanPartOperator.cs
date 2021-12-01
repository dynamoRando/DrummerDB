namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    interface IQueryPlanPartOperator
    {
        IQueryPlanPartOperator PreviousOperation { get; set; }
        IQueryPlanPartOperator NextOperation { get; set; }
    }
}
