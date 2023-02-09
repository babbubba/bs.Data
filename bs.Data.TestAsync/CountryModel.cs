using bs.Data.Interfaces.BaseEntities;
using bs.Data.Mapping;
using System;

namespace bs.Data.TestAsync
{
    public class CountryModel : IPersistentEntity
    {
        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }
    }

    public class CountryModelMap : BsClassMapping<CountryModel>
    {
        public CountryModelMap()
        {
            Table("Countries");
            GuidId(x => x.Id);
            PropertyText(b => b.Name, 50);
        }
    }
}