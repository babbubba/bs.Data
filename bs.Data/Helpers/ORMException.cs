using System;
using System.Runtime.Serialization;

namespace bs.Data.Helpers
{
    public class ORMException : Exception
    {
        public ORMException()
        {
        }

        public ORMException(string message) : base(message)
        {
        }

        public ORMException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ORMException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}