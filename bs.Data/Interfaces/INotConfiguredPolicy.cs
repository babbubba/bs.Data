using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces
{
    internal interface INotConfiguredPolicy
    {
        IRetryPolicy RetryOnLivelockAndDeadlock(int retries);
    }
}
