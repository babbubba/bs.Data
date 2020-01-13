using bs.Data.Interfaces;
using NHibernate;
using System;
using System.Collections.Generic;

namespace bs.Data
{
    /// <summary>
    /// The Unit Of Work used for transactional access to DB trough Repository
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.IUnitOfWork" />
    /// <seealso cref="System.IDisposable" />
    public sealed class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// The transactions dictionary
        /// </summary>
        private readonly Dictionary<Guid, NHibernate.ITransaction> transactions;

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// The session factory
        /// </summary>
        private ISessionFactory sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public UnitOfWork(IDbContext dbContext)
        {
            sessionFactory = SessionFactoryBuilder.BuildSessionFactory(dbContext);
            Session = sessionFactory.OpenSession();
            transactions = new Dictionary<Guid, NHibernate.ITransaction>();
        }

        /// <summary>
        /// Gets or sets the ORM session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        public ISession Session { get; set; }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns></returns>
        public Interfaces.ITransaction BeginTransaction()
        {
            var result = new Transaction(this);
            var newTransaction = Session.BeginTransaction();
            transactions.Add(result.Id, newTransaction);
            return result;
        }

        /// <summary>
        /// Commits the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public void Commit(Interfaces.ITransaction transaction)
        {
            var existingTransaction = GetTransaction(transaction.Id);
            if (existingTransaction != null && existingTransaction.IsActive)
            {
                existingTransaction.Commit();
                existingTransaction.Dispose();
                transactions.Remove(transaction.Id);
            }
        }

        /// <summary>
        /// Rollbacks the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public void Rollback(Interfaces.ITransaction transaction)
        {
            var existingTransaction = GetTransaction(transaction.Id);
            if (existingTransaction != null && existingTransaction.IsActive)
            {
                existingTransaction.Rollback();
                existingTransaction.Dispose();
                transactions.Remove(transaction.Id);
            }
        }

        /// <summary>
        /// Tries the transaction's commit or rollback.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <exception cref="Exception">Error during data commit, transaction has been rolled back. See the inner exception for details.</exception>
        public void TryCommitOrRollback(Interfaces.ITransaction transaction)
        {
            try
            {
                Commit(transaction);
            }
            catch (Exception ex)
            {
                Rollback(transaction);
                throw new Exception("Error during data commit, transaction has been rolled back. See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Gets the transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        /// <exception cref="Exception">This transaction not exists in this Unit Of Work context.</exception>
        private NHibernate.ITransaction GetTransaction(Guid transactionId)
        {
            if (!transactions.ContainsKey(transactionId)) throw new Exception("This transaction not exists in this Unit Of Work context.");
            return transactions[transactionId];
        }

        #region IDisposable Support

        /// <summary>
        /// Finalizes an instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var transaction in transactions)
                    {
                        if (transaction.Value != null && transaction.Value.IsActive) transaction.Value.Rollback();
                    }
                    if (Session != null && Session.IsOpen) Session.Close();
                }

                if (Session != null) Session.Dispose();
                Session = null;
                sessionFactory = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}