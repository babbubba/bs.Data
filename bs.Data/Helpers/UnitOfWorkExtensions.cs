using bs.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace bs.Data.Helpers
{
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        /// Execute the statement in action wrapped by an ORM transaction asyncronously. It commit (and in case of exception rollback) when action finish. After it close and destroy the transaction.
        /// </summary>
        /// <param name="uow">The uow.</param>
        /// <param name="action">The action.</param>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="uow">The uow.</param>
        /// <param name="func">The function.</param>
        /// <returns></returns>
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