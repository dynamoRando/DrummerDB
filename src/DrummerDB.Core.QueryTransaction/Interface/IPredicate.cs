namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// Represents part of a WHERE clause, either a <see cref="Predicate"/> or a <see cref="BoolPredicate"/>.
    /// </summary>
    interface IPredicate
    {
        uint Id { get; }
        Interval Interval { get; }
    }
}
