using bs.Data.Helpers;
using bs.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace bs.Data.Test
{
    [TestClass]
    public class BsDataTest
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
        public void Test_Sqlite()
        {
            CreateUnitOfWork_Sqlite();
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var repo = serviceProvider.GetService<BsDataRepository>();

            BsDataEntityExample newEntity = null;

            uow.RunInTransaction(() =>
            {
                newEntity = new BsDataEntityExample
                {
                    MyBlobProperty = new byte[] { 10, 18, 1, 0, 46, 0, 251, 0 },
                    MyDecimalProperty = 12.123M,
                    MyDoubleProperty = 12.1234567890123456,
                    MyGuidProperty = Guid.NewGuid(),
                    MyBoolProperty = true,
                    MyIntProperty = 123456,
                    MyLongProperty = 1234567890123456789,
                    MyStringProperty = "entità di prova!"
                };
                repo.CreateEntityExample(newEntity);
            });

            Assert.IsNotNull(newEntity.Id);
        }

        #endregion Sqlite

        private void CreateUnitOfWork_Sqlite()
        {
            var dbContext = new DbContext
            {
                ConnectionString = "Data Source=.\\bs.Data.Test.db;Version=3;BinaryGuid=False;",
                DatabaseEngineType = DbType.SQLite,
                Create = true,
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
    }
}