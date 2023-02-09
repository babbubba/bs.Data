using bs.Data.Interfaces.BaseEntities;
using bs.Data.Mapping;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;

namespace bs.Data.TestAsync
{
    public class RoomModel : IPersistentEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual IEnumerable<PersonModel> Persons { get; set; }
    }

    public class RoomModelMap : BsClassMapping<RoomModel>
    {
        public RoomModelMap()
        {
            Table("Rooms");
            GuidId(x=>x.Id);
            PropertyText(b => b.Name);
            SetManyToMany(p => p.Persons, "PersonsRoomsLink",  "RoomId", "PersonId", typeof(PersonModel), false);
        }
    }
}