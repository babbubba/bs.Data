using Microsoft.AspNetCore.Http;
using bs.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace bs.Data.Middleware
{
    /// <summary>
    /// Middleware that ensures NHibernate Unit of Work is properly disposed at the end of each request.
    /// Transaction management should be handled explicitly using RunInTransaction/RunInTransactionAsync extensions.
    /// </summary>
    public class NHibernateSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public NHibernateSessionMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(unitOfWork);

            try
            {
                await _next(context);
            }
            finally
            {
                // Solo dispose - le transazioni sono gestite da RunInTransaction
                await unitOfWork.DisposeAsync();
            }
        }
    }
}