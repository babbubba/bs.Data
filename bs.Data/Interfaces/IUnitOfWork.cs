using NHibernate;
using System;

namespace bs.Data.Interfaces
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets or sets the ORM session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        ISession Session { get; set; }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns></returns>
        ITransaction BeginTransaction();

        /// <summary>
        /// Commits the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        void Commit(ITransaction transaction);

        /// <summary>
        /// Tries the commit or rollback.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        void TryCommitOrRollback(ITransaction transaction);

        /// <summary>
        /// Rollbacks the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        void Rollback(ITransaction transaction);
    }
}