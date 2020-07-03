using bs.Data.Interfaces;
using bs.Data.Interfaces.BaseEntities;
using System;

namespace bs.Data.Test
{
    public class TestRepository : Repository
    {
        public TestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        internal new void Create<T>(T entityToCreate) where T : IPersistentEntity
        {
            base.Create<T>(entityToCreate);
        }

        internal new T GetById<T>(Guid id) where T : IPersistentEntity
        {
            return base.GetById<T>(id);
        }

        internal new void Update<T>(T entity) where T : IPersistentEntity
        {
            base.Update<T>(entity);
        }

        internal new void Delete<T>(Guid id) where T : IPersistentEntity
        {
            base.Delete<T>(id);
        }
    }
}