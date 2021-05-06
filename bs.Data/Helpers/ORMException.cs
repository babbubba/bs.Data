using System;
using System.Runtime.Serialization;

namespace bs.Data.Helpers
{
    /// <summary>
    /// Generic exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ORMException : Exception
    {
        public string ExceptionOrigin { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ORMException"/> class.
        /// </summary>
        public ORMException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ORMException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public ORMException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="exceptionOrigin">The exception origin (SQL, ADO, Generic, ...).</param>
        public ORMException(string message, Exception innerException, string exceptionOrigin) : base(message, innerException)
        {
            ExceptionOrigin = exceptionOrigin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ORMException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}