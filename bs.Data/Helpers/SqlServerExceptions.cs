using System.Data.SqlClient;

namespace bs.Data.Helpers
{
    internal static class SqlServerExceptions
    {
        /// <summary>
        /// Determines whether the specified real Sql Exception is this a deadlock.
        /// </summary>
        /// <param name="realException">The real SQL Exception.</param>
        /// <returns>
        ///   <c>true</c> if [is this a deadlock]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsThisADeadlock(SqlException realException)
        {
            // The SQLException error code for DeadLock is 1205
            return realException.ErrorCode == 1205;
        }
    }
}