using Microsoft.Data.SqlClient;

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
            // SqlException.Number is the SQL Server error number (1205 = deadlock victim).
            // Note: ErrorCode returns the COM HResult (-2146232060 for all SqlExceptions) and must NOT be used here.
            return realException.Number == 1205;
        }
    }
}