using bs.Data.Interfaces.BaseEntities;
using bs.Data.Mapping;
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

    public class AddressModelMap : BsClassMapping<AddressModel>
    {
        public AddressModelMap()
        {
            Table("Addresses");
            GuidId(x => x.Id);
            SetManyToOne(x => x.Person, "PersonId", "FK__Addresses_Person");
            PropertyText(b => b.StreetName, 90);
            PropertyText(b => b.PostalCode,10);
            SetManyToOne(x => x.Country, "CountryId", "FK_Addresses_Country");
        }
    }
}