//using NHibernate;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace bs.Data.Interfaces
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <seealso cref="System.IDisposable" />
//    public interface IAsyncUnitOfWork : IAsyncDisposable, IDisposable
//    {
//        /// <summary>
//        /// Gets or sets the ORM session.
//        /// </summary>
//        /// <value>
//        /// The session.
//        /// </value>
//        ISession Session { get; set; }
//        /// <summary>
//        /// Begins the transaction.
//        /// </summary>
//        /// <returns></returns>
//        Task<IAsyncTransaction> BeginTransaction();
//        /// <summary>
//        /// Commits the specified transaction.
//        /// </summary>
//        /// <param name="transaction">The transaction.</param>
//        Task Commit(IAsyncTransaction transaction);
//        /// <summary>
//        /// Tries the commit or rollback.
//        /// </summary>
//        /// <param name="transaction">The transaction.</param>
//        Task TryCommitOrRollback(IAsyncTransaction transaction);
//        /// <summary>
//        /// Rollbacks the specified transaction.
//        /// </summary>
//        /// <param name="transaction">The transaction.</param>
//        Task Rollback(IAsyncTransaction transaction);
//    }
//}
