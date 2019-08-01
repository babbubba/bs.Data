using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces.BaseEntities
{
    /// <summary>This is the base Interface for all entity models. Repositories implementation will accept types derived from this interface only.</summary>
    public interface IPersistentEntity
    {
        /// <summary>Gets or sets the unique identifier (formaly primary key) for this entity.</summary>
        /// <value>The identifier.</value>
        Guid Id { get; set; }
    }
}
