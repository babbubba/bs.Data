using bs.Data.Interfaces;
using System;
using System.Data.SqlClient;
using System.Threading;

namespace bs.Data.Helpers
{
        internal class ExponentialBackOffPolicy : IRetryPolicy
        {
            private readonly TimeSpan maxWait;
            private TimeSpan currentWait = TimeSpan.Zero; // initially, don't wait

            public ExponentialBackOffPolicy(TimeSpan maxWait)
            {
                this.maxWait = maxWait;
            }

            public bool PerformRetry(SqlException ex)
            {
                Thread.Sleep(currentWait);
                currentWait = currentWait == TimeSpan.Zero ? TimeSpan.FromMilliseconds(20) : currentWait + currentWait;
                return currentWait <= maxWait;
            }
        }
}