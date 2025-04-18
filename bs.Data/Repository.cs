﻿using bs.Data.Interfaces;
using bs.Data.Interfaces.BaseEntities;
using NHibernate;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace bs.Data
{
    /// <summary>
    /// Base repository. It implements the base methods to do CRUDS on a database with the ORM
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.IRepository" />
    public abstract class Repository : IRepository
    {
        /// <summary>
        /// The unit ofwork
        /// </summary>
        protected readonly IUnitOfWork unitOfwork;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="unitOfwork">The unit ofwork.</param>
        protected Repository(IUnitOfWork unitOfwork)
        {
            this.unitOfwork = unitOfwork;
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected void Create<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity.GetType().GetInterfaces().Contains(typeof(IAuditableEntity)))
            {
                ((IAuditableEntity)entity).CreationDate = DateTime.UtcNow;
            }
            unitOfwork.Session.Save(entity);
        }

        /// <summary>
        /// Creates the specified entity asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected async Task CreateAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity.GetType().GetInterfaces().Contains(typeof(IAuditableEntity)))
            {
                ((IAuditableEntity)entity).CreationDate = DateTime.UtcNow;
            }
            await unitOfwork.Session.SaveAsync(entity);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected void Delete<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            unitOfwork.Session.Delete(entity);
        }

        /// <summary>
        /// Deletes the specified entity asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            await unitOfwork.Session.DeleteAsync(entity);
        }

        /// <summary>
        /// Deletes the specified entity logically. It not  remove entity fron database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected void DeleteLogically<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            entity.IsDeleted = true;
            entity.DeletionDate = DateTime.UtcNow;
            unitOfwork.Session.Update(entity);
        }

        /// <summary>
        /// Deletes the specified entity asynchronous. It not  remove entity fron database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected async Task DeleteLogicallyAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            entity.IsDeleted = true;
            entity.DeletionDate = DateTime.UtcNow;
            await unitOfwork.Session.UpdateAsync(entity);
        }

        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected TEntity GetById<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            return unitOfwork.Session.Get<TEntity>(id);
        }

        /// <summary>
        /// Gets the entity by identifier asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected async Task<TEntity> GetByIdAsync<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            return await unitOfwork.Session.GetAsync<TEntity>(id);
        }

        /// <summary>
        /// Loads the entity by identifier (this not call Database but look for entity in the current session).
        /// It throw an exception if entity is not in the session, it means that the entity you are looking for has to be
        /// loaded previously with the <see cref="GetById"></see> or <see cref="GetByIdAsync"></see>  methods.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected TEntity LoadById<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            return unitOfwork.Session.Load<TEntity>(id);
        }

        /// <summary>
        /// Loads the entity by identifier asynchronous.(this not call Database but look for entity in the current session).
        /// It throw an exception if entity is not in the session, it means that the entity you are looking for has to be
        /// loaded previously with the <see cref="GetById"></see> or <see cref="GetByIdAsync"></see>  methods.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected async Task<TEntity> LoadByIdAsync<TEntity>(object id) where TEntity : class, IPersistentEntity
        {
            return await unitOfwork.Session.LoadAsync<TEntity>(id);
        }

        protected IQueryable<TEntity> Query<TEntity>() where TEntity : class, IPersistentEntity
        {
            return unitOfwork.Session.Query<TEntity>();
        }

        /// <summary>
        /// Queries the logically deleted entities of the specified type in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        protected IQueryable<TEntity> QueryLogicallyDeleted<TEntity>() where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            return unitOfwork.Session.Query<TEntity>().Where(e => e.IsDeleted);
        }

        protected IQueryable<TEntity> QueryLogicallyNotDeleted<TEntity>() where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            return unitOfwork.Session.Query<TEntity>().Where(e => !e.IsDeleted);
        }

        /// <summary>
        /// Queries 'over' this entity type and returns all entities in database of the specified type.
        /// QueryOver is a specific Nhibernate method that permits to handle in details the way ORM constructs the query to Database.
        /// To know more see: <see cref="https://nhibernate.info/doc/nhibernate-reference/queryqueryover.html">Nhibernate documentation</see>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        protected IQueryOver<TEntity> QueryOver<TEntity>() where TEntity : class, IPersistentEntity
        {
            return unitOfwork.Session.QueryOver<TEntity>();
        }

        /// <summary>
        /// Deletes the specified entity logically. It not  remove entity fron database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected void RestoreLogically<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            entity.IsDeleted = false;
            entity.DeletionDate = null;
            unitOfwork.Session.Update(entity);
        }

        /// <summary>
        /// Deletes the specified entity asynchronous. It not  remove entity fron database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected async Task RestoreLogicallyAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity, ILogicallyDeletableEntity
        {
            entity.IsDeleted = false;
            entity.DeletionDate = null;
            await unitOfwork.Session.UpdateAsync(entity);
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected void Update<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity.GetType().GetInterfaces().Contains(typeof(IAuditableEntity)))
            {
                ((IAuditableEntity)entity).LastUpdateDate = DateTime.UtcNow;
            }
            unitOfwork.Session.Update(entity);
        }

        /// <summary>
        /// Updates the specified entity asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        protected async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IPersistentEntity
        {
            if (entity.GetType().GetInterfaces().Contains(typeof(IAuditableEntity)))
            {
                ((IAuditableEntity)entity).LastUpdateDate = DateTime.UtcNow;
            }
            await unitOfwork.Session.UpdateAsync(entity);
        }
    }
}