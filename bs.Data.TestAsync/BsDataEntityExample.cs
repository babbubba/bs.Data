using bs.Data.Interfaces;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.TestAsync
{
    public class BsDataEntityExample : IPersistentEntity
    {
        public virtual Guid Id { get; set; }
        public virtual int MyIntProperty { get; set; }
        public virtual decimal MyDecimalProperty { get; set; }
        public virtual double MyDoubleProperty { get; set; }
        public virtual long MyLongProperty { get; set; }
        public virtual Guid MyGuidProperty { get; set; }
        public virtual string MyStringProperty { get; set; }
        public virtual bool MyBoolProperty { get; set; }
        public virtual byte[] MyBlobProperty { get; set; }
    }

    public class BsDataEntityExampleMap : ClassMapping<BsDataEntityExample>
    {

        public BsDataEntityExampleMap()
        {
            Table("EntityExamples");

            Id(x => x.Id, x =>
            {
                x.Generator(Generators.Guid);
                x.Type(NHibernateUtil.Guid);
                x.Column("Id");
                x.UnsavedValue(Guid.Empty);
            });

            Property(b => b.MyIntProperty);
            Property(b => b.MyDecimalProperty);
            Property(b => b.MyDoubleProperty);
            Property(b => b.MyLongProperty);
            Property(b => b.MyGuidProperty);
            Property(b => b.MyStringProperty, x =>
            {
                x.Length(50);
                x.Type(NHibernateUtil.StringClob);
                x.NotNullable(true);
            });
            Property(b => b.MyBoolProperty);
            Property(b => b.MyBlobProperty, x =>
            {
                x.Type(NHibernateUtil.BinaryBlob);
            });
        }
    }

}
