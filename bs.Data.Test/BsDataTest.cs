using System;
using bs.Data.Interfaces;
using FluentNHibernate.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace bs.Data.Test
{
    [TestClass]
    public class BsDataTest
    {
        [TestMethod]
        public void TestUnitOfWork()
        {
            IUnitOfWork uOW = CreateUnitOfWork();
            uOW.BeginTransaction();
            uOW.Commit();
            uOW.Dispose();
        }

        [TestMethod]
        public void TestRepository()
        {
            IUnitOfWork uOW = CreateUnitOfWork();
            IRepository repository = new TestRepository(uOW);

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

        private static IUnitOfWork CreateUnitOfWork()
        {
            var dbContext = new DbContext
            {
                ConnectionString = "Data Source=.\\bs.Data.Test.db;Version=3;BinaryGuid=False;",
                Create = true,
                Update = true,
                LookForEntitiesDllInCurrentDirectoryToo = true
            };
            var uOW = new UnitOfWork(dbContext);
            return uOW;
        }
    }
  
}
