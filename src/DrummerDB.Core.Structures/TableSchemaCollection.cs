using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class TableSchemaCollection : IEnumerable<TableSchema>
    {
        #region Private Fields
        private List<TableSchema> _tables;
        #endregion

        #region Public Properties
        public int Count => _tables.Count;
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public TableSchemaCollection()
        {
            _tables = new List<TableSchema>();
        }

        public TableSchemaCollection(int count)
        {
            _tables = new List<TableSchema>(count);
        }
        #endregion

        #region Public Methods
        public void Add(TableSchema item)
        {
            if (!Contains(item.Name))
            {
                _tables.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a table named {item.Name}");
            }
        }

        public bool Contains(TableSchema table)
        {
            foreach (var x in _tables)
            {
                if (string.Equals(x.Name, table.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(TableSchema item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _tables.Count; i++)
            {
                TableSchema currentTable = _tables[i];

                if (string.Equals(currentTable.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _tables.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(string tableName)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _tables.Count; i++)
            {
                TableSchema currentTable = _tables[i];

                if (string.Equals(currentTable.Name, tableName, StringComparison.OrdinalIgnoreCase))
                {
                    _tables.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Contains(string tableName)
        {
            foreach (var table in _tables)
            {
                if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<TableSchema> GetEnumerator()
        {
            return new TableSchemaCollectionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TableSchemaCollectionEnumerator(this);
        }

        public TableSchema this[int index]
        {
            get { return _tables[index]; }
            set { _tables[index] = value; }
        }
        #endregion

        #region Private Methods
        #endregion



    }
}
