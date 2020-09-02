using bs.Data.Interfaces;
using NHibernate;
using System.Linq;
using System.Threading.Tasks;

namespace bs.Data
{
    public abstract class Repository : IRepository
    {
        private readonly IUnitOfWork unitOfwork;

        public Repository(IUnitOfWork unitOfwork)
        {
            this.unitOfwork = unitOfwork;
        }

        protected void Delete<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            unitOfwork.Session.Delete(entity);
        }

        protected async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            await unitOfwork.Session.DeleteAsync(entity);
        }

        protected IQueryable<TEntity> Query<TEntity>() where TEntity : class, IPersistentEntity
        {
            return unitOfwork.Session.Query<TEntity>();
        }

        protected IQueryOver<TEntity> QueryOver<TEntity>() where TEntity : class, IPersistentEntity
        {
            return unitOfwork.Session.QueryOver<TEntity>();
        }

        protected void Create<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            unitOfwork.Session.Save(entity);
        }

        protected async Task CreateAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            await unitOfwork.Session.SaveAsync(entity);
        }

        protected void Update<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            unitOfwork.Session.Update(entity);
        }

        protected async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            await unitOfwork.Session.UpdateAsync(entity);
        }

        protected void GetById<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            unitOfwork.Session.Get<TEntity>(id);
        }

        protected async Task GetByIdAsync<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            await unitOfwork.Session.GetAsync<TEntity>(id);
        }

        protected void LoadById<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            unitOfwork.Session.Load<TEntity>(id);
        }

        protected async Task LoadByIdAsync<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            await unitOfwork.Session.LoadAsync<TEntity>(id);
        }
    }
}