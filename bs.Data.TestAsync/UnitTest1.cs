using bs.Data.Helpers;
using bs.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace bs.Data.TestAsync
{
    public class UnitTest1
    {
        private IServiceProvider serviceProvider;
        private IServiceCollection services;

        [Fact]
        public async Task Test_SqlServerAsync()
        {
            CreateUnitOfWork_SqlServer();
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var repo = serviceProvider.GetService<BsDataRepository>();

            await uow.RunInTransactionAsync(async () =>
            {
                var country = new CountryModel
                {
                    Name = "Italy"
                };
                await repo.CreateCountryAsync(country);

                var person1 = new PersonModel
                {
                    Name = "Fabio",
                    Lastname = "Cavallari",
                    Age = 40,
                    ContactDate = new DateTime(2020, 9, 2),
                    Description = "Simply me",
                    Photo = new byte[] { 12, 34, 76, 250, 1, 0, 44, 2 }
                };
                await repo.CreatePersonAsync(person1);

                var person1Addresses = new List<AddressModel>();
                person1Addresses.Add(new AddressModel
                {
                    Country = country,
                    PostalCode = "12345",
                    StreetName = "Via liguria, 34/12",
                    Person = person1
                });
                person1Addresses.Add(new AddressModel
                {
                    Country = country,
                    PostalCode = "6789",
                    StreetName = "Corso del popolo, 112",
                    Person = person1
                });
                person1Addresses.ForEach(async a => await repo.CreateAddressAsync(a));
                person1.Addresses = person1Addresses;
                await repo.UpdatePersonAsync(person1);

                var person2 = new PersonModel
                {
                    Name = "Pinco",
                    Lastname = "Pallino",
                    Age = 28,
                    ContactDate = new DateTime(2018, 9, 2),
                    Description = "Simply no one",
                    Photo = new byte[] { 60, 22, 115, 250, 20, 7, 44, 3 }
                };
                await repo.CreatePersonAsync(person2);

                var person2Addresses = new List<AddressModel>();
                person2Addresses.Add(new AddressModel
                {
                    Country = country,
                    PostalCode = "666",
                    StreetName = "Via carlo rosselli, 2",
                    Person = person2
                });
                person2Addresses.Add(new AddressModel
                {
                    Country = country,
                    PostalCode = "321",
                    StreetName = "Corso francia, 1112",
                    Person = person2
                });
                person2Addresses.ForEach(async a => await repo.CreateAddressAsync(a));
                person2.Addresses = person2Addresses;
                await repo.UpdatePersonAsync(person2);

                var rooms = new List<RoomModel>();
                rooms.Add(new RoomModel
                {
                    Name = "RECEPTION",
                    Persons = new PersonModel[] { person1, person2 }
                });
                rooms.Add(new RoomModel
                {
                    Name = "ADMINISTRATION",
                    Persons = new PersonModel[] { person2 }
                });
                rooms.Add(new RoomModel
                {
                    Name = "ICT",
                    Persons = new PersonModel[] { person1 }
                });

                rooms.ForEach(async a => await repo.CreateRoomAsync(a));

                var personsRet = await repo.GetPersonsAsync();
                var addressRet = await repo.GetAddressesAsync();
                var roomsRet = await repo.GetRoomsAsync();
                Assert.NotNull(personsRet.FirstOrDefault());
            });
        }

        private void CreateUnitOfWork_SqlServer()
        {
            var dbContext = new DbContext
            {
                ConnectionString = "Persist Security Info=False;Integrated Security=SSPI; database = OrmTest; server = (local)",
                DatabaseEngineType = DbType.MsSql2012,
                Create = true,
                Update = true,
                LookForEntitiesDllInCurrentDirectoryToo = false,
                SetBatchSize = 25
            };
            services = new ServiceCollection();
            services.AddBsData(dbContext);
            services.AddScoped<BsDataRepository>();
            serviceProvider = services.BuildServiceProvider();
        }
    }
}