using bs.Data.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Threading;

namespace bs.Data.Helpers
{
    internal class ExponentialBackOffPolicy : IRetryPolicy
    {
        private readonly TimeSpan maxWait;
        private TimeSpan currentWait = TimeSpan.Zero; // initially, don't wait

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackOffPolicy"/> class.
        /// </summary>
        /// <param name="maxWait">The maximum wait.</param>
        public ExponentialBackOffPolicy(TimeSpan maxWait)
        {
            this.maxWait = maxWait;
        }

        /// <summary>
        /// Performs the retry.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public bool PerformRetry(SqlException ex)
        {
            Thread.Sleep(currentWait);
            currentWait = currentWait == TimeSpan.Zero ? TimeSpan.FromMilliseconds(20) : currentWait + currentWait;
            return currentWait <= maxWait;
        }
    }
}