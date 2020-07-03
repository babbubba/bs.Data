using System;

namespace bs.Data.Interfaces.BaseEntities
{
    /// <summary>
    /// This interface is used to define 'Logically deletable' (they have a bool value that indicate if it is deleted or not) entities.
    /// This permit to delete logicaly an entity from a table and to restore it if needed.
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.BaseEntities.IPersistentEntity" />
    public interface ILogicallyDeletableEntity : IPersistentEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the deletion date.
        /// </summary>
        /// <value>
        /// The deletion date.
        /// </value>
        DateTime? DeletionDate { get; set; }
    }
}