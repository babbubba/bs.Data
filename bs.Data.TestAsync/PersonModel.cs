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
            Property(b => b.Name, map => map.Length(25));
            Property(b => b.Lastname, map => map.Length(25));
            Property(b => b.Age);
            Property(b => b.ContactDate);
            Property(b => b.Description, x =>
            {
                x.Length(500);
                x.Type(NHibernateUtil.StringClob);
                x.NotNullable(true);
            });
            Property(b => b.Photo, x =>
            {
                x.Type(NHibernateUtil.BinaryBlob);
            });
            SetOneToMany(p => p.Addresses, "PersonId", typeof(AddressModel));
            SetManyToMany(p => p.Rooms, "PersonsRoomsLink", "PersonId", "RoomId", typeof(RoomModel), false);
            Property(b => b.IsDeleted);
            Property(b => b.DeletionDate);
        }
    }
}