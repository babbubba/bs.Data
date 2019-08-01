using bs.Data.Interfaces;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data
{
    /// <summary>The Unit Of Work used for transactional access to DB trough Repository</summary>
    /// <seealso cref="bs.Data.Interfaces.IUnitOfWork" />
    /// <seealso cref="System.IDisposable" />
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
     
        private bool disposedValue = false;
        private ISessionFactory sessionFactory;
        public ISession Session { get; set; }
        private ITransaction transaction;

        /// <summary>Initializes a new instance of the <see cref="UnitOfWork"/> class.</summary>
        /// <param name="dbContext">The database context.</param>
        public UnitOfWork(IDbContext dbContext)
        {
            sessionFactory = SessionFactoryBuilder.BuildSessionFactory(dbContext);
            Session = sessionFactory.OpenSession();
        }

        /// <summary>Begins the transaction.</summary>
        public void BeginTransaction()
        {
            transaction = Session.BeginTransaction();
        }

        /// <summary>Commits the transacion initialized in this instance.</summary>
        public void Commit()
        {
            try
            {
                // commit transaction if there is one active
                if (transaction != null && transaction.IsActive)
                    transaction.Commit();
            }
            catch
            {
                // rollback if there was an exception
                if (transaction != null && transaction.IsActive)
                    transaction.Rollback();

                throw;
            }
            finally
            {
                //Session.Dispose();
            }
        }

        /// <summary>Rollbacks the transaction intialized in this instance.</summary>
        public void Rollback()
        {
            try
            {
                if (transaction != null && transaction.IsActive)
                    transaction.Rollback();
            }
            finally
            {
                //Session.Dispose();
            }
        }

        #region IDisposable Support

        /// <summary>Releases resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (transaction != null && transaction.IsActive) transaction.Rollback();
                    if (Session != null && Session.IsOpen) Session.Close();
                }

                if (Session != null) Session.Dispose();
                transaction = null;
                Session = null;
                sessionFactory = null;
                disposedValue = true;
            }
        }

        /// <summary>Finalizes an instance of the <see cref="UnitOfWork"/> class.</summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
