using System;
using bs.Data.Interfaces;
using FluentNHibernate.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace bs.Data.Test
{
    [TestClass]
    public class BsDataTest
    {
        #region Sqlite
        [TestMethod]
        public void TestUnitOfWork_Sqlite()
        {
            IUnitOfWork uOW = CreateUnitOfWork_Sqlite();
            var t =uOW.BeginTransaction();
            uOW.Commit(t);
            uOW.Dispose();
        }

        [TestMethod]
        public void TestRepositoryEntities_Sqlite()
        {
            IUnitOfWork uOW = CreateUnitOfWork_Sqlite();
            var repository = new TestRepository(uOW);

            #region Create Entity
            var entityToCreate = new TestEntityModel
            {
                DateTimeValue = DateTime.Now,
                IntValue = 1,
                StringValue = "Test"
            };
            using (var transaction = uOW.BeginTransaction())
            {
                repository.Create<TestEntityModel>(entityToCreate);
            }

            #endregion

            #region Retrieve Entity
            var entity = repository.GetById<TestEntityModel>(entityToCreate.Id);

            Assert.IsNotNull(entity);
            Assert.IsInstanceOfType(entity, typeof(TestEntityModel));
            #endregion

            #region Update Entity
            using (var transaction = uOW.BeginTransaction())
            {
                entity.IntValue = 2;
                entity.StringValue = "edited";

                repository.Update(entity);
            }
            #endregion

            #region Delete Entity
            using (var transaction = uOW.BeginTransaction())
            {
                repository.Delete<TestEntityModel>(entity.Id);
            }

            var entityAfterDelete = repository.GetById<TestEntityModel>(entity.Id);
            Assert.IsNull(entityAfterDelete);
            #endregion

            uOW.Dispose();
        }

        [TestMethod]
        public void TestRepositoryAuditableEntities_Sqlite()
        {
            IUnitOfWork uOW = CreateUnitOfWork_Sqlite();
            var repository = new TestRepository(uOW);

            #region Create Entity
            var entityToCreate = new TestAuditableEntityModel
            {
                DateTimeValue = DateTime.Now,
                IntValue = 1,
                StringValue = "Test"
            };
            using (var transaction = uOW.BeginTransaction())
            {
                repository.Create<TestAuditableEntityModel>(entityToCreate);
            }

            #endregion

            #region Retrieve Entity
            var entity = repository.GetById<TestAuditableEntityModel>(entityToCreate.Id);

            Assert.IsNotNull(entity);
            Assert.IsInstanceOfType(entity, typeof(TestAuditableEntityModel));
            #endregion

            #region Update Entity
            using (var transaction = uOW.BeginTransaction())
            {
                entity.IntValue = 2;
                entity.StringValue = "edited";

                repository.Update(entity);
            }
            #endregion

            #region Delete Entity
            using (var transaction = uOW.BeginTransaction())
            {
                repository.Delete<TestAuditableEntityModel>(entity.Id);
            }

            var entityAfterDelete = repository.GetById<TestAuditableEntityModel>(entity.Id);
            Assert.IsNull(entityAfterDelete);
            #endregion

            uOW.Dispose();
        }
        #endregion

        #region MySql
        [TestMethod]
        public void TestUnitOfWork_Mysql()
        {
            IUnitOfWork uOW = CreateUnitOfWork_Mysql();
            uOW.BeginTransaction();
            uOW.Commit();
            uOW.Dispose();
        }

        [TestMethod]
        public void TestRepositoryEntities_Mysql()
        {
            IUnitOfWork uOW = CreateUnitOfWork_Mysql();
            var repository = new TestRepository(uOW);

            #region Create Entity
            uOW.BeginTransaction();
            var entityToCreate = new TestEntityModel
            {
                DateTimeValue = DateTime.Now,
                IntValue = 1,
                StringValue = "Test"
            };
            repository.Create<TestEntityModel>(entityToCreate);
            uOW.Commit();


            #endregion

            #region Retrieve Entity
            var entity = repository.GetById<TestEntityModel>(entityToCreate.Id);

            Assert.IsNotNull(entity);
            Assert.IsInstanceOfType(entity, typeof(TestEntityModel));
            #endregion

            #region Update Entity
            uOW.BeginTransaction();
            entity.IntValue = 2;
            entity.StringValue = "edited";

            repository.Update(entity);
            uOW.Commit();
            #endregion

            #region Delete Entity
            uOW.BeginTransaction();
            repository.Delete<TestEntityModel>(entity.Id);
            uOW.Commit();

            var entityAfterDelete = repository.GetById<TestEntityModel>(entity.Id);
            Assert.IsNull(entityAfterDelete);
            #endregion

            uOW.Dispose();
        }

        [TestMethod]
        public void TestRepositoryAuditableEntities_Mysql()
        {
            IUnitOfWork uOW = CreateUnitOfWork_Mysql();
            var repository = new TestRepository(uOW);

            #region Create Entity
            uOW.BeginTransaction();
            var entityToCreate = new TestAuditableEntityModel
            {
                DateTimeValue = DateTime.Now,
                IntValue = 1,
                StringValue = "Test"
            };
            repository.Create<TestAuditableEntityModel>(entityToCreate);
            uOW.Commit();


            #endregion

            #region Retrieve Entity
            var entity = repository.GetById<TestAuditableEntityModel>(entityToCreate.Id);

            Assert.IsNotNull(entity);
            Assert.IsInstanceOfType(entity, typeof(TestAuditableEntityModel));
            #endregion

            #region Update Entity
            uOW.BeginTransaction();
            entity.IntValue = 2;
            entity.StringValue = "edited";

            repository.Update(entity);
            uOW.Commit();
            #endregion

            #region Delete Entity
            uOW.BeginTransaction();
            repository.Delete<TestAuditableEntityModel>(entity.Id);
            uOW.Commit();

            var entityAfterDelete = repository.GetById<TestAuditableEntityModel>(entity.Id);
            Assert.IsNull(entityAfterDelete);
            #endregion

            uOW.Dispose();
        }

        #endregion
        private static IUnitOfWork CreateUnitOfWork_Sqlite()
        {
            var dbContext = new DbContext
            {
                ConnectionString = "Data Source=.\\bs.Data.Test.db;Version=3;BinaryGuid=False;",
                DatabaseEngineType = "sqlite",
                Create = true,
                Update = true,
                //LookForEntitiesDllInCurrentDirectoryToo = true,
                //EntitiesFileNameScannerPatterns = new string[] { "bs.Data.*.dll" },
                UseExecutingAssemblyToo = true
            };
            var uOW = new UnitOfWork(dbContext);
            return uOW;
        }

        private static IUnitOfWork CreateUnitOfWork_Mysql()
        {
            string server_ip = "localhost";
            string server_port = "3307";
            string database_name = "bsdatadbtest";
            string db_user_name = "root";
            string db_user_password = "beibub1";
            var dbContext = new DbContext
            {
                ConnectionString = $"Server={server_ip};Port={server_port};Database={database_name};Uid={db_user_name};Pwd={db_user_password};SslMode=none",
                DatabaseEngineType = "mysql",
                Create = true,
                Update = true,
                LookForEntitiesDllInCurrentDirectoryToo = true
            };
            var uOW = new UnitOfWork(dbContext);
            return uOW;
        }
    }
  
}
