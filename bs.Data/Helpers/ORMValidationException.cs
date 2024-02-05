using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace bs.Data.Helpers
{
    /// <summary>
    /// ORM Validation exception
    /// </summary>
    /// <seealso cref="bs.Data.Helpers.OrmException" />
    public class OrmValidationException : OrmException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrmValidationException"/> class.
        /// </summary>
        public OrmValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrmValidationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OrmValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrmValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public OrmValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets the validation errors.
        /// </summary>
        /// <value>
        /// The validation errors.
        /// </value>
        public IReadOnlyCollection<string> ValidationErrors { get; set; }
    }
}