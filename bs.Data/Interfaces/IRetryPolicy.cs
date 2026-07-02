using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace bs.Data.Interfaces
{
    internal interface IRetryPolicy
    {
        Task<bool> PerformRetryAsync(SqlException ex);
    }
}