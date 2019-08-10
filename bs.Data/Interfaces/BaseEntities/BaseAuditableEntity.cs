using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces.BaseEntities
{
    public abstract class BaseAuditableEntity : IAuditableEntity
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual DateTime? LastUpdateDate { get; set; }
    }

    public class BaseAuditableEntityMap : ClassMap<BaseAuditableEntity>
    {
        public BaseAuditableEntityMap()
        {
            // indicates that this class is the base
            // one for the TPC inheritance strategy and that 
            // the values of its properties should
            // be united with the values of derived classes
            UseUnionSubclassForInheritanceMapping();

            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.CreationDate).Nullable();
            Map(x => x.LastUpdateDate).Nullable();

        }
    }
}
