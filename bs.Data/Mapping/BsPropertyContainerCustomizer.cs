using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;
using NHibernate.Mapping.ByCode.Impl.CustomizersImpl;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace bs.Data.Mapping
{
    public class BsPropertyContainerCustomizer<TEntity> : PropertyContainerCustomizer<TEntity>
    {

        public BsPropertyContainerCustomizer(IModelExplicitDeclarationsHolder explicitDeclarationsHolder, ICustomizersHolder customizersHolder, PropertyPath propertyPath) : base(explicitDeclarationsHolder, customizersHolder, propertyPath)
        {
        }

        /// <summary>
        /// Sets the many to many.
        /// </summary>
        /// <typeparam name="TElement">The type of the element (cascade all).</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="linkTableName">Name of the link table.</param>
        /// <param name="currentEntityColumn">The current entity column.</param>
        /// <param name="referencedEntityColumn">The referenced entity column.</param>
        public void SetManyToMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, string linkTableName, string currentEntityColumn, string referencedEntityColumn)
        {
            SetManyToMany(property, linkTableName, currentEntityColumn, referencedEntityColumn,null, true);
        }
        public void SetManyToMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, string linkTableName, string currentEntityColumn, string referencedEntityColumn, Type referenceClass, bool cascadeAll)
        {
            void properyMapper(ISetPropertiesMapper<TEntity, TElement> p)
            {
                p.Table(linkTableName);
                p.Key(pk => pk.Column(currentEntityColumn));
                p.Fetch(CollectionFetchMode.Subselect);
                p.BatchSize(100);
                p.Lazy(CollectionLazy.NoLazy);
                if (cascadeAll) p.Cascade(Cascade.All);


            }

            void mapping(ICollectionElementRelation<TElement> map)
            {
                map.ManyToMany(collectionMapping =>
                {
                    if(referenceClass!=null) collectionMapping.Class(referenceClass);
                    collectionMapping.Column(referencedEntityColumn);

                });
            }

            RegisterSetMapping(property, properyMapper, mapping);


        }

        public void SetManyToOne<TProperty>(Expression<Func<TEntity, TProperty>> property, string refColumn) where TProperty : class
        {
            SetManyToOne(property, refColumn, null);
        }
        public void SetManyToOne<TProperty>(Expression<Func<TEntity, TProperty>> property, string refColumn, string fkName) where TProperty : class
        {
            void mapping(IManyToOneMapper t)
            {
                t.Column(refColumn);
                if (fkName != null)
                {
                    t.ForeignKey(fkName);
                }
                t.Cascade(Cascade.All);
                t.Lazy(LazyRelation.NoProxy);

            }

            RegisterManyToOneMapping(property, mapping);
        }

        /// <summary>
        /// Sets the one to many relationship (the saving responsability is of the related entity and cascade all is setted as deefault behaviour).
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="referenceColumn">The reference column.</param>
        /// <param name="referenceClass">The reference class.</param>
        public void SetOneToMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, string referenceColumn, Type referenceClass)
        {
            SetOneToMany(property, referenceColumn, referenceClass, true, true);
        }
        /// <summary>
        /// Sets the one to many.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="referenceColumn">The reference column.</param>
        /// <param name="referenceClass">The reference class.</param>
        /// <param name="inverse">if set to <c>true</c> the save/update action is demanded to related entity otherwise is demanded to this entity.</param>
        public void SetOneToMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, string referenceColumn, Type referenceClass, bool inverse, bool cascadeAll)
        {
            void properyMapper(ISetPropertiesMapper<TEntity, TElement> t)
            {
                t.Inverse(inverse);
                t.Key(km => km.Column(referenceColumn));
                t.Fetch(CollectionFetchMode.Subselect);
                t.BatchSize(100);
                t.Lazy(CollectionLazy.NoLazy);
                if(cascadeAll) t.Cascade(Cascade.All);
            }

            void mapping(ICollectionElementRelation<TElement> map)
            {
                map.OneToMany(a => a.Class(referenceClass));
            }

            RegisterSetMapping(property, properyMapper, mapping);
        }
    }
}