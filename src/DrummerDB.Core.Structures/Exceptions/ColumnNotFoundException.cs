using System;

namespace Drummersoft.DrummerDB.Core.Structures.Exceptions
{
    /// <summary>
    /// Thrown when the specified column does not exist in the specified table. This exception should be caught by any UI leveraging the database system.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    internal class ColumnNotFoundException : Exception
    {
        public ColumnNotFoundException() : base() { }
        public ColumnNotFoundException(string message) : base(message) { }
        public ColumnNotFoundException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected ColumnNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Makes a new <see cref="ColumnNotFoundException"/> class. Thrown when the specified column does not exist in the specified table.
        /// </summary>
        /// <param name="expectedColumnName">Name of the column.</param>
        /// <param name="tableName">Name of the table.</param>
        public ColumnNotFoundException(string expectedColumnName, string tableName) : base($"The column {expectedColumnName} was not found in table {tableName}") { }
    }
}
