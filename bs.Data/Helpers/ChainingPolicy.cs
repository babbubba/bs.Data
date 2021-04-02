using bs.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace bs.Data.Helpers
{
        internal class ChainingPolicy : IRetryPolicy
        {
            private readonly IEnumerable<IRetryPolicy> policies;

            public ChainingPolicy(IEnumerable<IRetryPolicy> policies)
            {
                if (policies == null) throw new ArgumentNullException("policies");
                this.policies = policies;
            }

            public bool PerformRetry(SqlException ex)
            {
                return policies.Aggregate(true, (val, policy) => val && policy.PerformRetry(ex));
            }
        }
}