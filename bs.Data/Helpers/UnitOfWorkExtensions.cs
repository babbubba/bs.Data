using bs.Data.Interfaces;
using Microsoft.Data.SqlClient;
using NHibernate;
using System;
using System.Threading.Tasks;

namespace bs.Data.Helpers
{
    /// <summary>
    /// Extension to manage Unit of Work transactions
    /// </summary>
    public static partial class UnitOfWorkExtensions
    {
        /// <summary>
        /// Walks the exception chain looking for a <see cref="SqlException"/>.
        /// </summary>
        /// <remarks>
        /// NHibernate typically wraps raw ADO.NET provider exceptions (including a SQL Server
        /// <see cref="SqlException"/>) in its own <see cref="ADOException"/> hierarchy
        /// (<c>GenericADOException</c>) before they reach calling code. A deadlock can therefore
        /// arrive here as an <see cref="ADOException"/> whose <see cref="Exception.InnerException"/>
        /// (or a deeper ancestor) is the real <see cref="SqlException"/>. Without unwrapping it,
        /// deadlock detection/retry would only ever trigger for a bare <see cref="SqlException"/>
        /// thrown directly (uncommon), silently never firing for the far more common case of a
        /// deadlock raised while flushing (e.g. during <c>Session.SaveAsync</c>/<c>UpdateAsync</c>).
        /// </remarks>
        private static SqlException FindSqlException(Exception ex)
        {
            while (ex != null)
            {
                if (ex is SqlException sqlEx) return sqlEx;
                ex = ex.InnerException;
            }
            return null;
        }

        /// <summary>
        /// Rolls back the transaction, tolerating the case where the server already aborted it
        /// on its own.
        /// </summary>
        /// <remarks>
        /// When SQL Server kills a transaction to resolve a deadlock, the connection is left in a
        /// "zombied" state on the client: <see cref="ITransaction.WasRolledBack"/> is still
        /// <c>false</c> (no one called <c>Rollback</c> from here), yet issuing a
        /// <see cref="ITransaction.RollbackAsync"/> against it throws a
        /// <see cref="TransactionException"/> ("Transaction not connected, or was disconnected").
        /// That exception would otherwise escape from inside the catch block that is trying to
        /// evaluate the deadlock for a retry, since a sibling catch clause never catches an
        /// exception raised by another catch clause in the same try. There is nothing left to
        /// roll back in that case, so it is safe to swallow it and let the deadlock retry policy
        /// decide what happens next.
        /// </remarks>
        private static async Task SafeRollbackAsync(ITransaction transaction)
        {
            if (transaction.WasRolledBack) return;

            try
            {
                await transaction.RollbackAsync();
            }
            catch (TransactionException)
            {
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
            catch (SqlException sqlEx)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();
                throw new ORMException(sqlEx.Message, sqlEx, "SQL");
            }
            catch (ADOException adoEx)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();

                var wrappedSqlEx = FindSqlException(adoEx);
                if (wrappedSqlEx != null)
                    throw new ORMException(wrappedSqlEx.Message, wrappedSqlEx, "SQL");

                throw new ORMException(adoEx.Message, adoEx, "ADO");
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
                throw new ORMException(sqlEx.Message, sqlEx, "SQL");
            }
            catch (ADOException AdoEx)
            {
                if (uow.TransactionIsNotNull) uow.Rollback();

                var wrappedSqlEx = FindSqlException(AdoEx);
                if (wrappedSqlEx != null)
                    throw new ORMException(wrappedSqlEx.Message, wrappedSqlEx, "SQL");

                throw new ORMException(AdoEx.Message, AdoEx, "ADO");
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

        /// <summary>
        /// Execute the statement in action wrapped by an ORM transaction asynchronously.
        /// </summary>
        /// <remarks>
        /// This overload accepts a synchronous <see cref="Action"/> delegate. For async workloads use
        /// <see cref="RunInTransactionAsync{T}(IUnitOfWork, Func{Task{T}}, int)"/> instead, which correctly
        /// awaits the delegate and supports deadlock retry.
        /// </remarks>
        [Obsolete("Use RunInTransactionAsync(Func<Task<T>>, int) instead. This overload accepts a synchronous Action which cannot properly await async operations inside the delegate.")]
        public static async Task RunInTransactionAsync(this IUnitOfWork uow, Action action, int retry = 3)
        {
            if (uow is null)
                throw new ORMException("Unit of work is not a valid instance, cannot run a new transaction");

            if (uow.Session is null)
                throw new ORMException("Unit of work has not a valid session instance, cannot run a new transaction");

            // Built once per call, before the retry loop: the retry counter and the exponential
            // back-off interval are mutable state that must persist across retries of THIS
            // transaction and must never be shared with concurrent/unrelated calls (see
            // RetryPolicies.CreateExponentialBackOff for why a shared/singleton instance is unsafe).
            var deadlockRetryPolicy = RetryPolicies.CreateExponentialBackOff().RetryOnLivelockAndDeadlock(retry);

            while (true)
            {
                using (var transaction = uow.Session.BeginTransaction())
                {
                    try
                    {
                        action();
                        await transaction.CommitAsync();
                        break;
                    }
                    catch (SqlException sqlEx)
                    {
                        await SafeRollbackAsync(transaction);

                        // Try perform retry with exponential back off if this is a DeadLock exception
                        if (await deadlockRetryPolicy.PerformRetryAsync(sqlEx))
                        {
                            // The session's first-level cache may still hold entities loaded/modified
                            // during the rolled-back attempt. Without clearing it, the retry would see
                            // stale, still-dirty in-memory state instead of re-reading from the database,
                            // risking double-applied changes or a StaleObjectStateException.
                            uow.Session.Clear();
                            continue;
                        }

                        throw new ORMException(sqlEx.Message, sqlEx, "SQL");
                    }
                    catch (ADOException AdoEx)
                    {
                        await SafeRollbackAsync(transaction);

                        var wrappedSqlEx = FindSqlException(AdoEx);
                        if (wrappedSqlEx != null && await deadlockRetryPolicy.PerformRetryAsync(wrappedSqlEx))
                        {
                            uow.Session.Clear();
                            continue;
                        }
                        if (wrappedSqlEx != null) throw new ORMException(wrappedSqlEx.Message, wrappedSqlEx, "SQL");

                        throw new ORMException(AdoEx.Message, AdoEx, "ADO");
                    }
                    catch (Exception ex)
                    {
                        await SafeRollbackAsync(transaction);
                        throw new ORMException(ex.Message, ex, "GENERIC");
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

            // Built once per call, before the retry loop: the retry counter and the exponential
            // back-off interval are mutable state that must persist across retries of THIS
            // transaction and must never be shared with concurrent/unrelated calls (see
            // RetryPolicies.CreateExponentialBackOff for why a shared/singleton instance is unsafe).
            var deadlockRetryPolicy = RetryPolicies.CreateExponentialBackOff().RetryOnLivelockAndDeadlock(retry);

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
                        await SafeRollbackAsync(transaction);

                        // Try perform retry with exponential back off if this is a DeadLock exception
                        if (await deadlockRetryPolicy.PerformRetryAsync(sqlEx))
                        {
                            // The session's first-level cache may still hold entities loaded/modified
                            // during the rolled-back attempt. Without clearing it, the retry would see
                            // stale, still-dirty in-memory state instead of re-reading from the database,
                            // risking double-applied changes or a StaleObjectStateException.
                            uow.Session.Clear();
                            continue;
                        }

                        // This was not a DeadLock exception so throw exception
                        throw new ORMException(sqlEx.Message, sqlEx, "SQL");
                    }
                    catch (ADOException AdoEx)
                    {
                        await SafeRollbackAsync(transaction);

                        // NHibernate often wraps a deadlocking SqlException inside a GenericADOException
                        // (an ADOException) rather than letting it surface directly - unwrap it so the
                        // retry logic above still engages instead of failing immediately.
                        var wrappedSqlEx = FindSqlException(AdoEx);
                        if (wrappedSqlEx != null && await deadlockRetryPolicy.PerformRetryAsync(wrappedSqlEx))
                        {
                            uow.Session.Clear();
                            continue;
                        }
                        if (wrappedSqlEx != null) throw new ORMException(wrappedSqlEx.Message, wrappedSqlEx, "SQL");

                        throw new ORMException(AdoEx.Message, AdoEx, "ADO");
                    }
                    catch (Exception ex)
                    {
                        await SafeRollbackAsync(transaction);
                        throw new ORMException(ex.GetBaseException().Message, ex, "GENERIC");
                    }
                }
            }
        }
    }
}