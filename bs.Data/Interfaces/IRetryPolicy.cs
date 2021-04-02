using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace bs.Data.Interfaces
{
    public interface IRetryPolicy
    {
        bool PerformRetry(SqlException ex);
    }
}