using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Represents a WHERE clause
    /// </summary>
    class WhereClause
    {
        /// <summary>
        /// The full text of the the WHERE clause
        /// </summary>
        public string FullText { get; set; }
        public PredicateCollection Parts { get; set; }

        public WhereClause()
        {
            FullText = string.Empty;
            Parts = new PredicateCollection();
        }

        public IPredicate GetMaxPredicate()
        {
            uint maxId = 0;
            foreach (var part in Parts)
            {
                if (part.Id > maxId)
                {
                    maxId = part.Id;
                }
            }

            foreach (var part in Parts)
            {
                if (part.Id == maxId)
                {
                    return part;
                }
            }

            return null;
        }

        public uint GetMaxWhereClauseId()
        {
            uint maxId = 0;

            foreach (var part in Parts)
            {
                if (part.Id > maxId)
                {
                    maxId = part.Id;
                }
            }


            return maxId;
        }

        public IPredicate GetPredicate(Interval interval)
        {
            return Parts.Get(interval);
        }

        /// <summary>
        /// Searches the WHERE clause for any predicate (bool or regular) that matches the specified interval. Note that this will search
        /// <see cref="BoolPredicate"/> for both their left, right, and entire interval to see if there is a match.
        /// </summary>
        /// <param name="interval">The internval to search for </param>
        /// <returns><c>TRUE</c> if the interval is found, otherwise <c>FALSE</c></returns>
        public bool HasPredicate(Interval interval)
        {
            foreach (var part in Parts)
            {
                if (part is Predicate)
                {
                    if (part.Interval == interval)
                    {
                        return true;
                    }
                }

                if (part is BoolPredicate)
                {
                    var boo = part as BoolPredicate;

                    if (boo.Left is Predicate)
                    {
                        var booLeft = boo.Left as Predicate;
                        if (booLeft.Interval == interval)
                        {
                            return true;
                        }
                    }

                    if (boo.Right is Predicate)
                    {
                        var booRight = boo.Right as Predicate;
                        if (booRight.Interval == interval)
                        {
                            return true;
                        }
                    }

                    if (boo.Interval == interval)
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        public void Add(IPredicate predicate)
        {
            Parts.Add(predicate);
        }

        /// <summary>
        /// Searches the WHERE clause for an empty BOOL operation that contains the specified predicate
        /// </summary>
        /// <param name="predicate">The predicate to search for</param>
        /// <returns><c>TRUE</c> if there is an open ended BOOL predicate in the WHERE clause, otherwise <c>FALSE</c></returns>
        public bool HasBooleanPredicate(Predicate predicate)
        {
            foreach (var item in Parts)
            {
                if (item is BoolPredicate)
                {
                    var foo = item as BoolPredicate;

                    // if we have a boolean that is already set, skip over this one
                    if (foo.Left is not null && foo.Right is not null)
                    {
                        if (foo.Left is Predicate && foo.Right is Predicate)
                        {
                            continue;
                        }
                    }

                    if (foo.Left is Predicate)
                    {
                        if (foo.Left.Interval.A == predicate.Interval.A && foo.Left.Interval.B == predicate.Interval.B)
                        {
                            return true;
                        }
                    }

                    if (foo.Right is Predicate)
                    {
                        if (foo.Right.Interval.A == predicate.Interval.A && foo.Right.Interval.B == predicate.Interval.B)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public BoolPredicate GetBooleanPredicate(Predicate predicate)
        {
            foreach (var item in Parts)
            {
                if (item is BoolPredicate)
                {
                    var foo = item as BoolPredicate;

                    if (foo.Left is Predicate)
                    {
                        if (foo.Left.Interval.A == predicate.Interval.A && foo.Left.Interval.B == predicate.Interval.B)
                        {
                            return foo;
                        }
                    }

                    if (foo.Right is Predicate)
                    {
                        if (foo.Right.Interval.A == predicate.Interval.A && foo.Right.Interval.B == predicate.Interval.B)
                        {
                            return foo;
                        }
                    }
                }
            }

            return null;
        }
    }
}
