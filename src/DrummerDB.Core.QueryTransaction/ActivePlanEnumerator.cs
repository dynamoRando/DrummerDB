using System;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class ActivePlanEnumerator : IEnumerator<ActivePlan>
    {
        private ActivePlanCollection _plans;
        private int _index;
        private ActivePlan _current;

        public ActivePlan Current => _current;
        object IEnumerator.Current => Current;

        public ActivePlanEnumerator(ActivePlanCollection collection)
        {
            _plans = collection;
            _index = -1;
            _current = default(ActivePlan);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _plans.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _plans[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public IEnumerator<ActivePlan> GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }
}
