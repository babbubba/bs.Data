using bs.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace bs.Data.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.IRetryPolicy" />
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
                if (policies == null) throw new ArgumentNullException("policies");
                this.policies = policies;
            }

        /// <summary>
        /// Performs the retry.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public bool PerformRetry(SqlException ex)
            {
                return policies.Aggregate(true, (val, policy) => val && policy.PerformRetry(ex));
            }
        }
}