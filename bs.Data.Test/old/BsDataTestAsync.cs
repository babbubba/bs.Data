//using System;
//using System.Threading.Tasks;
//using bs.Data.Interfaces;
//using FluentNHibernate.Mapping;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace bs.Data.Test
//{
//    [TestClass]
//    public class BsDataTestAsync
//    {
//        #region Sqlite
//        [TestMethod]
//        public void TestUnitOfWork_Sqlite()
//        {
//            Task.Run(async () =>
//            {
//                IAsyncUnitOfWork uOW = CreateUnitOfWork_Sqlite();
//                var t = await uOW.BeginTransaction();
//                await uOW.Commit(t);
//                await uOW.DisposeAsync();
//            }).GetAwaiter().GetResult();
//        }

//        [TestMethod]
//        public void TestRepositoryEntities_Sqlite()
//        {
//            Task.Run(async () =>
//            {
//                IAsyncUnitOfWork uOW = CreateUnitOfWork_Sqlite();
//                var repository = new TestAsyncRepository(uOW);

//                #region Create Entity
//                var entityToCreate = new TestEntityModel
//                {
//                    DateTimeValue = DateTime.Now,
//                    IntValue = 1,
//                    StringValue = "Test"
//                };
//                using (var transaction = uOW.BeginTransaction())
//                {
//                    await repository.Create<TestEntityModel>(entityToCreate);
//                }

//                #endregion

//                #region Retrieve Entity
//                var entity = await repository.GetById<TestEntityModel>(entityToCreate.Id);

//                Assert.IsNotNull(entity);
//                Assert.IsInstanceOfType(entity, typeof(TestEntityModel));
//                #endregion

//                #region Update Entity
//                using (var transaction = uOW.BeginTransaction())
//                {
//                    entity.IntValue = 2;
//                    entity.StringValue = "edited";

//                    await repository.Update(entity);
//                }
//                #endregion

//                #region Delete Entity
//                using (var transaction = uOW.BeginTransaction())
//                {
//                    await repository.Delete<TestEntityModel>(entity.Id);
//                }

//                var entityAfterDelete = await repository.GetById<TestEntityModel>(entity.Id);
//                Assert.IsNull(entityAfterDelete);
//                #endregion

//                await uOW.DisposeAsync();

//            }).GetAwaiter().GetResult();
//        }

//        [TestMethod]
//        public void TestRepositoryAuditableEntities_Sqlite()
//        {
//            Task.Run(async () =>
//            {
//                IAsyncUnitOfWork uOW = CreateUnitOfWork_Sqlite();
//                var repository = new TestAsyncRepository(uOW);

//                #region Create Entity
//                var entityToCreate = new TestAuditableEntityModel
//                {
//                    DateTimeValue = DateTime.Now,
//                    IntValue = 1,
//                    StringValue = "Test"
//                };
//                using (var transaction = uOW.BeginTransaction())
//                {
//                    await repository.Create<TestAuditableEntityModel>(entityToCreate);
//                }

//                #endregion

//                #region Retrieve Entity
//                var entity = await repository.GetById<TestAuditableEntityModel>(entityToCreate.Id);

//                Assert.IsNotNull(entity);
//                Assert.IsInstanceOfType(entity, typeof(TestAuditableEntityModel));
//                #endregion

//                #region Update Entity
//                using (var transaction = uOW.BeginTransaction())
//                {
//                    entity.IntValue = 2;
//                    entity.StringValue = "edited";

//                    await repository.Update(entity);
//                }
//                #endregion

//                #region Delete Entity
//                using (var transaction = uOW.BeginTransaction())
//                {
//                    await repository.Delete<TestAuditableEntityModel>(entity.Id);
//                }

//                var entityAfterDelete = await repository.GetById<TestAuditableEntityModel>(entity.Id);
//                Assert.IsNull(entityAfterDelete);
//                #endregion

//                await uOW.DisposeAsync();

//            }).GetAwaiter().GetResult();
//        }
//        #endregion

//        //#region MySql
//        //[TestMethod]
//        //public void TestUnitOfWork_Mysql()
//        //{
//        //    IUnitOfWork uOW = CreateUnitOfWork_Mysql();
//        //    using (var transaction = uOW.BeginTransaction())
//        //    {
//        //    }
//        //    uOW.Dispose();
//        //}

//        //[TestMethod]
//        //public void TestRepositoryEntities_Mysql()
//        //{
//        //    IUnitOfWork uOW = CreateUnitOfWork_Mysql();
//        //    var repository = new TestRepository(uOW);

//        //    #region Create Entity

//        //    var entityToCreate = new TestEntityModel
//        //    {
//        //        DateTimeValue = DateTime.Now,
//        //        IntValue = 1,
//        //        StringValue = "Test"
//        //    };
//        //    using (var transaction = uOW.BeginTransaction())
//        //    {
//        //        repository.Create<TestEntityModel>(entityToCreate);
//        //    }

//        //    #endregion

//        //    #region Retrieve Entity
//        //    var entity = repository.GetById<TestEntityModel>(entityToCreate.Id);

//        //    Assert.IsNotNull(entity);
//        //    Assert.IsInstanceOfType(entity, typeof(TestEntityModel));
//        //    #endregion

//        //    #region Update Entity
//        //    using (var transaction = uOW.BeginTransaction())
//        //    {
//        //        entity.IntValue = 2;
//        //        entity.StringValue = "edited";

//        //        repository.Update(entity);
//        //    }
//        //    #endregion

//        //    #region Delete Entity
//        //    using (var transaction = uOW.BeginTransaction())
//        //    {
//        //        repository.Delete<TestEntityModel>(entity.Id);
//        //    }

//        //    var entityAfterDelete = repository.GetById<TestEntityModel>(entity.Id);
//        //    Assert.IsNull(entityAfterDelete);
//        //    #endregion

//        //    uOW.Dispose();
//        //}

//        //[TestMethod]
//        //public void TestRepositoryAuditableEntities_Mysql()
//        //{
//        //    IUnitOfWork uOW = CreateUnitOfWork_Mysql();
//        //    var repository = new TestRepository(uOW);

//        //    var entityToCreate = new TestAuditableEntityModel
//        //    {
//        //        DateTimeValue = DateTime.Now,
//        //        IntValue = 1,
//        //        StringValue = "Test"
//        //    };
//        //    #region Create Entity
//        //    using (var transaction = uOW.BeginTransaction())
//        //    {
//        //        repository.Create<TestAuditableEntityModel>(entityToCreate);
//        //    }

//        //    #endregion

//        //    #region Retrieve Entity
//        //    var entity = repository.GetById<TestAuditableEntityModel>(entityToCreate.Id);

//        //    Assert.IsNotNull(entity);
//        //    Assert.IsInstanceOfType(entity, typeof(TestAuditableEntityModel));
//        //    #endregion

//        //    #region Update Entity
//        //    using (var transaction = uOW.BeginTransaction())
//        //    {
//        //        entity.IntValue = 2;
//        //        entity.StringValue = "edited";

//        //        repository.Update(entity);
//        //    }
//        //    #endregion

//        //    #region Delete Entity
//        //    using (var transaction = uOW.BeginTransaction())
//        //    {
//        //        repository.Delete<TestAuditableEntityModel>(entity.Id);
//        //    }

//        //    var entityAfterDelete = repository.GetById<TestAuditableEntityModel>(entity.Id);
//        //    Assert.IsNull(entityAfterDelete);
//        //    #endregion

//        //    uOW.Dispose();
//        //}

//        //#endregion
//        private static IAsyncUnitOfWork CreateUnitOfWork_Sqlite()
//        {
//            var dbContext = new DbContext
//            {
//                ConnectionString = "Data Source=.\\bs.Data.TestAsync.db;Version=3;BinaryGuid=False;",
//                //DatabaseEngineType = "sqlite",
//                DatabaseEngineType = DbType.SQLite,
//                Create = true,
//                Update = true,
//                LookForEntitiesDllInCurrentDirectoryToo = false,

//            };
//            var uOW = new AsyncUnitOfWork(dbContext);
//            return uOW;
//        }

//        private static IAsyncUnitOfWork CreateUnitOfWork_Mysql()
//        {
//            string server_ip = "localhost";
//            string server_port = "3307";
//            string database_name = "bsdatadbtest";
//            string db_user_name = "root";
//            string db_user_password = "xxx";
//            var dbContext = new DbContext
//            {
//                ConnectionString = $"Server={server_ip};Port={server_port};Database={database_name};Uid={db_user_name};Pwd={db_user_password};SslMode=none",
//                //DatabaseEngineType = "mysql",
//                DatabaseEngineType = DbType.MySQL,
//                Create = true,
//                Update = true,
//                UseExecutingAssemblyToo = true
//            };
//            var uOW = new AsyncUnitOfWork(dbContext);
//            return uOW;
//        }
//    }

//}