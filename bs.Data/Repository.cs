using bs.Data.Interfaces;
using bs.Data.Interfaces.BaseEntities;
using NHibernate;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace bs.Data
{
    /// <summary>
    /// Base repository implementing CRUD operations with NHibernate ORM.
    /// </summary>
    public abstract class Repository : IRepository
    {
        protected readonly IUnitOfWork _unitOfWork;

        protected Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        protected ISession Session => _unitOfWork.Session;

        #region Create

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        protected void Create<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            SetCreationDate(entity);
            Session.Save(entity);
        }

        /// <summary>
        /// Creates the specified entity asynchronously.
        /// </summary>
        protected async Task CreateAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            SetCreationDate(entity);
            await Session.SaveAsync(entity);
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        protected void Update<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            SetUpdateDate(entity);
            Session.Update(entity);
        }

        /// <summary>
        /// Updates the specified entity asynchronously.
        /// </summary>
        protected async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            SetUpdateDate(entity);
            await Session.UpdateAsync(entity);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the specified entity physically from the database.
        /// </summary>
        protected void Delete<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Session.Delete(entity);
        }

        /// <summary>
        /// Deletes the specified entity physically from the database asynchronously.
        /// </summary>
        protected async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await Session.DeleteAsync(entity);
        }

        /// <summary>
        /// Deletes the specified entity logically without removing it from the database.
        /// </summary>
        protected void DeleteLogically<TEntity>(TEntity entity)
            where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.IsDeleted = true;
            entity.DeletionDate = DateTime.UtcNow;
            Session.Update(entity);
        }

        /// <summary>
        /// Deletes the specified entity logically without removing it from the database asynchronously.
        /// </summary>
        protected async Task DeleteLogicallyAsync<TEntity>(TEntity entity)
            where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.IsDeleted = true;
            entity.DeletionDate = DateTime.UtcNow;
            await Session.UpdateAsync(entity);
        }

        /// <summary>
        /// Restores a logically deleted entity.
        /// </summary>
        protected void RestoreLogically<TEntity>(TEntity entity)
            where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.IsDeleted = false;
            entity.DeletionDate = null;
            Session.Update(entity);
        }

        /// <summary>
        /// Restores a logically deleted entity asynchronously.
        /// </summary>
        protected async Task RestoreLogicallyAsync<TEntity>(TEntity entity)
            where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.IsDeleted = false;
            entity.DeletionDate = null;
            await Session.UpdateAsync(entity);
        }

        #endregion

        #region Retrieve

        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        protected TEntity GetById<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            ArgumentNullException.ThrowIfNull(id);


            return Session.Get<TEntity>(id);
        }

        /// <summary>
        /// Gets the entity by identifier asynchronously.
        /// </summary>
        protected async Task<TEntity> GetByIdAsync<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            ArgumentNullException.ThrowIfNull(id);

            return await Session.GetAsync<TEntity>(id);
        }

        /// <summary>
        /// Loads the entity by identifier from the session cache (does not query the database).
        /// Throws an exception if the entity is not in the session.
        /// The entity must be loaded previously with GetById or GetByIdAsync methods.
        /// </summary>
        protected TEntity LoadById<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            ArgumentNullException.ThrowIfNull(id);

            return Session.Load<TEntity>(id);
        }

        /// <summary>
        /// Loads the entity by identifier from the session cache asynchronously (does not query the database).
        /// Throws an exception if the entity is not in the session.
        /// The entity must be loaded previously with GetById or GetByIdAsync methods.
        /// </summary>
        protected async Task<TEntity> LoadByIdAsync<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            ArgumentNullException.ThrowIfNull(id);

            return await Session.LoadAsync<TEntity>(id);
        }

        #endregion

        #region Query

        /// <summary>
        /// Creates a LINQ query for the specified entity type.
        /// </summary>
        protected IQueryable<TEntity> Query<TEntity>() where TEntity : class, IPersistentEntity
        {
            return Session.Query<TEntity>();
        }

        /// <summary>
        /// Queries logically deleted entities of the specified type.
        /// </summary>
        protected IQueryable<TEntity> QueryLogicallyDeleted<TEntity>()
            where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            return Session.Query<TEntity>().Where(e => e.IsDeleted);
        }

        /// <summary>
        /// Queries entities that are not logically deleted.
        /// </summary>
        protected IQueryable<TEntity> QueryLogicallyNotDeleted<TEntity>()
            where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            return Session.Query<TEntity>().Where(e => !e.IsDeleted);
        }

        /// <summary>
        /// Creates a QueryOver query for the specified entity type.
        /// QueryOver provides strongly-typed NHibernate queries with more control over query construction.
        /// See: <see href="https://nhibernate.info/doc/nhibernate-reference/queryqueryover.html">NHibernate documentation</see>
        /// </summary>
        protected IQueryOver<TEntity> QueryOver<TEntity>() where TEntity : class, IPersistentEntity
        {
            return Session.QueryOver<TEntity>();
        }

        #endregion

        #region Private Helpers

        private static void SetCreationDate<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity is IAuditableEntity auditable)
            {
                auditable.CreationDate = DateTime.UtcNow;
            }
        }

        private static void SetUpdateDate<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity is IAuditableEntity auditable)
            {
                auditable.LastUpdateDate = DateTime.UtcNow;
            }
        }

        #endregion
    }
}