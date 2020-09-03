using bs.Data.Interfaces;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bs.Data.Test
{
    public class BsDataRepository : Repository
    {
        public BsDataRepository(IUnitOfWork unitOfwork) : base(unitOfwork)
        {
        }

        public PersonModel[] GetPersons()
        {
            return Query<PersonModel>().ToArray();
        }


        public void CreatePerson(PersonModel entity)
        {
            Create(entity);
        }
  
        public void CreateAddress(AddressModel entity)
        {
            Create(entity);
        }
      
        public void CreateCountry(CountryModel entity)
        {
            Create(entity);
        }

        public AddressModel[] GetAddresses()
        {
            return Query<AddressModel>().ToArray();
        }


        public void UpdatePerson(PersonModel entity)
        {
            Update(entity);
        }


        public void CreateRoom(RoomModel entity)
        {
            Create(entity);
        }


        public RoomModel[] GetRooms()
        {
            return Query<RoomModel>().ToArray();
        }
    }
}