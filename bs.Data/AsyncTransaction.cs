//using bs.Data.Interfaces;
//using System;
//using System.Threading.Tasks;

//namespace bs.Data
//{
//    public class AsyncTransaction : IAsyncTransaction
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="Transaction"/> class.
//        /// </summary>
//        /// <param name="uow">The uow.</param>
//        public AsyncTransaction(IAsyncUnitOfWork uow)
//        {
//            ParentUow = uow;
//            Id = Guid.NewGuid();
//        }
//        /// <summary>
//        /// Gets the identifier.
//        /// </summary>
//        /// <value>
//        /// The identifier.
//        /// </value>
//        public Guid Id { get; }
//        /// <summary>
//        /// Gets the parent uow.
//        /// </summary>
//        /// <value>
//        /// The parent uow.
//        /// </value>
//        public IAsyncUnitOfWork ParentUow { get; }

//        #region IDisposable Support
//        /// <summary>
//        /// The disposed value
//        /// </summary>
//        private bool disposedValue = false;

//        public ValueTask DisposeAsync()
//        {
//            if (!disposedValue)
//            {
//                Dispose();
//                disposedValue = true;
//            }
//            return default;
//        }

//        public async void Dispose()
//        {
//            await ParentUow.TryCommitOrRollback(this);
//        }
//        #endregion
//    }
//}