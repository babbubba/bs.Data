using bs.Data.Interfaces;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;

namespace bs.Data.TestAsync
{
    public class AddressModel : IPersistentEntity
    {
        public virtual Guid Id { get; set; }
        public virtual PersonModel Person { get; set; }
        public virtual string StreetName { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual CountryModel Country { get; set; }
    }

    public class AddressModelMap : ClassMapping<AddressModel>
    {
        public AddressModelMap()
        {
            Table("Addresses");
            Id(x => x.Id, x =>
            {
                x.Generator(Generators.Guid);
                x.Type(NHibernateUtil.Guid);
                x.Column("Id");
                x.UnsavedValue(Guid.Empty);
            });
            ManyToOne(x => x.Person, map =>
            {
                map.Column("PersonId");
                map.ForeignKey("FK__Addresses_Persons");
            });
            Property(b => b.StreetName, map => map.Length(90));
            Property(b => b.PostalCode, map => map.Length(10));
            ManyToOne(x => x.Country, map =>
            {
                map.Column("CountryId");
                map.ForeignKey("FK__Addresses_Countries");
            });
        }
    }
}