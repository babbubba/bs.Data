using bs.Data.Interfaces;
using System;
using System.Data.SqlClient;

namespace bs.Data.Helpers
{
    internal class SqlServerRetryPolicy : IRetryPolicy
    {
        private readonly int maxRetry;
        private int tries;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerRetryPolicy"/> class.
        /// </summary>
        /// <param name="maxRetry">The maximum retry. It has to be greater than 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">maxRetry</exception>
        public SqlServerRetryPolicy(int maxRetry)
        {
            if (maxRetry < 1) throw new ArgumentOutOfRangeException("maxRetry");
            this.maxRetry = maxRetry;
        }

        /// <summary>
        /// Performs the retry if the error was a DeadLock and try counter has not reached the limit.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public bool PerformRetry(SqlException ex)
        {
            // If this is not a sql exception cannot be a deadlock... so return false
            if (ex == null) return false;

            // checks if the SqlException is a DeadLock error and if the current try is less than max retry
            return SqlServerExceptions.IsThisADeadlock(ex) && ++tries < maxRetry;
        }
    }
}