//using System;
//using System.Threading.Tasks;
//using bs.Data.Interfaces;
//using bs.Data.Interfaces.BaseEntities;

//namespace bs.Data.Test
//{
//    public class TestAsyncRepository : AsyncRepository
//    {
//        public TestAsyncRepository(IAsyncUnitOfWork unitOfWork) : base(unitOfWork)
//        {
//        }

//        internal new async Task Create<T>(T entityToCreate) where T : IPersistentEntity
//        {
//            await base.Create<T>(entityToCreate);
//        }

//        internal new async Task<T> GetById<T>(Guid id) where T : IPersistentEntity
//        {
//            return await base.GetById<T>(id);
//        }

//        internal new async Task Update<T>(T entity) where T : IPersistentEntity
//        {
//            await base.Update<T>(entity);
//        }

//        internal new async Task Delete<T>(Guid id) where T : IPersistentEntity
//        {
//            await base.Delete<T>(id);
//        }
//    }

//}