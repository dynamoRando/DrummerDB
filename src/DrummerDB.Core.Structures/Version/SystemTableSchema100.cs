using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    internal class SystemTableSchema100 : SystemTableSchema
    {
        #region Private Fields
        private ColumnSchema[] _columns;
        private int _Id;
        private string _name;
        private Guid _dbId;
        private TreeAddress _address => GetTreeAddress();
        private Guid _objectId;
        private DatabaseSchemaInfo _schema;
        #endregion

        #region Public Properties
        /// <summary>
        /// The columns of a table
        /// </summary>
        public override ColumnSchema[] Columns => _columns;
        /// <summary>
        /// The local id of the table
        /// </summary>
        public override int Id => _Id;
        /// <summary>
        /// The name of the table
        /// </summary>
        public override string Name => _name;
        /// <summary>
        /// The local database id that the table belongs to
        /// </summary>
        public override Guid DatabaseId => _dbId;
        /// <summary>
        /// The address for this table (database id, table id)
        /// </summary>
        public override TreeAddress Address => _address;
        public override Guid ObjectId => _objectId;
        public override DatabaseSchemaInfo Schema => _schema;
        public override string DatabaseName { get; set; }
        public override LogicalStoragePolicy StoragePolicy => LogicalStoragePolicy.None;
        public override Guid ContractGUID { get; set; }
        #endregion

        #region Constructors
        public SystemTableSchema100(int id, string name, Guid dbId, List<ColumnSchema> columns)
        {
            _Id = id;
            _name = name;
            _dbId = dbId;
            _columns = columns.ToArray();
            ContractGUID = Guid.Empty;
        }

        public SystemTableSchema100(int id, string name, Guid dbId, List<ColumnSchema> columns, Guid objectId) : this(id, name, dbId, columns)
        {
            _objectId = objectId;
        }
        #endregion

        #region Public Methods
        public override void SortBinaryOrder()
        {
            _columns = _columns.ToList().OrderBy(c => !c.IsFixedBinaryLength).ThenBy(c => c.Ordinal).ToArray();
        }

        public override bool HasColumn(string columnName)
        {
            foreach (var column in _columns)
            {
                if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool HasAllFixedWithColumns()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private TreeAddress GetTreeAddress()
        {
            return new TreeAddress(_dbId, _Id, _schema.SchemaGUID);
        }

      
        #endregion

    }
}
