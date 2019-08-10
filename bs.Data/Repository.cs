using bs.Data.Interfaces;
using bs.Data.Interfaces.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bs.Data
{
    /// <summary>Abstracted base class for all repositories.</summary>
    /// <seealso cref="bs.Data.Interfaces.IRepository"/>
    public abstract class Repository : IRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        /// <summary>Initializes a new instance of the <see cref="Repository"/> class.</summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Gets an IQueryable object representing all entities of the specified type.</summary>
        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
        /// <returns>The IQueryable representing all entities of the specified 'T' Type.</returns>
        public IQueryable<T> GetAll<T>() where T : IPersistentEntity
        {
            return _unitOfWork.Session.Query<T>();
        }

        /// <summary>Gets the entity by its identifier.</summary>
        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
        /// <param name="id">The unique identifier (primary key).</param>
        /// <returns>The desired entity or null value.</returns>
        public T GetById<T>(Guid id) where T : IPersistentEntity
        {
            return _unitOfWork.Session.Get<T>(id);
        }

        /// <summary>Creates the specified entity in the ORM Session (and in the DB after transaction will be committed).</summary>
        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
        /// <param name="entity">The entity to create in the database.</param>
        public void Create<T>(T entity) where T : IPersistentEntity
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                ((IAuditableEntity)entity).CreationDate = DateTime.Now;
            }
            _unitOfWork.Session.Save(entity);
        }

        /// <summary>Update the specified entity in the ORM Session (and in the DB after transaction will be committed).</summary>
        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
        /// <param name="entity">The entity to update in the database.</param>
        public void Update<T>(T entity) where T : IPersistentEntity
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                ((IAuditableEntity)entity).LastUpdateDate = DateTime.Now;
            }
            _unitOfWork.Session.Update(entity);
        }

        /// <summary>Deletes the Entity with specified identifier in the ORM Session (and in the DB after transaction will be committed).</summary>
        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
        /// <param name="id">The Entity unique identifier.</param>
        public void Delete<T>(Guid id) where T : IPersistentEntity
        {
            _unitOfWork.Session.Delete(_unitOfWork.Session.Load<T>(id));
        }

        /// <summary>
        /// Deletes logically the entity (the row field IsDeleted will setted to true and the value of the field DeletionDate 
        /// is setted to current date time).
        /// </summary>
        /// <typeparam name="T">The Entity Type that derives from ILogicallyDeletableEntity interface.</typeparam>
        /// <param name="entity">The entity.</param>
        public void DeleteLogically<T>(T entity) where T : ILogicallyDeletableEntity
        {
            ((ILogicallyDeletableEntity)entity).DeletionDate = DateTime.Now;
            ((ILogicallyDeletableEntity)entity).IsDeleted = true;
            _unitOfWork.Session.Update(entity);
        }

        /// <summary>
        /// Restores logically the entity (the row field IsDeleted will setted to false and the value of the field DeletionDate 
        /// is setted to null). This method is the opposite of 'DeleteLogically' method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        public void RestoreLogically<T>(T entity) where T : ILogicallyDeletableEntity
        {
            ((ILogicallyDeletableEntity)entity).DeletionDate = null;
            ((ILogicallyDeletableEntity)entity).IsDeleted = false;
            _unitOfWork.Session.Update(entity);
        }

    }
}
