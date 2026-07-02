using NHibernate;
using System;
using System.Threading.Tasks;

namespace bs.Data.Interfaces
{
    /// <summary>
    /// Unit of work used to enable transactional access to database
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    /// <seealso cref="System.IDisposable" />

    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the ORM session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        ISession Session { get; }

        /// <summary>
        /// Gets a value indicating whether [transaction is not null].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [transaction is not null]; otherwise, <c>false</c>.
        /// </value>
        bool TransactionIsNotNull { get; }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Closes current transaction.
        /// </summary>
        void CloseTransaction();

        /// <summary>
        /// Commits the current transaction in this session
        /// </summary>
        void Commit();

        /// <summary>
        /// Commits the current transaction in this session asynchronously.
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();

        /// <summary>
        /// Rollbacks the current transaction in this session
        /// </summary>
        void Rollback();

        /// <summary>
        /// Rollbacks the current transaction in this session asynchronously.
        /// </summary>
        /// <returns></returns>
        Task RollbackAsync();

        /// <summary>
        /// Tries to commit the current transaction in this session if exception occurs rollback transaction and throw the exception.
        /// </summary>
        void TryCommitOrRollback();

        /// <summary>
        /// Tries to commit the current transaction in this session if exception occurs rollback transaction asynchronously and throw the exception.
        /// </summary>
        /// <returns></returns>
        Task TryCommitOrRollbackAsync();

        /// <summary>
        /// Flushes pending changes to the database (without committing) and clears the session
        /// first-level cache. Use this in batch loops to avoid unbounded memory growth.
        /// </summary>
        /// <remarks>
        /// <see cref="ISession.Flush"/> pushes pending SQL to the DB within the current transaction.
        /// <see cref="ISession.Clear"/> then evicts all entities from the identity map, freeing memory.
        /// Call periodically inside large batch operations:
        /// <code>
        /// for (int i = 0; i &lt; rows.Count; i++)
        /// {
        ///     await repo.CreateAsync(rows[i]);
        ///     if (i % 100 == 0) uow.FlushAndClear();
        /// }
        /// </code>
        /// </remarks>
        void FlushAndClear();

        /// <summary>
        /// Flushes pending changes to the database asynchronously (without committing) and clears
        /// the session first-level cache. Use this in batch loops to avoid unbounded memory growth.
        /// </summary>
        /// <remarks>
        /// <see cref="ISession.FlushAsync"/> pushes pending SQL to the DB within the current transaction.
        /// <see cref="ISession.Clear"/> then evicts all entities from the identity map, freeing memory.
        /// Call periodically inside large batch operations:
        /// <code>
        /// for (int i = 0; i &lt; rows.Count; i++)
        /// {
        ///     await repo.CreateAsync(rows[i]);
        ///     if (i % 100 == 0) await uow.FlushAndClearAsync();
        /// }
        /// </code>
        /// </remarks>
        Task FlushAndClearAsync();
    }
}