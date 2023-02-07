using bs.Data.Interfaces;
using NHibernate;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace bs.Data.Helpers
{
    /// <summary>
    /// Extension to manage Unit of Work transactions
    /// </summary>
    public static partial class UnitOfWorkExtensions
    {
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
            catch (SqlException sqlEx)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();
                throw new ORMException(sqlEx?.Message, sqlEx, "SQL");
            }
            catch (ADOException AdoEx)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();
                throw new ORMException(AdoEx?.Message, AdoEx, "ADO");
            }
            catch (Exception ex)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();
                throw new ORMException(ex.GetBaseException().Message, ex, "GENERIC");
            }
            finally
            {
                if (uow.TransactionIsNotNull) uow.CloseTransaction();
            }
        }

        public static async Task RunInTransactionAsync(this IUnitOfWork uow, Action action, int retry = 3)
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
                    catch (SqlException sqlEx)
                    {
                        if (!transaction.WasRolledBack) await transaction.RollbackAsync();

                        // Try perform retry with exponential back off if this is a DeadLock exception
                        if (RetryPolicies.ExponentialBackOff.RetryOnLivelockAndDeadlock(retry).PerformRetry(sqlEx)) continue;

                        // This was not a DeadLock exception so throw exception
                        throw new ORMException(sqlEx?.Message, sqlEx, "SQL");
                    }
                    catch (ADOException AdoEx)
                    {
                        if (!transaction.WasRolledBack) await transaction.RollbackAsync();
                        throw new ORMException(AdoEx?.Message, AdoEx, "ADO");
                    }
                    catch (Exception ex)
                    {
                        if (!transaction.WasRolledBack) await transaction.RollbackAsync();
                        throw new ORMException(ex?.Message, ex, "GENERIC");
                    }
                }
            }
        }

        /// <summary>
        /// Execute the statement in function (so it has a retun value) wrapped by an ORM transaction asyncronously. It commit (and in case of exception rollback) when action finish. After it close and destroy the transaction.
        /// </summary>
        /// <typeparam name="T">The return value Type.</typeparam>
        /// <param name="uow">The uow.</param>
        /// <param name="func">The function.</param>
        /// <param name="retry">Hom many time retry in case of deadlock exception.</param>
        /// <returns>
        /// The function's return value
        /// </returns>
        /// <exception cref="ORMException"></exception>
        /// <example>
        /// You can use this in this way:<code>var entity = await uow.RunInTransactionAsync(async () =&gt;
        /// {
        /// var country = new CountryModel
        /// {
        /// Name = "Italy"
        /// };
        /// await repo.CreateCountryAsync(country);
        /// return country;
        /// }</code></example>
        public static async Task<T> RunInTransactionAsync<T>(this IUnitOfWork uow, Func<Task<T>> func, int retry = 3)
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
                        var result = await func();
                        await transaction.CommitAsync();
                        return result; // stop looping
                    }
                    catch (SqlException sqlEx)
                    {
                        if (!transaction.WasRolledBack) await transaction.RollbackAsync();

                        // Try perform retry with exponential back off if this is a DeadLock exception
                        if (RetryPolicies.ExponentialBackOff.RetryOnLivelockAndDeadlock(retry).PerformRetry(sqlEx)) continue;

                        // This was not a DeadLock exception so throw exception
                        throw new ORMException(sqlEx?.Message, sqlEx, "SQL");
                    }
                    catch (ADOException AdoEx)
                    {
                        if (!transaction.WasRolledBack) await transaction.RollbackAsync();
                        throw new ORMException(AdoEx?.Message, AdoEx, "ADO");
                    }
                    catch (Exception ex)
                    {
                        if (!transaction.WasRolledBack) await transaction.RollbackAsync();
                        throw new ORMException(ex?.Message, ex, "GENERIC");
                    }
                }
            }
        }
    }
}