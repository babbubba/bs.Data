//using bs.Data.Interfaces;
//using bs.Data.Interfaces.BaseEntities;
//using NHibernate.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace bs.Data
//{
//    public abstract class AsyncRepository : IRepository
//    {
//        protected readonly IAsyncUnitOfWork _unitOfWork;
//        /// <summary>Initializes a new instance of the <see cref="Repository"/> class.</summary>
//        /// <param name="unitOfWork">The unit of work.</param>
//        public AsyncRepository(IAsyncUnitOfWork unitOfWork)
//        {
//            _unitOfWork = unitOfWork;
//        }

//        /// <summary>Gets an IEnumerable object representing all entities of the specified type.</summary>
//        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
//        /// <returns>The IQueryable representing all entities of the specified 'T' Type.</returns>
//        protected async Task<IEnumerable<T>> GetAll<T>() where T : IPersistentEntity
//        {
//            return await _unitOfWork.Session.Query<T>().ToListAsync();
//        }

//        /// <summary>Gets the entity by its identifier.</summary>
//        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
//        /// <param name="id">The unique identifier (primary key).</param>
//        /// <returns>The desired entity or null value.</returns>
//        protected async Task<T> GetById<T>(Guid id) where T : IPersistentEntity
//        {
//            return await _unitOfWork.Session.GetAsync<T>(id);
//        }

//        /// <summary>Gets the entities by their identifiers.</summary>
//        /// <typeparam name="T">The Entities Type that derives from IPersisterEntity interface.</typeparam>
//        /// <param name="ids">The unique identifier array(primary key).</param>
//        /// <returns>The desired entities or null value.</returns>
//        protected async Task<IEnumerable<T>> GetByIds<T>(Guid[] ids) where T : IPersistentEntity
//        {
//            return await _unitOfWork.Session.Query<T>().Where(e => ids.Contains(e.Id)).ToListAsync();
//        }

//        /// <summary>Creates the specified entity in the ORM Session (and in the DB after transaction will be committed).</summary>
//        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
//        /// <param name="entity">The entity to create in the database.</param>
//        protected async Task Create<T>(T entity) where T : IPersistentEntity
//        {
//            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
//            {
//                ((IAuditableEntity)entity).CreationDate = DateTime.Now;
//            }
//            await _unitOfWork.Session.SaveAsync(entity);
//        }

//        /// <summary>Update the specified entity in the ORM Session (and in the DB after transaction will be committed).</summary>
//        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
//        /// <param name="entity">The entity to update in the database.</param>
//        protected async Task Update<T>(T entity) where T : IPersistentEntity
//        {
//            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
//            {
//                ((IAuditableEntity)entity).LastUpdateDate = DateTime.Now;
//            }
//            await _unitOfWork.Session.UpdateAsync(entity);
//        }

//        /// <summary>Deletes the Entity with specified identifier in the ORM Session (and in the DB after transaction will be committed).</summary>
//        /// <typeparam name="T">The Entity Type that derives from IPersisterEntity interface.</typeparam>
//        /// <param name="id">The Entity unique identifier.</param>
//        protected async Task Delete<T>(Guid id) where T : IPersistentEntity
//        {
//            await _unitOfWork.Session.DeleteAsync(await _unitOfWork.Session.LoadAsync<T>(id));
//        }

//        /// <summary>
//        /// Deletes logically the entity (the row field IsDeleted will setted to true and the value of the field DeletionDate
//        /// is setted to current date time).
//        /// </summary>
//        /// <typeparam name="T">The Entity Type that derives from ILogicallyDeletableEntity interface.</typeparam>
//        /// <param name="entity">The entity.</param>
//        protected async Task DeleteLogically<T>(T entity) where T : ILogicallyDeletableEntity
//        {
//            ((ILogicallyDeletableEntity)entity).DeletionDate = DateTime.Now;
//            ((ILogicallyDeletableEntity)entity).IsDeleted = true;
//            await _unitOfWork.Session.UpdateAsync(entity);
//        }

//        /// <summary>
//        /// Restores logically the entity (the row field IsDeleted will setted to false and the value of the field DeletionDate
//        /// is setted to null). This method is the opposite of 'DeleteLogically' method.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="entity">The entity.</param>
//        protected async Task RestoreLogically<T>(T entity) where T : ILogicallyDeletableEntity
//        {
//            ((ILogicallyDeletableEntity)entity).DeletionDate = null;
//            ((ILogicallyDeletableEntity)entity).IsDeleted = false;
//            await _unitOfWork.Session.UpdateAsync(entity);
//        }
//    }
//}