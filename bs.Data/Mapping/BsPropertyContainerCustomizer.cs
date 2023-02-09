using NHibernate;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;
using NHibernate.Mapping.ByCode.Impl.CustomizersImpl;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;

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
            SetManyToMany(property, linkTableName, currentEntityColumn, referencedEntityColumn,null, null);
        }
        public void SetManyToMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, string linkTableName, string currentEntityColumn, string referencedEntityColumn, Type referenceClass, bool cascadeAll)
        {
            void properyMapper(ISetPropertiesMapper<TEntity, TElement> p)
            {
                p.Cascade(cascadeAll ? Cascade.All :Cascade.None);
            }

            void manyToManyMapping(IManyToManyMapper m)
            {
                if (referenceClass != null) m.Class(referenceClass);
            }

            SetManyToMany(property, linkTableName, currentEntityColumn, referencedEntityColumn, properyMapper, manyToManyMapping);
        }
        public void SetManyToMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, string linkTableName, string currentEntityColumn, string referencedEntityColumn, Action<ISetPropertiesMapper<TEntity, TElement>> propertiesMapper, Action<IManyToManyMapper> manyToManyMapper)
        {
            void properyMapper(ISetPropertiesMapper<TEntity, TElement> p)
            {
                p.Table(linkTableName);
                p.Key(pk => pk.Column(currentEntityColumn));
                p.Fetch(CollectionFetchMode.Subselect);
                p.BatchSize(100);
                p.Lazy(CollectionLazy.NoLazy);
                propertiesMapper?.Invoke(p);
            }

            void mapping(ICollectionElementRelation<TElement> map)
            {
                map.ManyToMany(collectionMapping =>
                {
                    collectionMapping.Column(referencedEntityColumn);
                    manyToManyMapper?.Invoke(collectionMapping);
                });
            }

            RegisterSetMapping(property, properyMapper, mapping);


        }

        public void SetManyToOne<TElement>(Expression<Func<TEntity, TElement>> property, string refColumn) where TElement : class
        {
            SetManyToOne(property, refColumn, null,null,null);
        }
        public void SetManyToOne<TElement>(Expression<Func<TEntity, TElement>> property, string refColumn, string fkName) where TElement : class
        {
            SetManyToOne(property, refColumn, fkName,null,null);
        }

        public void SetManyToOne<TElement>(Expression<Func<TEntity, TElement>> property, string refColumn, string fkName, Type referenceClass) where TElement : class
        {
            SetManyToOne(property, refColumn, fkName, referenceClass, null);
        }
        public void SetManyToOne<TElement>(Expression<Func<TEntity, TElement>> property, string refColumn, string fkName, Type referenceClass, Action<IManyToOneMapper> propMapper) where TElement : class
        {
            void mapping(IManyToOneMapper t)
            {
                t.Column(refColumn);
                if (fkName != null) t.ForeignKey(fkName);
                t.Cascade(Cascade.All);
                t.Lazy(LazyRelation.NoProxy);
                if(referenceClass!=null) t.Class(referenceClass);
                propMapper?.Invoke(t);
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
                t.Cascade(cascadeAll ? Cascade.All : Cascade.None);
            }

            SetOneToMany(property, referenceColumn, referenceClass, properyMapper);
        }
        public void SetOneToMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, string referenceColumn, Type referenceClass, Action<ISetPropertiesMapper<TEntity, TElement>> propMapper)
        {
            void properyMapper(ISetPropertiesMapper<TEntity, TElement> t)
            {
                t.Key(km => km.Column(referenceColumn));
                t.Fetch(CollectionFetchMode.Subselect);
                t.BatchSize(Global.BATCH_SIZE);
                t.Lazy(CollectionLazy.NoLazy);
                t.Inverse(true);
                t.Cascade(Cascade.All);
                propMapper?.Invoke(t);
            }

            void mapping(ICollectionElementRelation<TElement> map)
            {
                map.OneToMany(a => a.Class(referenceClass));
            }

            RegisterSetMapping(property, properyMapper, mapping);
        }

        public void PropertyUtcDate<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            void mapping (IPropertyMapper map)
            {
                map.Type<UtcDateTimeType>();
            }
            RegisterPropertyMapping(property, mapping);
        }
        
        public void PropertyText<TProperty>(Expression<Func<TEntity, TProperty>> property, int lenght = 25)
        {
            void mapping(IPropertyMapper map)
            {
                map.Length(lenght);
            }
            RegisterPropertyMapping(property, mapping);
        }

        public void PropertyLongText<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            PropertyLongText(property, 1200, null);
        }
        public void PropertyLongText<TProperty>(Expression<Func<TEntity, TProperty>> property, int lenght)
        {
            PropertyLongText(property, lenght, null);
        }
        public void PropertyLongText<TProperty>(Expression<Func<TEntity, TProperty>> property, int lenght, Action<IPropertyMapper> propertyMapper)
        {
            void mapping(IPropertyMapper map)
            {
                map.Length(lenght);
                map.Type(NHibernateUtil.StringClob);
                propertyMapper?.Invoke(map);
            }
            RegisterPropertyMapping(property, mapping);
        }

        public void PropertyUnique<TProperty>(Expression<Func<TEntity, TProperty>> property, string uniqueKey)
        {
            void mapping(IPropertyMapper map)
            {
                map.UniqueKey(uniqueKey);
            }
            RegisterPropertyMapping(property, mapping);
        }

        public void PropertyBlob<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            void mapping(IPropertyMapper map)
            {
                map.Type(NHibernateUtil.BinaryBlob);
            }
            RegisterPropertyMapping(property, mapping);
        }
    }
}