using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class QueryExpressionCollection : ICollection<QueryExpression>
    {
        #region Private Fields
        private List<QueryExpression> _expressions;
        #endregion

        #region Public Properties
        public int Count => _expressions.Count();
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public QueryExpressionCollection()
        {
            _expressions = new List<QueryExpression>();
        }

        public QueryExpressionCollection(int length)
        {
            _expressions = new List<QueryExpression>(length);
        }
        #endregion

        #region Public Methods
        public QueryExpression this[int index]
        {
            get { return _expressions[index]; }
            set { _expressions[index] = value; }
        }

        public QueryExpression GetQueryExpression(int id)
        {
            foreach (var expression in _expressions)
            {
                if (id == expression.Id)
                {
                    return expression;
                }
            }

            return new QueryExpression();
        }

        public void Add(QueryExpression item)
        {
            if (!Contains(item))
            {
                _expressions.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                    $"There is already an expression with id {item.Id.ToString()}");
            }
        }

        public void Clear()
        {
            _expressions.Clear();
        }

        public bool Contains(QueryExpression item)
        {
            foreach (var expresion in _expressions)
            {
                if (expresion.Id == item.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(QueryExpression[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _expressions.Count; i++)
            {
                array[i + arrayIndex] = _expressions[i];
            }
        }

        public IEnumerator<QueryExpression> GetEnumerator()
        {
            return new QueryExpressionEnumerator(this);
        }

        public bool Remove(QueryExpression item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _expressions.Count; i++)
            {
                QueryExpression curEx = _expressions[i];

                if (item.Id == curEx.Id)
                {
                    _expressions.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new QueryExpressionEnumerator(this);
        }


        #endregion

        #region Private Methods
        #endregion
    }
}
