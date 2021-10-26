using System;

namespace Drummersoft.DrummerDB.Core.Structures.Exceptions
{
    /// <summary>
    /// Thrown when a database verison supplied is not handled. This exception is meant to be informational to the database software developer.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    internal class UnknownDbVersionException : Exception
    {
        public UnknownDbVersionException() : base() { }
        public UnknownDbVersionException(string message) : base(message) { }
        public UnknownDbVersionException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected UnknownDbVersionException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Makes a new <see cref="UnknownDbVersionException"/> class. Thrown when the expected version is not found in code.
        /// </summary>
        /// <param name="expectedVersion">The expected version.</param>
        public UnknownDbVersionException(int expectedVersion) : base($"The expected db version {expectedVersion.ToString()} was not found.") { }

    }
}
