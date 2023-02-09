using bs.Data.Interfaces.BaseEntities;
using bs.Data.Mapping;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;

namespace bs.Data.TestAsync
{
    public class PersonModel : IPersistentEntity, ILogicallyDeletableEntity
    {
        public PersonModel()
        {
            Addresses = new List<AddressModel>();
        }
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Lastname { get; set; }
        public virtual string Description { get; set; }
        public virtual int Age { get; set; }
        public virtual DateTime ContactDate { get; set; }
        public virtual byte[] Photo { get; set; }
        public virtual ICollection<AddressModel> Addresses { get; set; }
        public virtual ICollection<RoomModel> Rooms { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletionDate { get; set; }
    }

    public class PersonModelMap : BsClassMapping<PersonModel>
    {
        public PersonModelMap()
        {
            Table("Persons");
            GuidId(x => x.Id);
            PropertyText(b => b.Name);
            Property(b => b.Lastname);
            Property(b => b.Age);
            PropertyUtcDate(b => b.ContactDate);
            PropertyLongText(b => b.Description, 500);
            PropertyBlob(b => b.Photo);
            SetOneToMany(p => p.Addresses, "PersonId", typeof(AddressModel), pm=> 
            {
                pm.Fetch(CollectionFetchMode.Join);
            });
            SetManyToMany(p => p.Rooms, "PersonsRoomsLink", "PersonId", "RoomId", typeof(RoomModel), false);
            Property(b => b.IsDeleted);
            Property(b => b.DeletionDate);
        }
    }
}