using bs.Data.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

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
            if (maxRetry < 1) throw new ArgumentOutOfRangeException(nameof(maxRetry));
            this.maxRetry = maxRetry;
        }

        /// <summary>
        /// Returns whether a retry should be attempted: only if the exception is a deadlock
        /// and the retry counter has not reached the configured limit.
        /// </summary>
        public Task<bool> PerformRetryAsync(SqlException ex)
        {
            if (ex == null) return Task.FromResult(false);

            // SqlException.Number 1205 = deadlock victim (see SqlServerExceptions.IsThisADeadlock)
            bool shouldRetry = SqlServerExceptions.IsThisADeadlock(ex) && ++tries < maxRetry;
            return Task.FromResult(shouldRetry);
        }
    }
}