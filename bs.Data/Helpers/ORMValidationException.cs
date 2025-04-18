﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace bs.Data.Helpers
{
    /// <summary>
    /// ORM Validation exception
    /// </summary>
    /// <seealso cref="bs.Data.Helpers.ORMException" />
    public class ORMValidationException : ORMException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ORMValidationException"/> class.
        /// </summary>
        public ORMValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMValidationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ORMValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public ORMValidationException(string message, Exception innerException) : base(message, innerException)
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