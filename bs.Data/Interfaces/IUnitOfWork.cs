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

        bool TransactionIsNotNull { get; }
        /// <summary>
        /// Begins the transaction.
        /// </summary>
        void BeginTransaction();

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
        /// Closes current transaction.
        /// </summary>
        void CloseTransaction();
    }
}