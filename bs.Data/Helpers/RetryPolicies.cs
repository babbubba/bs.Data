using bs.Data.Interfaces;
using System;

namespace bs.Data.Helpers
{
        internal class RetryPolicies : INotConfiguredPolicy
        {
            private readonly IRetryPolicy policy;

            private RetryPolicies(IRetryPolicy policy)
            {
                if (policy == null) throw new ArgumentNullException("policy");
                this.policy = policy;
            }

            public static readonly INotConfiguredPolicy ExponentialBackOff =
                new RetryPolicies(new ExponentialBackOffPolicy(TimeSpan.FromMilliseconds(200)));

            IRetryPolicy INotConfiguredPolicy.RetryOnLivelockAndDeadlock(int retries)
            {
                return new ChainingPolicy(new[] { new SqlServerRetryPolicy(retries), policy });
            }
        }
}