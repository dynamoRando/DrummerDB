using System;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class ActivePlanCollection : IEnumerable<ActivePlan>
    {
        #region Private Fields
        private List<ActivePlan> _activePlans;
        #endregion

        #region Public Properties
        public int Count => _activePlans.Count;
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public ActivePlanCollection()
        {
            _activePlans = new List<ActivePlan>();
        }

        public ActivePlanCollection(int length)
        {
            _activePlans = new List<ActivePlan>(length);
        }
        #endregion

        #region Public Methods
        public ActivePlan this[int index]
        {
            get { return _activePlans[index]; }
            set { _activePlans[index] = value; }
        }

        public ActivePlan GetActivePlan(Guid planId)
        {
            foreach (var plan in _activePlans)
            {
                if (plan.PlanId == planId)
                {
                    return plan;
                }
            }

            return null;
        }

        public void Add(ActivePlan item)
        {
            if (!Contains(item))
            {
                _activePlans.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a plan with id {item.PlanId.ToString()}");
            }
        }

        public void Clear()
        {
            _activePlans.Clear();
        }

        public bool Contains(ActivePlan item)
        {
            foreach (var plan in _activePlans)
            {
                if (plan.PlanId == item.PlanId)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(Guid planId)
        {
            foreach (var plan in _activePlans)
            {
                if (plan.PlanId == planId)
                {
                    return true;
                }
            }
            return false;
        }


        public void CopyTo(ActivePlan[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _activePlans.Count; i++)
            {
                array[i + arrayIndex] = _activePlans[i];
            }
        }

        public IEnumerator<ActivePlan> GetEnumerator()
        {
            return new ActivePlanEnumerator(this);
        }

        public bool Remove(ActivePlan item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _activePlans.Count; i++)
            {
                ActivePlan curPlan = _activePlans[i];

                if (curPlan.PlanId == item.PlanId)
                {
                    _activePlans.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ActivePlanEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
