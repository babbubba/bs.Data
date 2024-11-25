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
            CreateUnitOfWork_Postgresql();
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var repo = serviceProvider.GetService<BsDataRepository>();

            uow.BeginTransaction();
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
                ContactDate = new DateTime(2020, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                Description = "Simply me",
                Photo = [12, 34, 76, 250, 1, 0, 44, 2],
                Tags = ["1", "2", "3", "stella"]
            };
            await repo.CreatePersonAsync(person1);

            var p1a1 = new AddressModel
            {
                Country = country,
                PostalCode = "12345",
                StreetName = "Via liguria, 34/12",
                Person = person1
            };
            await repo.CreateAddressAsync(p1a1);
            person1.Addresses.Add(p1a1);

            var p1a2 = new AddressModel
            {
                Country = country,
                PostalCode = "6789",
                StreetName = "Corso del popolo, 112",
                Person = person1
            };
            await repo.CreateAddressAsync(p1a2);
            person1.Addresses.Add(p1a2);

            await repo.UpdatePersonAsync(person1);

            var person2 = new PersonModel
            {
                Name = "Pinco",
                Lastname = "Pallino",
                Age = 28,
                ContactDate = new DateTime(2018, 7, 2, 0, 0, 0, DateTimeKind.Utc),
                Description = "Simply no one",
                Photo = [60, 22, 115, 250, 20, 7, 44, 3],
                Tags = ["Tag1 - |", " Tag 2 ", "Tag3"]
            };
            await repo.CreatePersonAsync(person2);

            var p2a1 = new AddressModel
            {
                Country = country,
                PostalCode = "666",
                StreetName = "Via carlo rosselli, 2",
                Person = person2,
            };
            await repo.CreateAddressAsync(p2a1);
            person2.Addresses.Add(p2a1);

            var p2a2 = new AddressModel
            {
                Country = country,
                PostalCode = "321",
                StreetName = "Corso francia, 1112",
                Person = person2
            };
            await repo.CreateAddressAsync(p2a2);
            person2.Addresses.Add(p2a2);

            await repo.UpdatePersonAsync(person2);

            var rooms = new List<RoomModel>
                {
                    new() {
                        Name = "RECEPTION",
                        Persons = [person1, person2]
                    },
                    new() {
                        Name = "ADMINISTRATION",
                        Persons = [person2]
                    },
                    new() {
                        Name = "ICT",
                        Persons = [person1]
                    }
                };

            rooms.ForEach(async a => await repo.CreateRoomAsync(a));
            await uow.TryCommitOrRollbackAsync();

            uow.BeginTransaction();
            var personsRet = (await repo.GetPersonsAsync()).ToList();
            repo.DeletePersonLogically(personsRet.Last());
            await uow.TryCommitOrRollbackAsync();

            uow.BeginTransaction();
            var personsRet2 = await repo.GetPersonsLogicallyNotDeletedAsync();
            Assert.NotNull(personsRet.FirstOrDefault());
            await uow.TryCommitOrRollbackAsync();
        }

        [Fact]
        public async Task Test_interrupted_transaction_SqlServerAsync()
        {
            CreateUnitOfWork_SqlServer();
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var repo = serviceProvider.GetService<BsDataRepository>();

            await TransactionInterupted(uow, repo);

            // if no exception occurred it worked fine
        }

        private static async Task TransactionInterupted(IUnitOfWork uow, BsDataRepository repo)
        {
            await uow.RunInTransactionAsync(async () =>
            {
                var country = new CountryModel
                {
                    Name = "United Kindom"
                };
                await repo.CreateCountryAsync(country);

                var person1 = new PersonModel
                {
                    Name = "Gigi",
                    Lastname = "La Trottola",
                    Age = 64,
                    ContactDate = new DateTime(2021, 2, 16, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Simply me",
                    Photo = [12, 34, 76, 250, 1, 0, 44, 2]
                };
                await repo.CreatePersonAsync(person1);

                var p1a1 = new AddressModel
                {
                    Country = country,
                    PostalCode = "12345",
                    StreetName = "Via liguria, 34/12",
                    Person = person1
                };
                await repo.CreateAddressAsync(p1a1);
                person1.Addresses.Add(p1a1);

                var p1a2 = new AddressModel
                {
                    Country = country,
                    PostalCode = "6789",
                    StreetName = "Corso del popolo, 112",
                    Person = person1
                };
                await repo.CreateAddressAsync(p1a2);
                person1.Addresses.Add(p1a2);

                await repo.UpdatePersonAsync(person1);

                var person2 = new PersonModel
                {
                    Name = "Pinco",
                    Lastname = "Pallino",
                    Age = 28,
                    ContactDate = new DateTime(2020, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Simply no one",
                    Photo = [60, 22, 115, 250, 20, 7, 44, 3]
                };
                await repo.CreatePersonAsync(person2);

                var p2a1 = new AddressModel
                {
                    Country = country,
                    PostalCode = "666",
                    StreetName = "Via carlo rosselli, 2",
                    Person = person2
                };
                await repo.CreateAddressAsync(p2a1);
                person2.Addresses.Add(p2a1);

                var p2a2 = new AddressModel
                {
                    Country = country,
                    PostalCode = "321",
                    StreetName = "Corso francia, 1112",
                    Person = person2
                };
                await repo.CreateAddressAsync(p2a2);
                person2.Addresses.Add(p2a2);

                await repo.UpdatePersonAsync(person2);

                var rooms = new List<RoomModel>
                {
                    new() {
                        Name = "RECEPTION",
                        Persons = [person1, person2]
                    },
                    new() {
                        Name = "ADMINISTRATION",
                        Persons = [person2]
                    },
                    new() {
                        Name = "ICT",
                        Persons = [person1]
                    }
                };

                rooms.ForEach(async a => await repo.CreateRoomAsync(a));

                // now we emulate a situation were we have to rollback the transaction
                await uow.RollbackAsync();

                //after we have to exit this method
                return;

                // when disposing the uow the extension thata usually have to commit or rollback have not to do it...
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
            services.AddSingleton<BsDataRepository>();
            serviceProvider = services.BuildServiceProvider();
        }

        private void CreateUnitOfWork_Postgresql()
        {
            var dbContext = new DbContext
            {
                //ConnectionString = "User ID=italcom;Password=Password01;Host=ITA-TO-GEL01;Port=5432;Database=ormtest;Pooling=true;Connection Lifetime=0;",
                ConnectionString = "User ID=postgres;Password=Password01!;Host=192.168.254.38;Port=5432;Database=ormtest",

                DatabaseEngineType = DbType.PostgreSQL83,
                Create = true,
                Update = true,
                LookForEntitiesDllInCurrentDirectoryToo = false,
                SetBatchSize = 25
            };
            services = new ServiceCollection();
            services.AddBsData(dbContext);
            services.AddSingleton<BsDataRepository>();
            serviceProvider = services.BuildServiceProvider();
        }
    }
}