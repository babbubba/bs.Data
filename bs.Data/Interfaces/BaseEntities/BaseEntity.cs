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
            // indicates that this class is the base
            // one for the TPC inheritance strategy and that 
            // the values of its properties should
            // be united with the values of derived classes
            UseUnionSubclassForInheritanceMapping();

            Id(x => x.Id).GeneratedBy.GuidComb();
        }
    }
}
