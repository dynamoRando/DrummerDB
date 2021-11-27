
using System;
using System.Collections;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class ColumnSchemaCollection : IEnumerable<ColumnSchema>
    {
        #region Private Fields
        private List<ColumnSchema> _ColumnSchemas;
        #endregion

        #region Public Properties
        public int Count => _ColumnSchemas.Count;
        public bool IsReadOnly => false;
        public List<ColumnSchema> List =>  _ColumnSchemas;
        #endregion

        #region Constructors
        public ColumnSchemaCollection()
        {
            _ColumnSchemas = new List<ColumnSchema>();
        }

        public ColumnSchemaCollection(int size)
        {
            _ColumnSchemas = new List<ColumnSchema>(size);
        }
        #endregion

        #region Public Methods
        public ColumnSchema Get(string name)
        {
            foreach(var ColumnSchema in _ColumnSchemas)
            {
                if (string.Equals(ColumnSchema.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return ColumnSchema;
                }
            }

            return null;
        }

        public void Add(ColumnSchema item)
        {
            if (!Contains(item.Name))
            {
                _ColumnSchemas.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a ColumnSchema named {item.Name}");
            }
        }

        public bool Contains(ColumnSchema ColumnSchema)
        {
            foreach (var x in _ColumnSchemas)
            {
                if (string.Equals(x.Name, ColumnSchema.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string ColumnSchemaName)
        {
            foreach (var ColumnSchema in _ColumnSchemas)
            {
                if (string.Equals(ColumnSchema.Name, ColumnSchemaName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(ColumnSchema item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _ColumnSchemas.Count; i++)
            {
                ColumnSchema currentColumnSchema = _ColumnSchemas[i];

                if (string.Equals(currentColumnSchema.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _ColumnSchemas.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(string ColumnSchemaName)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _ColumnSchemas.Count; i++)
            {
                ColumnSchema currentColumnSchema = _ColumnSchemas[i];

                if (string.Equals(currentColumnSchema.Name, ColumnSchemaName, StringComparison.OrdinalIgnoreCase))
                {
                    _ColumnSchemas.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public ColumnSchema this[int index]
        {
            get { return _ColumnSchemas[index]; }
            set { _ColumnSchemas[index] = value; }
        }

        public IEnumerator<ColumnSchema> GetEnumerator()
        {
            return new ColumnSchemaEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ColumnSchemaEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
