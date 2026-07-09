using bs.Data.Interfaces;
using System;

namespace bs.Data.Helpers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.INotConfiguredPolicy" />
    internal class RetryPolicies : INotConfiguredPolicy
    {
        /// <summary>
        /// Creates a new exponential back-off policy configuration.
        /// </summary>
        /// <remarks>
        /// A fresh instance must be created for every independent retry loop (i.e. once per call
        /// to <c>RunInTransactionAsync</c>, before its <c>while</c> loop), not shared as a
        /// singleton. The underlying <see cref="ExponentialBackOffPolicy"/> carries mutable state
        /// (the current wait interval), which must not be shared/reused across unrelated calls or
        /// concurrent transactions: doing so both breaks the per-call retry limit and, since the
        /// wait interval only ever grows and is never reset, permanently disables retries for the
        /// rest of the process once enough cumulative deadlocks have pushed it past the max wait.
        /// </remarks>
        public static INotConfiguredPolicy CreateExponentialBackOff() =>
            new RetryPolicies(new ExponentialBackOffPolicy(TimeSpan.FromMilliseconds(200)));

        private readonly IRetryPolicy policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicies"/> class.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <exception cref="ArgumentNullException">policy</exception>
        private RetryPolicies(IRetryPolicy policy)
        {
            this.policy = policy ?? throw new ArgumentNullException("policy");
        }

        /// <summary>
        /// Retries the on livelock and deadlock.
        /// </summary>
        /// <param name="retries">The retries.</param>
        /// <returns></returns>
        IRetryPolicy INotConfiguredPolicy.RetryOnLivelockAndDeadlock(int retries)
        {
            return new ChainingPolicy(new[] { new SqlServerRetryPolicy(retries), policy });
        }
    }
}