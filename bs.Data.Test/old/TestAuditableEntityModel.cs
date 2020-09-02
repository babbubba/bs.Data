//using bs.Data.Interfaces.BaseEntities;
//using FluentNHibernate.Mapping;
//using System;

//namespace bs.Data.Test
//{
//    public class TestAuditableEntityModel : BaseAuditableEntity
//    {
//        public virtual string StringValue { get; set; }
//        public virtual int IntValue { get; set; }
//        public virtual DateTime DateTimeValue { get; set; }
//    }

//    internal class TestAuditableEntityModelMap : SubclassMap<TestAuditableEntityModel>
//    {
//        public TestAuditableEntityModelMap()
//        {
//            // indicates that the base class is abstract
//            Abstract();

//            Table("TestAuditableEntity");
//            Map(x => x.StringValue);
//            Map(x => x.IntValue);
//            Map(x => x.DateTimeValue);
//        }
//    }
//}