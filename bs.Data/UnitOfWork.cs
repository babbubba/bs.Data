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
    public sealed class UnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        private ITransaction _transaction;

        public UnitOfWork(ISession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public ISession Session { get; }
        public bool TransactionIsNotNull => _transaction is not null;

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <exception cref="ORMException">Thrown when an active transaction already exists.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the UnitOfWork has been disposed.</exception>
        public void BeginTransaction()
        {
            ThrowIfDisposed();

            if (_transaction?.IsActive == true)
                throw new ORMException("An active transaction already exists. Close it before creating a new one.");

            _transaction = Session.BeginTransaction();
        }

        /// <summary>
        /// Closes the current transaction if it exists.
        /// </summary>
        public void CloseTransaction()
        {
            _transaction?.Dispose();
            _transaction = null;
        }

        /// <summary>
        /// Commits the current transaction if it exists and wasn't rolled back.
        /// </summary>
        public void Commit()
        {
            ThrowIfDisposed();

            if (_transaction != null && !_transaction.WasRolledBack)
            {
                _transaction.Commit();
            }
        }

        /// <summary>
        /// Commits the current transaction asynchronously if it exists and wasn't rolled back.
        /// </summary>
        public async Task CommitAsync()
        {
            ThrowIfDisposed();

            if (_transaction != null && !_transaction.WasRolledBack)
            {
                await _transaction.CommitAsync();
            }
        }

        /// <summary>
        /// Rolls back the current transaction if it exists and wasn't committed.
        /// </summary>
        public void Rollback()
        {
            ThrowIfDisposed();

            if (_transaction != null && !_transaction.WasCommitted)
            {
                _transaction.Rollback();
            }
        }

        /// <summary>
        /// Rolls back the current transaction asynchronously if it exists and wasn't committed.
        /// </summary>
        public async Task RollbackAsync()
        {
            ThrowIfDisposed();

            if (_transaction != null && !_transaction.WasCommitted)
            {
                await _transaction.RollbackAsync();
            }
        }

        /// <summary>
        /// Attempts to commit the transaction. Rolls back on exception.
        /// </summary>
        public void TryCommitOrRollback()
        {
            ThrowIfDisposed();

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
        /// Attempts to commit the transaction asynchronously. Rolls back on exception.
        /// </summary>
        public async Task TryCommitOrRollbackAsync()
        {
            ThrowIfDisposed();

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

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // A transaction still active at Dispose() time means the caller's own commit/
                // rollback logic never ran - typically an unhandled exception, or a return path
                // that skipped it. Roll back rather than trying to commit whatever was pending, so
                // an abandoned unit of work never silently persists partial/incomplete work. This
                // matches how a bare NHibernate ITransaction (and most ADO.NET/ORM transaction
                // types) already behave when disposed without an explicit Commit().
                if (_transaction?.IsActive == true)
                {
                    Rollback();
                    CloseTransaction();
                }
            }
            finally
            {
                Session?.Dispose();
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            try
            {
                if (_transaction?.IsActive == true)
                {
                    await RollbackAsync();
                    CloseTransaction();
                }
            }
            finally
            {
                Session?.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Flushes pending changes to the database (without committing) and clears the session
        /// first-level cache. Use this in batch loops to avoid unbounded memory growth.
        /// </summary>
        public void FlushAndClear()
        {
            ThrowIfDisposed();
            Session.Flush();
            Session.Clear();
        }

        /// <summary>
        /// Flushes pending changes to the database asynchronously (without committing) and clears
        /// the session first-level cache. Use this in batch loops to avoid unbounded memory growth.
        /// </summary>
        public async Task FlushAndClearAsync()
        {
            ThrowIfDisposed();
            await Session.FlushAsync();
            Session.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));
        }
    }
}