using bs.Data.Helpers;
using bs.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace bs.Data.TestAsync
{
    public class PostgresTests : IClassFixture<PostgresFixture>
    {
        private readonly BsDataRepository _repo;
        private readonly IUnitOfWork _uow;

        public PostgresTests(PostgresFixture fixture)
        {
            _repo = fixture.Repository;
            _uow = fixture.UnitOfWork;
        }

        [Fact]
        public async Task Test_PostgreSqlAsync()
        {
            _uow.BeginTransaction();
            var country = new CountryModel
            {
                Name = "Italy"
            };
            await _repo.CreateCountryAsync(country);

            var person1 = new PersonModel
            {
                Name = "Fabio",
                Lastname = "Cavallari",
                Age = 40,
                ContactDate = new DateTime(2020, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                Description = "Simply me",
                Photo = new byte[] { 12, 34, 76, 250, 1, 0, 44, 2 },
                Tags = new List<string> { "1", "2", "3", "stella" }
            };
            await _repo.CreatePersonAsync(person1);

            var p1a1 = new AddressModel
            {
                Country = country,
                PostalCode = "12345",
                StreetName = "Via liguria, 34/12",
                Person = person1
            };
            await _repo.CreateAddressAsync(p1a1);
            person1.Addresses.Add(p1a1);

            var p1a2 = new AddressModel
            {
                Country = country,
                PostalCode = "6789",
                StreetName = "Corso del popolo, 112",
                Person = person1
            };
            await _repo.CreateAddressAsync(p1a2);
            person1.Addresses.Add(p1a2);

            await _repo.UpdatePersonAsync(person1);

            var person2 = new PersonModel
            {
                Name = "Pinco",
                Lastname = "Pallino",
                Age = 28,
                ContactDate = new DateTime(2018, 7, 2, 0, 0, 0, DateTimeKind.Utc),
                Description = "Simply no one",
                Photo = new byte[] { 60, 22, 115, 250, 20, 7, 44, 3 },
                Tags = new[] { "Tag1 - |", " Tag 2 ", "Tag3" }
            };
            await _repo.CreatePersonAsync(person2);

            var p2a1 = new AddressModel
            {
                Country = country,
                PostalCode = "666",
                StreetName = "Via carlo rosselli, 2",
                Person = person2,
            };
            await _repo.CreateAddressAsync(p2a1);
            person2.Addresses.Add(p2a1);

            var p2a2 = new AddressModel
            {
                Country = country,
                PostalCode = "321",
                StreetName = "Corso francia, 1112",
                Person = person2
            };
            await _repo.CreateAddressAsync(p2a2);
            person2.Addresses.Add(p2a2);

            await _repo.UpdatePersonAsync(person2);

            var rooms = new List<RoomModel>
                {
                    new RoomModel
                    {
                        Name = "RECEPTION",
                        Persons = new PersonModel[] { person1, person2 }
                    },
                    new RoomModel
                    {
                        Name = "ADMINISTRATION",
                        Persons = new PersonModel[] { person2 }
                    },
                    new RoomModel
                    {
                        Name = "ICT",
                        Persons = new PersonModel[] { person1 }
                    }
                };

            rooms.ForEach(async a => await _repo.CreateRoomAsync(a));
            await _uow.TryCommitOrRollbackAsync();

            _uow.BeginTransaction();
            var personsRet = (await _repo.GetPersonsAsync()).ToList();
            _repo.DeletePersonLogically(personsRet.Last());
            await _uow.TryCommitOrRollbackAsync();

            _uow.BeginTransaction();
            var personsRet2 = await _repo.GetPersonsLogicallyNotDeletedAsync();
            Assert.NotNull(personsRet.FirstOrDefault());
            await _uow.TryCommitOrRollbackAsync();
        }

        [Fact]
        public async Task TransactionInterupted()
        {
            await _uow.RunInTransactionAsync(async () =>
            {
                var country = new CountryModel
                {
                    Name = "United Kindom"
                };
                await _repo.CreateCountryAsync(country);

                var person1 = new PersonModel
                {
                    Name = "Gigi",
                    Lastname = "La Trottola",
                    Age = 64,
                    ContactDate = new DateTime(2021, 2, 16, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Simply me",
                    Photo = new byte[] { 12, 34, 76, 250, 1, 0, 44, 2 }
                };
                await _repo.CreatePersonAsync(person1);

                var p1a1 = new AddressModel
                {
                    Country = country,
                    PostalCode = "12345",
                    StreetName = "Via liguria, 34/12",
                    Person = person1
                };
                await _repo.CreateAddressAsync(p1a1);
                person1.Addresses.Add(p1a1);

                var p1a2 = new AddressModel
                {
                    Country = country,
                    PostalCode = "6789",
                    StreetName = "Corso del popolo, 112",
                    Person = person1
                };
                await _repo.CreateAddressAsync(p1a2);
                person1.Addresses.Add(p1a2);

                await _repo.UpdatePersonAsync(person1);

                var person2 = new PersonModel
                {
                    Name = "Pinco",
                    Lastname = "Pallino",
                    Age = 28,
                    ContactDate = new DateTime(2020, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Simply no one",
                    Photo = new byte[] { 60, 22, 115, 250, 20, 7, 44, 3 }
                };
                await _repo.CreatePersonAsync(person2);

                var p2a1 = new AddressModel
                {
                    Country = country,
                    PostalCode = "666",
                    StreetName = "Via carlo rosselli, 2",
                    Person = person2
                };
                await _repo.CreateAddressAsync(p2a1);
                person2.Addresses.Add(p2a1);

                var p2a2 = new AddressModel
                {
                    Country = country,
                    PostalCode = "321",
                    StreetName = "Corso francia, 1112",
                    Person = person2
                };
                await _repo.CreateAddressAsync(p2a2);
                person2.Addresses.Add(p2a2);

                await _repo.UpdatePersonAsync(person2);

                var rooms = new List<RoomModel>
                {
                    new RoomModel
                    {
                        Name = "RECEPTION",
                        Persons = new PersonModel[] { person1, person2 }
                    },
                    new RoomModel
                    {
                        Name = "ADMINISTRATION",
                        Persons = new PersonModel[] { person2 }
                    },
                    new RoomModel
                    {
                        Name = "ICT",
                        Persons = new PersonModel[] { person1 }
                    }
                };

                rooms.ForEach(async a => await _repo.CreateRoomAsync(a));

                // now we emulate a situation were we have to rollback the transaction
                await _uow.RollbackAsync();
            });
        }
    }
}