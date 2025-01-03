﻿using bs.Data.Interfaces;
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
        /// The exponential back off
        /// </summary>
        public static readonly INotConfiguredPolicy ExponentialBackOff =
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