using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace bs.Data.Helpers
{
    public class RepositoryException : Exception
    {
        public RepositoryException()
        {
        }
        public RepositoryException(string message) : base(message)
        {
        }


        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
