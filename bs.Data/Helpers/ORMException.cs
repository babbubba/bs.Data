using System;

namespace bs.Data.Helpers
{
    /// <summary>
    /// Generic exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class OrmException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrmException"/> class.
        /// </summary>
        public OrmException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrmException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OrmException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrmException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public OrmException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrmException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="exceptionOrigin">The exception origin (SQL, ADO, Generic, ...).</param>
        public OrmException(string message, Exception innerException, string exceptionOrigin) : base(message, innerException)
        {
            ExceptionOrigin = exceptionOrigin;
        }

        public string ExceptionOrigin { get; }
    }
}