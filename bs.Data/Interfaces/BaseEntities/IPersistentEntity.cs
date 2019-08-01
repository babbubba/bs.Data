using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces.BaseEntities
{
    public interface IPersistentEntity
    {
        Guid Id { get; set; }
    }
}
