using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Databases
{
    internal class TableCollection : IEnumerable<Table>
    {
        #region Private Fields
        private List<Table> _tables;
        #endregion

        #region Public Properties
        public int Count => _tables.Count;
        public bool IsReadOnly => false;
        #endregion

        #region Constructors
        public TableCollection()
        {
            _tables = new List<Table>();
        }
        #endregion

        #region Public Methods
        public Table Get(string tableName)
        {
            foreach (var table in _tables)
            {
                if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return table;
                }
            }

            return null;
        }

        public Table Get(string tableName, string schemaName)
        {
            foreach (var table in _tables)
            {
                if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(table.Schema().Schema.SchemaName, schemaName, StringComparison.OrdinalIgnoreCase)
                    )
                {
                    return table;
                }
            }

            return null;
        }

        public Table Get(uint tableId)
        {
            foreach (var table in _tables)
            {
                if (table.Address.TableId == tableId)
                {
                    return table;
                }
            }

            return null;
        }

        public void Add(Table item)
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

        public bool Contains(Table table)
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

        public bool Contains(int tableId)
        {
            foreach (var table in _tables)
            {
                if (table.Address.TableId == tableId)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string tableName, string schemaName)
        {
            foreach (var table in _tables)
            {
                if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase) && string.Equals(table.Schema().Schema.SchemaName, schemaName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(Table item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _tables.Count; i++)
            {
                Table currentTable = _tables[i];

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
                Table currentTable = _tables[i];

                if (string.Equals(currentTable.Name, tableName, StringComparison.OrdinalIgnoreCase))
                {
                    _tables.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public Table this[int index]
        {
            get { return _tables[index]; }
            set { _tables[index] = value; }
        }

        public IEnumerator<Table> GetEnumerator()
        {
            return new TableCollectionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TableCollectionEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
