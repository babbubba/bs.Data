using bs.Data.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bs.Data.Helpers
{
    /// <summary>
    /// Chains multiple <see cref="IRetryPolicy"/> instances: all policies must agree to retry.
    /// </summary>
    internal class ChainingPolicy : IRetryPolicy
    {
        private readonly IEnumerable<IRetryPolicy> policies;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainingPolicy"/> class.
        /// </summary>
        /// <param name="policies">The policies.</param>
        /// <exception cref="ArgumentNullException">policies</exception>
        public ChainingPolicy(IEnumerable<IRetryPolicy> policies)
        {
            this.policies = policies ?? throw new ArgumentNullException(nameof(policies));
        }

        /// <summary>
        /// Returns true only if every policy in the chain agrees to retry.
        /// </summary>
        public async Task<bool> PerformRetryAsync(SqlException ex)
        {
            foreach (var policy in policies)
            {
                if (!await policy.PerformRetryAsync(ex))
                    return false;
            }
            return true;
        }
    }
}