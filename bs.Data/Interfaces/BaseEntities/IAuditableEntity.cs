using System;

namespace bs.Data.Interfaces.BaseEntities
{
    /// <summary>
    ///   <para>This interface is used to define 'auditable' (they have a creation and last update date) entities.</para>
    ///   <para>The value of the properties CreationDate and LastUpdateDate are managed in the repository base class directly.</para>
    /// </summary>
    public interface IAuditableEntity : IPersistentEntity
    {
        /// <summary>Gets or sets the creation date.</summary>
        /// <value>The creation date.</value>
        DateTime? CreationDate { get; set; }

        /// <summary>Gets or sets the last update date.</summary>
        /// <value>The last update date.</value>
        DateTime? LastUpdateDate { get; set; }
    }
}