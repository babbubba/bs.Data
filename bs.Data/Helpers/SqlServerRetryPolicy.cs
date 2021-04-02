using bs.Data.Interfaces;
using System;
using System.Data.SqlClient;

namespace bs.Data.Helpers
{
        internal class SqlServerRetryPolicy : IRetryPolicy
        {
            private int tries;
            private readonly int maxRetry;

            public SqlServerRetryPolicy(int maxRetry)
            {
                if (maxRetry < 1) throw new ArgumentOutOfRangeException("cutOffPoint");
                this.maxRetry = maxRetry;
            }

            public bool PerformRetry(SqlException ex)
            {
                if (ex == null) throw new ArgumentNullException("ex");
                // checks the ErrorCode property on the SqlException
                return SqlServerExceptions.IsThisADeadlock(ex) && ++tries < maxRetry;
            }
        }
}