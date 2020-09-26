using bs.Data.Interfaces.BaseEntities;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;

namespace bs.Data.TestAsync
{
    public class CountryModel : IPersistentEntity
    {
        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }
    }

    public class CountryModelMap : ClassMapping<CountryModel>
    {
        public CountryModelMap()
        {
            Table("Countries");

            Id(x => x.Id, x =>
            {
                x.Generator(Generators.Guid);
                x.Type(NHibernateUtil.Guid);
                x.Column("Id");
                x.UnsavedValue(Guid.Empty);
            });

            Property(b => b.Name, map => map.Length(50));
        }
    }
}