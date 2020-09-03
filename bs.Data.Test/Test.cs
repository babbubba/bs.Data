using bs.Data.Helpers;
using bs.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bs.Data.Test
{
    [TestClass]
    public class Test
    {
        private IServiceProvider serviceProvider;
        private IServiceCollection services;

        [TestInitialize]
        public void Init()
        {
            services = new ServiceCollection();
        }

        #region Sqlite

        [TestMethod]
        public void Test_SqlServer()
        {
            CreateUnitOfWork_SqlServer();
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var repo = serviceProvider.GetService<BsDataRepository>();


            uow.RunInTransaction(() =>
            {
                var country = new CountryModel
                {
                    Name = "Italy"
                };
                repo.CreateCountry(country);


                var person1 = new PersonModel
                {
                    Name = "Fabio",
                    Lastname = "Cavallari",
                    Age = 40,
                    ContactDate = new DateTime(2020, 9, 2),
                    Description = "Simply me",
                    Photo = new byte[] { 12, 34, 76, 250, 1, 0, 44, 2 }
                };
                repo.CreatePerson(person1);



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
                person1Addresses.ForEach(a => repo.CreateAddress(a));
                person1.Addresses = person1Addresses;
                repo.UpdatePerson(person1);

                var person2 = new PersonModel
                {
                    Name = "Pinco",
                    Lastname = "Pallino",
                    Age = 28,
                    ContactDate = new DateTime(2018, 9, 2),
                    Description = "Simply no one",
                    Photo = new byte[] { 60, 22, 115, 250, 20, 7, 44, 3 }
                };
                repo.CreatePerson(person2);

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
                person2Addresses.ForEach(a => repo.CreateAddress(a));
                person2.Addresses = person2Addresses;
                repo.UpdatePerson(person2);

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

                rooms.ForEach(a => repo.CreateRoom(a));

            });

            var persons = repo.GetPersons();
            var address = repo.GetAddresses();
            var rooms = repo.GetRooms();

            Assert.IsNotNull(persons.FirstOrDefault().Id);
        }

   

        #endregion Sqlite

        private void CreateUnitOfWork_Sqlite()
        {
            var dbContext = new DbContext
            {
                ConnectionString = "Data Source=.\\bs.Data.Test.db;Version=3;BinaryGuid=False;",
                DatabaseEngineType = DbType.SQLite,
                Create = false,
                Update = true,
                LookForEntitiesDllInCurrentDirectoryToo = false,
                SetBatchSize = 25
            };

            services.AddBsData(dbContext);
            services.AddScoped<BsDataRepository>();
            serviceProvider = services.BuildServiceProvider();
        }

        private void CreateUnitOfWork_Mysql()
        {
            string server_ip = "localhost";
            string server_port = "3307";
            string database_name = "bsdatadbtest";
            string db_user_name = "root";
            string db_user_password = "xxx";
            var dbContext = new DbContext
            {
                ConnectionString = $"Server={server_ip};Port={server_port};Database={database_name};Uid={db_user_name};Pwd={db_user_password};SslMode=none",
                DatabaseEngineType = DbType.MySQL,
                Create = true,
                Update = true,
                LookForEntitiesDllInCurrentDirectoryToo = false,
                SetBatchSize = 25
            };

            services.AddBsData(dbContext);
            services.AddScoped<BsDataRepository>();
            serviceProvider = services.BuildServiceProvider();
        }

        private void CreateUnitOfWork_PostgreeSql()
        {
            var dbContext = new DbContext
            {
                ConnectionString = "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=bsDataTestDb;Pooling=true;",
                DatabaseEngineType = DbType.PostgreSQL,
                Create = true,
                Update = true,
                LookForEntitiesDllInCurrentDirectoryToo = false,
                SetBatchSize = 25
            };

            services.AddBsData(dbContext);
            services.AddScoped<BsDataRepository>();
            serviceProvider = services.BuildServiceProvider();
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

            services.AddBsData(dbContext);
            services.AddScoped<BsDataRepository>();
            serviceProvider = services.BuildServiceProvider();
        }
    }
}