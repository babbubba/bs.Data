using bs.Data.Helpers;
using bs.Data.Interfaces;
using NHibernate;
using System;
using System.Threading.Tasks;

namespace bs.Data
{
    /// <summary>
    /// Unit of Work pattern for handling transactional operations
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.IUnitOfWork" />
    public sealed class UnitOfWork : IUnitOfWork
    {
        private bool disposedValue;
        private ITransaction transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public UnitOfWork(ISession session)
        {
            this.Session = session;
        }

        public ISession Session { get; }
        public bool TransactionIsNotNull => transaction is not null;

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <exception cref="ORMException">This Unit of Work contains a live transaction. You have to close existing transaction before creating new one.</exception>
        public void BeginTransaction()
        {
            if (transaction?.IsActive ?? false)
                throw new ORMException("This Unit of Work contains a live transaction. You have to close existing transaction before creating new one.");

            transaction = Session.BeginTransaction();
        }

        /// <summary>
        /// Closes the current transaction (if a transaction exists).
        /// </summary>
        public void CloseTransaction()
        {
            if (transaction != null)
            {
                transaction.Dispose();
                transaction = null;
            }
        }

        /// <summary>
        /// Commits the current transaction in this session if this transaction was not rolled back before.
        /// </summary>
        public void Commit()
        {
            if (!(transaction?.WasRolledBack ?? true)) transaction.Commit();
        }

        /// <summary>
        /// Commits the current transaction in this session asynchronously if this transaction was not rolled back before.
        /// </summary>
        public async Task CommitAsync()
        {
            if (!(transaction?.WasRolledBack ?? true)) await transaction.CommitAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                if (transaction != null && transaction.IsActive)
                {
                    await TryCommitOrRollbackAsync();
                    Session?.Dispose();
                }

                Dispose(false);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Rollbacks the current transaction in this session
        /// </summary>
        public void Rollback()
        {
            if (!(transaction?.WasCommitted ?? true)) transaction.Rollback();
        }

        /// <summary>
        /// Rollbacks the current transaction in this session asynchronously.
        /// </summary>
        public async Task RollbackAsync()
        {
            if (!(transaction?.WasCommitted ?? true)) await transaction.RollbackAsync();
        }

        /// <summary>
        /// Tries to commit the current transaction in this session if exception occurs rollback transaction and throw the exception.
        /// </summary>
        public void TryCommitOrRollback()
        {
            try
            {
                Commit();
            }
            catch
            {
                Rollback();
                throw;
            }
            finally
            {
                CloseTransaction();
            }
        }

        /// <summary>
        /// Tries to commit the current transaction in this session if exception occurs rollback transaction asynchronously and throw the exception.
        /// </summary>
        public async Task TryCommitOrRollbackAsync()
        {
            try
            {
                await CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                CloseTransaction();
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (transaction != null && transaction.IsActive)
                    {
                        TryCommitOrRollback();
                    }

                    Session?.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}