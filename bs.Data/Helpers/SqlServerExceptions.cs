using System.Data.SqlClient;

namespace bs.Data.Helpers
{
        internal static class SqlServerExceptions
        {
            public static bool IsThisADeadlock(SqlException realException)
            {
                return realException.ErrorCode == 1205;
            }
        }
}