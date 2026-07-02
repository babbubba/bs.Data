using bs.Data.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

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
        /// Waits with exponential back-off and returns whether another retry should be attempted.
        /// Uses <see cref="Task.Delay"/> to avoid blocking the calling thread.
        /// </summary>
        public async Task<bool> PerformRetryAsync(SqlException ex)
        {
            await Task.Delay(currentWait);
            currentWait = currentWait == TimeSpan.Zero ? TimeSpan.FromMilliseconds(20) : currentWait + currentWait;
            return currentWait <= maxWait;
        }
    }
}