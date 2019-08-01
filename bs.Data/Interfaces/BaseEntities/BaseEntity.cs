using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces.BaseEntities
{
    public abstract class BaseEntity : IPersistentEntity
    {
        public virtual Guid Id { get; set; }
    }

    public class BaseEntityMap : ClassMap<BaseEntity>
    {
        public BaseEntityMap()
        {
            Id(x => x.Id).GeneratedBy.GuidComb();
        }
    }
}
