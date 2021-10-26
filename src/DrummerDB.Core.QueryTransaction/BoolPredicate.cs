﻿using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Represents a bool operation on a WHERE clause between two predicates, i.e if a WHERE clause has FOO = A OR BAR = 2, this would hold the predicates and the bool operation
    /// </summary>
    class BoolPredicate : IPredicate
    {
        private int _id;
        public BooleanComparisonOperator ComparisonOperator { get; set; }
        public IPredicate Left { get; set; }
        public IPredicate Right { get; set; }
        public int Id => _id;
        public Interval Interval => GetInterval();

        public BoolPredicate(int id)
        {
            _id = id;
        }

        private Interval GetInterval()
        {
            var interval = new Interval { A = 0, B = 0 };

            if (Left is not null)
            {
                interval.A = Left.Interval.A;
            }

            if (Right is not null)
            {
                interval.B = Right.Interval.B;
            }

            return interval;
        }
    }
}
