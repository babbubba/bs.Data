using bs.Data.Interfaces.BaseEntities;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;

namespace bs.Data.Test
{
    public class PersonModel : IPersistentEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Lastname { get; set; }
        public virtual string Description { get; set; }
        public virtual int Age { get; set; }
        public virtual DateTime ContactDate { get; set; }
        public virtual byte[] Photo { get; set; }
        public virtual IEnumerable<AddressModel> Addresses { get; set; }
        public virtual IEnumerable<RoomModel> Rooms { get; set; }
    }

    public class PersonModelMap : ClassMapping<PersonModel>
    {
        public PersonModelMap()
        {
            Table("Persons");

            Id(x => x.Id, x =>
            {
                x.Generator(Generators.Guid);
                x.Type(NHibernateUtil.Guid);
                x.Column("Id");
                x.UnsavedValue(Guid.Empty);
            });
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

            Bag(x => x.Addresses, m =>
            {
                m.Inverse(true);
                m.Key(km => km.Column("PersonId"));
            },
            map => map.OneToMany(a => a.Class(typeof(AddressModel))));

            Bag(x => x.Rooms, m =>
            {
                m.Table("PersonsRooms");
                m.Key(k => k.Column("PersonId"));
            },
            map => map.ManyToMany(p =>
            {
                p.Class(typeof(RoomModel));
                p.Column("RoomId");
            }));
        }
    }
}