using bs.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace bs.Data.Helpers
{
    /// <summary>
    /// Extension to manage Unit of Work transactions
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        /// Execute the statement in action wrapped by an ORM transaction asyncronously. It commit (and in case of exception rollback) when action finish. After it close and destroy the transaction.
        /// </summary>
        /// <param name="uow">The unit of work.</param>
        /// <param name="action">The action to execute in the unit of work context.</param>
        /// <example>
        /// You can use this in this way:<code>await uow.RunInTransactionAsync(async () =&gt;
        /// {
        ///     var country = new CountryModel
        ///     {
        ///         Name = "Italy"
        ///     };
        ///     await repo.CreateCountryAsync(country);
        /// }</code></example>
        public static async Task RunInTransactionAsync(this IUnitOfWork uow, Action action)
        {
            try
            {
                uow.BeginTransaction();
                action();
                await uow.CommitAsync();
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
            finally
            {
                uow.CloseTransaction();
            }
        }

        /// <summary>
        /// Execute the statement in action wrapped by an ORM transaction. It commit (and in case of exception rollback) when action finish. After it close and destroy the transaction.
        /// </summary>
        /// <param name="uow">The uow.</param>
        /// <param name="action">The action.</param>
        public static void RunInTransaction(this IUnitOfWork uow, Action action)
        {
            try
            {
                uow.BeginTransaction();
                action();
                uow.Commit();
            }
            catch
            {
                uow.Rollback();
                throw;
            }
            finally
            {
                uow.CloseTransaction();
            }
        }

        /// <summary>
        /// Execute the statement in function (so it has a retun value) wrapped by an ORM transaction asyncronously. It commit (and in case of exception rollback) when action finish. After it close and destroy the transaction.
        /// </summary>
        /// <typeparam name="T">The return value Type.</typeparam>
        /// <param name="uow">The uow.</param>
        /// <param name="func">The function.</param>
        /// <example>
        /// You can use this in this way:<code>var entity = await uow.RunInTransactionAsync(async () =&gt;
        /// {
        ///     var country = new CountryModel
        ///     {
        ///         Name = "Italy"
        ///     };
        ///     await repo.CreateCountryAsync(country);
        ///     return country;
        /// }</code></example>
        /// <returns>The function's return value</returns>
        public static async Task<T> RunInTransactionAsync<T>(this IUnitOfWork uow, Func<Task<T>> func)
        {
            try
            {
                uow.BeginTransaction();
                var result = await func();
                await uow.CommitAsync();
                return result;
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
            finally
            {
                uow.CloseTransaction();
            }
        }

        /// <summary>
        /// Execute the statement in function (so it has a retun value) wrapped by an ORM transaction. It commit (and in case of exception rollback) when action finish. After it close and destroy the transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uow">The uow.</param>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public static T RunInTransaction<T>(this IUnitOfWork uow, Func<T> func)
        {
            try
            {
                uow.BeginTransaction();
                var result = func();
                uow.Commit();
                return result;
            }
            catch
            {
                uow.Rollback();
                throw;
            }
            finally
            {
                uow.CloseTransaction();
            }
        }
    }
}