using bs.Data.Interfaces;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;

namespace bs.Data.Test
{
    public class RoomModel : IPersistentEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual IEnumerable<PersonModel> Persons { get; set; }

    }

    public class RoomModelMap : ClassMapping<RoomModel>
    {
        public RoomModelMap()
        {
            Table("Rooms");

            Id(x => x.Id, x =>
            {
                x.Generator(Generators.Guid);
                x.Type(NHibernateUtil.Guid);
                x.Column("Id");
                x.UnsavedValue(Guid.Empty);
            });
            Property(b => b.Name, map => map.Length(25));

            Bag(x => x.Persons, m =>
             {
                 m.Table("PersonsRooms");
                 m.Key(k => k.Column("RoomId"));
             },
             map=> map.ManyToMany(p=> 
             {
                 p.Class(typeof(PersonModel));
                 p.Column("PersonId");
             }));
        }
    }
}