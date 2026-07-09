using bs.Data.Helpers;
using bs.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace bs.Data.TestAsync
{
    [Collection(SqlServerCollection.Name)]
    public class SqlServerTests
    {
        private readonly SqlServerFixture _fixture;
        private readonly BsDataRepository _repo;
        private readonly IUnitOfWork _uow;

        public SqlServerTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
            _repo = fixture.Repository;
            _uow = fixture.UnitOfWork;
        }

        [Fact]
        public async Task Test_SqlServerAsync()
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
                Photo = [12, 34, 76, 250, 1, 0, 44, 2],
                Tags = ["1", "2", "3", "stella"]
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
                Photo = [60, 22, 115, 250, 20, 7, 44, 3],
                Tags = ["Tag1 - |", " Tag 2 ", "Tag3"]
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

            foreach (var room in rooms) await _repo.CreateRoomAsync(room);
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
                    Photo = [12, 34, 76, 250, 1, 0, 44, 2]
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
                    Photo = [60, 22, 115, 250, 20, 7, 44, 3]
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

                foreach (var room in rooms) await _repo.CreateRoomAsync(room);

                // now we emulate a situation were we have to rollback the transaction
                await _uow.RollbackAsync();

                //after we have to exit this method
                return;

                // when disposing the _uow the extension thata usually have to commit or rollback have not to do it...
            });
        }

        /// <summary>
        /// Forces a genuine SQL Server deadlock (error 1205) between two concurrent
        /// <c>RunInTransactionAsync</c> calls and verifies both complete successfully - i.e. that
        /// the deadlock victim is transparently retried instead of the exception surfacing to the
        /// caller. Regression test for the bug where the retry back-off state was a shared
        /// static singleton: once its budget was exhausted by earlier deadlocks anywhere in the
        /// process, retries silently stopped working for good. Running this test more than once
        /// in the same process (e.g. via `dotnet test` re-runs within one session, or by duplicating
        /// the [Fact] below) is exactly the scenario that used to regress.
        /// </summary>
        [Fact]
        public async Task RunInTransactionAsync_RetriesAndSucceeds_OnRealDeadlock()
        {
            // Seed two independent rows that the two concurrent transactions below will lock in
            // opposite order, to force a real circular wait (deadlock) rather than simulating one.
            Guid personAId = default, personBId = default;
            await _uow.RunInTransactionAsync(async () =>
            {
                var personA = new PersonModel { Name = "DeadlockA", Lastname = "Test", Age = 1, ContactDate = DateTime.UtcNow };
                var personB = new PersonModel { Name = "DeadlockB", Lastname = "Test", Age = 1, ContactDate = DateTime.UtcNow };
                await _repo.CreatePersonAsync(personA);
                await _repo.CreatePersonAsync(personB);
                personAId = personA.Id;
                personBId = personB.Id;
                return true;
            });

            // Each side gets its own DI scope, so the two transactions use independent NHibernate
            // sessions/connections - exactly as two concurrent web requests would.
            using var lockedFirstRowSignalA = new SemaphoreSlim(0, 1);
            using var lockedFirstRowSignalB = new SemaphoreSlim(0, 1);

            async Task<int> RunSideAsync(Guid firstRowId, Guid secondRowId, SemaphoreSlim signalOwnLock, SemaphoreSlim waitForOtherLock)
            {
                using var scope = _fixture.ServiceProvider.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var repo = scope.ServiceProvider.GetRequiredService<BsDataRepository>();

                var attempts = 0;
                await uow.RunInTransactionAsync(async () =>
                {
                    attempts++;

                    var first = await repo.GetPersonByIdAsync(firstRowId);
                    first.Age++;
                    await repo.UpdatePersonAsync(first);

                    // Force NHibernate to send the UPDATE (and take SQL Server's row lock) right
                    // now, instead of deferring it to the automatic flush-before-commit. Without
                    // this, both updates in this delegate would only hit the database together at
                    // commit time and the two sides would never actually contend for the same rows.
                    await uow.Session.FlushAsync();

                    // Only hand-shake with the other side on the very first attempt: on a retry the
                    // other side has already finished, so waiting again here would hang forever.
                    if (attempts == 1)
                    {
                        signalOwnLock.Release();
                        await waitForOtherLock.WaitAsync();
                    }

                    var second = await repo.GetPersonByIdAsync(secondRowId);
                    second.Age++;
                    await repo.UpdatePersonAsync(second);
                    // Flushed implicitly by transaction.CommitAsync() below: at that point this
                    // side blocks on the other side's lock on `secondRowId`, symmetrically with the
                    // other side blocking on this side's lock on `firstRowId` - the circular wait
                    // SQL Server's deadlock monitor detects and resolves by killing one side.

                    return true;
                });

                return attempts;
            }

            var sideA = RunSideAsync(personAId, personBId, lockedFirstRowSignalA, lockedFirstRowSignalB);
            var sideB = RunSideAsync(personBId, personAId, lockedFirstRowSignalB, lockedFirstRowSignalA);

            var attemptCounts = await Task.WhenAll(sideA, sideB);

            // Both transactions must ultimately succeed - RunInTransactionAsync must transparently
            // retry whichever side SQL Server picked as the deadlock victim.
            Assert.True(attemptCounts[0] >= 1);
            Assert.True(attemptCounts[1] >= 1);

            // At least one side should have needed more than one attempt; otherwise the two
            // transactions never actually contended for the same locks and the test proved nothing.
            Assert.True(attemptCounts[0] > 1 || attemptCounts[1] > 1);
        }

        //private void CreateUnitOfWork_SqlServer()
        //{
        //    var dbContext = new DbContext
        //    {
        //        ConnectionString = "database = OrmTest; server = 192.168.254.13; user = sa; password = Password01;TrustServerCertificate=true;",
        //        DatabaseEngineType = DbType.MsSql2012,
        //        Create = true,
        //        Update = true,
        //        LookForEntitiesDllInCurrentDirectoryToo = false,
        //        SetBatchSize = 25
        //    };
        //    services = new ServiceCollection();
        //    services.AddBsData(dbContext);
        //    services.AddSingleton<BsDataRepository>();
        //    serviceProvider = services.BuildServiceProvider();
        //}

        //private void CreateUnitOfWork_Postgresql()
        //{
        //    var dbContext = new DbContext
        //    {
        //        //ConnectionString = "User ID=italcom;Password=Password01;Host=ITA-TO-GEL01;Port=5432;Database=ormtest;Pooling=true;Connection Lifetime=0;",
        //        ConnectionString = "User ID=postgres;Password=Password01!;Host=192.168.254.38;Port=5432;Database=ormtest",

        //        DatabaseEngineType = DbType.PostgreSQL83,
        //        Create = true,
        //        Update = true,
        //        LookForEntitiesDllInCurrentDirectoryToo = false,
        //        SetBatchSize = 25
        //    };
        //    services = new ServiceCollection();
        //    services.AddBsData(dbContext);
        //    services.AddSingleton<BsDataRepository>();
        //    serviceProvider = services.BuildServiceProvider();
        //}
    }
}