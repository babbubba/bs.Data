using Microsoft.Data.SqlClient;

namespace bs.Data.Interfaces
{
    public interface IRetryPolicy
    {
        bool PerformRetry(SqlException ex);
    }
}