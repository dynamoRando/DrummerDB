using System;

namespace Drummersoft.DrummerDB.Core.Structures.Exceptions
{
    /// <summary>
    /// Thrown when the specified SQL Type supplied is not handled. This exception is meant to be informational to the database software developer.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    internal class UnknownSQLTypeException : Exception
    {
        public UnknownSQLTypeException() : base() { }
        public UnknownSQLTypeException(string message) : base(message) { }
        public UnknownSQLTypeException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected UnknownSQLTypeException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
