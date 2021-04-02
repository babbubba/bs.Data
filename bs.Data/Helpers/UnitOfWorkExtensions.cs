using bs.Data.Interfaces;
using NHibernate;
using NHibernate.Exceptions;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace bs.Data.Helpers
{
    /// <summary>
    /// Extension to manage Unit of Work transactions
    /// </summary>
    public static partial class UnitOfWorkExtensions
    {
        /// <summary>
        /// Execute the statement in action wrapped by an ORM transaction asyncronously. It commit (and in case of exception rollback) when action finish. After it close and destroy the transaction.
        /// It doesnt matter if you closed the transaction before.
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
            if (uow is null)
            {
                throw new ORMException("Unit of work is not a valid instance, cannot run a new transaction");
            }
            if (uow.Session is null)
            {
                throw new ORMException("Unit of work has not a valid session instance, cannot run a new transaction");
            }


            try
            {
                uow.BeginTransaction();
                action();

                if (uow.TransactionIsNotNull) await uow.CommitAsync();
            }
            catch (Exception ex)
            {
                if (uow.TransactionIsNotNull) await uow.RollbackAsync();
                throw new ORMException(ex.GetBaseException().Message, ex);
            }
            finally
            {
                if (uow.TransactionIsNotNull) uow.CloseTransaction();
            }
        }

        public static async Task RunInTransactionAsync(this IUnitOfWork uow, Action action, int retry)
        {
            if (uow is null)
            {
                throw new ORMException("Unit of work is not a valid instance, cannot run a new transaction");
            }
            if (uow.Session is null)
            {
                throw new ORMException("Unit of work has not a valid session instance, cannot run a new transaction");
            }

            while (true)
            {
                using (var transaction = uow.Session.BeginTransaction())
                {
                    try
                    {
                        action();
                        await transaction.CommitAsync();
                        break; // stop looping
                    }
                    catch (ADOException ex)
                    {
                        if (!transaction.WasRolledBack) await transaction.RollbackAsync(); // will back our transaction


                        var dbException = ADOExceptionHelper.ExtractDbException(ex) as SqlException;

                        if (RetryPolicies.ExponentialBackOff.RetryOnLivelockAndDeadlock(retry).PerformRetry(dbException)) continue;
                        throw new ORMException(dbException?.Message, ex);
                    }
                }
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
                if (uow.TransactionIsNotNull) uow.Commit();
            }
            catch (Exception ex)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();
                throw new ORMException(ex.GetBaseException().Message, ex);

            }
            finally
            {
                if (uow.TransactionIsNotNull) uow.CloseTransaction();
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
                if (uow.TransactionIsNotNull) await uow.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                if (uow.TransactionIsNotNull) await uow.RollbackAsync();
                throw new ORMException(ex.GetBaseException().Message, ex);
            }
            finally
            {
                if (uow.TransactionIsNotNull) uow.CloseTransaction();
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
                if (uow.TransactionIsNotNull) uow.Commit();
                return result;
            }
            catch (Exception ex)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();
                throw new ORMException(ex.GetBaseException().Message, ex);
            }
            finally
            {
                if (uow.TransactionIsNotNull) uow.CloseTransaction();
            }
        }
    }
}