using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces
{
    /// <summary>
    /// This wrap the ORM session's transaction
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        Guid Id { get; }
        /// <summary>
        /// Gets the parent uow.
        /// </summary>
        /// <value>
        /// The parent uow.
        /// </value>
        IUnitOfWork ParentUow { get; }
    }
}
