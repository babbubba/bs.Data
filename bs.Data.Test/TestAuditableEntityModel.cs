using System;
using bs.Data.Interfaces.BaseEntities;
using FluentNHibernate.Mapping;

namespace bs.Data.Test
{
    public class TestAuditableEntityModel : BaseEntity, IAuditableEntity
    {
        public virtual string StringValue { get; set; }
        public virtual int IntValue { get; set; }
        public virtual DateTime DateTimeValue { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }
    }

    class TestAuditableEntityModelMap : SubclassMap<TestAuditableEntityModel>
    {
        public TestAuditableEntityModelMap()
        {
            Table("TestAuditableEntity");
            Map(x => x.StringValue);
            Map(x => x.IntValue);
            Map(x => x.DateTimeValue);
            Map(x => x.CreationDate);
            Map(x => x.LastUpdateDate);
        }
    }
}
