using bs.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data
{

    /// <summary>
    /// Handle a session's ORM transaction. It support IDisposable that auto commit or rollback transaction.
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.ITransaction" />
    public class Transaction : ITransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="uow">The uow.</param>
        public Transaction(IUnitOfWork uow)
        {
            ParentUow = uow;
            Id = Guid.NewGuid();
        }
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; }
        /// <summary>
        /// Gets the parent uow.
        /// </summary>
        /// <value>
        /// The parent uow.
        /// </value>
        public IUnitOfWork ParentUow { get; }

        #region IDisposable Support
        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ParentUow.TryCommitOrRollback(this);
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
