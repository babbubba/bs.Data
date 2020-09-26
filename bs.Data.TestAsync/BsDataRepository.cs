using bs.Data.Interfaces;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bs.Data.TestAsync
{
    public class BsDataRepository : Repository
    {
        public BsDataRepository(IUnitOfWork unitOfwork) : base(unitOfwork)
        {
        }

        public async Task<List<PersonModel>> GetPersonsAsync()
        {
            return await Query<PersonModel>().ToListAsync();
        }

        public async Task CreatePersonAsync(PersonModel entity)
        {
            await CreateAsync(entity);
        }

        public async Task CreateAddressAsync(AddressModel entity)
        {
            await CreateAsync(entity);
        }

        public void CreateCountry(CountryModel entity)
        {
            Create(entity);
        }

        public async Task CreateCountryAsync(CountryModel entity)
        {
            await CreateAsync(entity);
        }

        public async Task<List<AddressModel>> GetAddressesAsync()
        {
            return await Query<AddressModel>().ToListAsync();
        }

        public async Task UpdatePersonAsync(PersonModel entity)
        {
            await UpdateAsync(entity);
        }

        public async Task CreateRoomAsync(RoomModel entity)
        {
            await CreateAsync(entity);
        }

        public async Task<List<RoomModel>> GetRoomsAsync()
        {
            return await Query<RoomModel>().ToListAsync();
        }
    }
}