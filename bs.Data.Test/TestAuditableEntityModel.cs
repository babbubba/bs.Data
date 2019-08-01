using System;
using bs.Data.Interfaces.BaseEntities;
using FluentNHibernate.Mapping;

namespace bs.Data.Test
{
    public class TestAuditableEntityModel : IAuditableEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string StringValue { get; set; }
        public virtual int IntValue { get; set; }
        public virtual DateTime DateTimeValue { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }
    }

    class TestAuditableEntityModelMap : ClassMap<TestAuditableEntityModel>
    {
        public TestAuditableEntityModelMap()
        {
            Table("TestAuditableEntity");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.StringValue);
            Map(x => x.IntValue);
            Map(x => x.DateTimeValue);
            Map(x => x.CreationDate);
            Map(x => x.LastUpdateDate);
        }
    }
}
