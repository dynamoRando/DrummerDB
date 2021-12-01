using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class QueryExpressionEnumerator : IEnumerator<QueryExpression>
    {
        private QueryExpressionCollection _expressions;
        private int _index;
        private QueryExpression _current;

        public QueryExpression Current => _current;

        object IEnumerator.Current => Current;

        public QueryExpressionEnumerator(QueryExpressionCollection collection)
        {
            _expressions = collection;
            _index = -1;
            _current = default(QueryExpression);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _expressions.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _expressions[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
