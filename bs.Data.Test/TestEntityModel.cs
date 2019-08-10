using System;
using bs.Data.Interfaces.BaseEntities;
using FluentNHibernate.Mapping;

namespace bs.Data.Test
{
    //public class TestEntityModel : IPersistentEntity
    public class TestEntityModel : BaseEntity
    {
        public virtual string StringValue { get; set ; }
        public virtual int IntValue { get; set ; }
        public virtual DateTime DateTimeValue { get; set ; }
    }

    class TestEntityModelMap : SubclassMap<TestEntityModel>
    {
        public TestEntityModelMap()
        {
            // indicates that the base class is abstract
            Abstract();

            Table("TestEntity");
            Map(x => x.StringValue);
            Map(x => x.IntValue);
            Map(x => x.DateTimeValue);
        }
    }
}
