using bs.Data.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace bs.Data.TestAsync
{
    /// <summary>
    /// Regression tests for the deadlock retry back-off state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Before this fix, <c>UnitOfWorkExtensions.RunInTransactionAsync</c> read the exponential
    /// back-off policy from a <c>static readonly</c> singleton (<c>RetryPolicies.ExponentialBackOff</c>)
    /// shared by every call, in every concurrent transaction, for the lifetime of the process. Its
    /// mutable <c>currentWait</c> field only ever grew and was never reset, so once enough cumulative
    /// deadlocks (across the whole application, not just one transaction) pushed it past the configured
    /// max wait, the deadlock-retry feature silently and permanently stopped working for every
    /// subsequent deadlock the app would ever encounter, for the rest of the process's life.
    /// </para>
    /// <para>
    /// The fix replaces the singleton with <c>RetryPolicies.CreateExponentialBackOff()</c>, a factory
    /// that must be called once per call to <c>RunInTransactionAsync</c> (before its retry loop), giving
    /// every transaction its own independent, short-lived back-off state. These tests exercise
    /// <see cref="ExponentialBackOffPolicy"/> directly (accessible via <c>InternalsVisibleTo</c>) since
    /// that is the class that carried the bug; the SQL-Server-specific deadlock detection itself
    /// (<c>SqlServerRetryPolicy</c>, which requires a real <c>Microsoft.Data.SqlClient.SqlException</c>)
    /// is covered instead by the live-deadlock integration test in <see cref="SqlServerTests"/>, since
    /// <c>SqlException</c> cannot be constructed reliably outside of a real ADO.NET failure.
    /// </para>
    /// </remarks>
    public class RetryPolicyTests
    {
        [Fact]
        public async Task ExponentialBackOffPolicy_ExhaustsItsBudget_WhenReusedRepeatedly()
        {
            var policy = new ExponentialBackOffPolicy(TimeSpan.FromMilliseconds(200));

            // Wait sequence: 0 -> 20 -> 40 -> 80 -> 160 -> 320 (stop, 320 > 200).
            var attempts = 0;
            bool canRetry;
            do
            {
                canRetry = await policy.PerformRetryAsync(null);
                attempts++;
            } while (canRetry && attempts < 20);

            Assert.False(canRetry);
            // 20, 40, 80, 160 are all <= 200 (4 successful retries); 320 is the 5th, failing one.
            Assert.Equal(5, attempts);
        }

        [Fact]
        public async Task ExponentialBackOffPolicy_NewInstance_IsNotAffectedByAnExhaustedOne()
        {
            var maxWait = TimeSpan.FromMilliseconds(200);

            var exhausted = new ExponentialBackOffPolicy(maxWait);
            bool canRetry;
            do
            {
                canRetry = await exhausted.PerformRetryAsync(null);
            } while (canRetry);

            Assert.False(canRetry); // sanity check: the first instance's budget is used up

            // A brand-new instance (what RetryPolicies.CreateExponentialBackOff() hands out for
            // every new call to RunInTransactionAsync) must start fresh regardless of how many
            // other, unrelated retry loops have already exhausted their own budget.
            var fresh = new ExponentialBackOffPolicy(maxWait);
            Assert.True(await fresh.PerformRetryAsync(null));
        }

        [Fact]
        public void RetryPolicies_CreateExponentialBackOff_ReturnsANewInstanceEveryTime()
        {
            var first = RetryPolicies.CreateExponentialBackOff();
            var second = RetryPolicies.CreateExponentialBackOff();

            Assert.NotSame(first, second);
        }
    }
}
