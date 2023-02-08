using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;
using NHibernate.Mapping.ByCode.Impl.CustomizersImpl;
using NHibernate.Persister.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using TypeExtensions = NHibernate.Mapping.ByCode.TypeExtensions;

namespace bs.Data.Mapping
{
    public class BsClassCustomizer<TEntity> : BsPropertyContainerCustomizer<TEntity>, IClassMapper<TEntity>, IConformistHoldersProvider, IEntitySqlsWithCheckMapper where TEntity : class
    {
        private string currentTableName;
        private Dictionary<string, IJoinMapper<TEntity>> joinCustomizers;

        public BsClassCustomizer(IModelExplicitDeclarationsHolder explicitDeclarationsHolder, ICustomizersHolder customizersHolder)
            : base(explicitDeclarationsHolder, customizersHolder, null)
        {
            if (explicitDeclarationsHolder == null)
            {
                throw new ArgumentNullException("explicitDeclarationsHolder");
            }
            explicitDeclarationsHolder.AddAsRootEntity(typeof(TEntity));

            // Add an empty customizer as a way to register the class as explicity declared
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => { });
        }

        ICustomizersHolder IConformistHoldersProvider.CustomizersHolder
        {
            get { return CustomizersHolder; }
        }

        IModelExplicitDeclarationsHolder IConformistHoldersProvider.ExplicitDeclarationsHolder
        {
            get { return ExplicitDeclarationsHolder; }
        }

        private Dictionary<string, IJoinMapper<TEntity>> JoinCustomizers
        {
            get { return joinCustomizers ?? (joinCustomizers = new Dictionary<string, IJoinMapper<TEntity>>()); }
        }

        #region Implementation of IClassAttributesMapper<TEntity>

        public void Abstract(bool isAbstract)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Abstract(isAbstract));
        }

        public void Catalog(string catalogName)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Catalog(catalogName));
        }

        public void Check(string tableName)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Check(tableName));
        }

        public void ComponentAsId<TComponent>(Expression<Func<TEntity, TComponent>> idProperty)
        {
            ComponentAsId(idProperty, x => { });
        }

        public void ComponentAsId<TComponent>(Expression<Func<TEntity, TComponent>> idProperty, Action<IComponentAsIdMapper<TComponent>> idMapper)
        {
            var memberOf = TypeExtensions.DecodeMemberAccessExpressionOf(idProperty);
            RegisterComponentAsIdMapping(idMapper, memberOf);
        }

        public void ComponentAsId<TComponent>(string notVisiblePropertyOrFieldName)
        {
            ComponentAsId<TComponent>(notVisiblePropertyOrFieldName, x => { });
        }

        public void ComponentAsId<TComponent>(string notVisiblePropertyOrFieldName, Action<IComponentAsIdMapper<TComponent>> idMapper)
        {
            var member = typeof(TEntity).GetPropertyOrFieldMatchingName(notVisiblePropertyOrFieldName);
            RegisterComponentAsIdMapping(idMapper, member);
        }

        public void ComposedId(Action<IComposedIdMapper<TEntity>> idPropertiesMapping)
        {
            idPropertiesMapping(new ComposedIdCustomizer<TEntity>(ExplicitDeclarationsHolder, CustomizersHolder));
        }

        public void Discriminator(Action<IDiscriminatorMapper> discriminatorMapping)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Discriminator(discriminatorMapping));
        }

        public void DiscriminatorValue(object value)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.DiscriminatorValue(value));
        }

        public void GuidId<TProperty>(Expression<Func<TEntity, TProperty>> idProperty)
        {
            Action<IIdMapper> idMapper = mapper =>
            {
                mapper.Generator(Generators.Guid);
                mapper.Type(NHibernateUtil.Guid);
                mapper.Column("Id");
                mapper.UnsavedValue(Guid.Empty);
            };

            Id(idProperty, idMapper);
        }

        public void Id<TProperty>(Expression<Func<TEntity, TProperty>> idProperty)
        {
            Id(idProperty, x => { });
        }

        public void Id<TProperty>(Expression<Func<TEntity, TProperty>> idProperty, Action<IIdMapper> idMapper)
        {
            MemberInfo member = null;
            if (idProperty != null)
            {
                member = TypeExtensions.DecodeMemberAccessExpression(idProperty);
                ExplicitDeclarationsHolder.AddAsPoid(member);
            }
            CustomizersHolder.AddCustomizer(typeof(TEntity), m => m.Id(member, idMapper));
        }

        public void Id(string notVisiblePropertyOrFieldName, Action<IIdMapper> idMapper)
        {
            MemberInfo member = null;
            if (notVisiblePropertyOrFieldName != null)
            {
                member = typeof(TEntity).GetPropertyOrFieldMatchingName(notVisiblePropertyOrFieldName);
                ExplicitDeclarationsHolder.AddAsPoid(member);
            }
            CustomizersHolder.AddCustomizer(typeof(TEntity), m => m.Id(member, idMapper));
        }

        public void OptimisticLock(OptimisticLockMode mode)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.OptimisticLock(mode));
        }

        public void Polymorphism(PolymorphismType type)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Polymorphism(type));
        }

        public void Schema(string schemaName)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Schema(schemaName));
        }

        public void Table(string tableName)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Table(tableName));
            currentTableName = tableName;
        }

        private void RegisterComponentAsIdMapping<TComponent>(Action<IComponentAsIdMapper<TComponent>> idMapper, params MemberInfo[] members)
        {
            foreach (var member in members)
            {
                var propertyPath = new PropertyPath(PropertyPath, member);
                idMapper(new ComponentAsIdCustomizer<TComponent>(ExplicitDeclarationsHolder, CustomizersHolder, propertyPath));
            }
        }

        #endregion Implementation of IClassAttributesMapper<TEntity>

        #region Implementation of IEntityAttributesMapper

        public void BatchSize(int value)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.BatchSize(value));
        }

        public void Cache(Action<ICacheMapper> cacheMapping)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Cache(cacheMapping));
        }

        public void DynamicInsert(bool value)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.DynamicInsert(value));
        }

        public void DynamicUpdate(bool value)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.DynamicUpdate(value));
        }

        public void EntityName(string value)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.EntityName(value));
        }

        public void Filter(string filterName, Action<IFilterMapper> filterMapping)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Filter(filterName, filterMapping));
        }

        public void Join(string splitGroupId, Action<IJoinMapper<TEntity>> splitMapping)
        {
            // add the customizer only to create the JoinMapper instance
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Join(splitGroupId, j => { }));

            IJoinMapper<TEntity> joinCustomizer;
            if (!JoinCustomizers.TryGetValue(splitGroupId, out joinCustomizer))
            {
                joinCustomizer = new JoinCustomizer<TEntity>(splitGroupId, ExplicitDeclarationsHolder, CustomizersHolder);
                JoinCustomizers.Add(splitGroupId, joinCustomizer);
            }
            splitMapping(joinCustomizer);
        }

        public void Lazy(bool value)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Lazy(value));
        }

        public void Mutable(bool isMutable)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Mutable(isMutable));
        }

        public void NaturalId(Action<IBasePlainPropertyContainerMapper<TEntity>> naturalIdPropertiesMapping, Action<INaturalIdAttributesMapper> naturalIdMapping)
        {
            naturalIdPropertiesMapping(new NaturalIdCustomizer<TEntity>(ExplicitDeclarationsHolder, CustomizersHolder));
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.NaturalId(nidm => naturalIdMapping(nidm)));
        }

        public void NaturalId(Action<IBasePlainPropertyContainerMapper<TEntity>> naturalIdPropertiesMapping)
        {
            NaturalId(naturalIdPropertiesMapping, mapAttr => { });
        }

        public void Persister<T>() where T : IEntityPersister
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Persister<T>());
        }

        public void Proxy(System.Type proxy)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Proxy(proxy));
        }

        public void SchemaAction(NHibernate.Mapping.ByCode.SchemaAction action)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SchemaAction(action));
        }

        public void SelectBeforeUpdate(bool value)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SelectBeforeUpdate(value));
        }

        public void Synchronize(params string[] table)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Synchronize(table));
        }

        public void Version<TProperty>(Expression<Func<TEntity, TProperty>> versionProperty, Action<IVersionMapper> versionMapping)
        {
            MemberInfo member = TypeExtensions.DecodeMemberAccessExpression(versionProperty);
            ExplicitDeclarationsHolder.AddAsVersionProperty(member);
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Version(member, versionMapping));
        }

        public void Version(string notVisiblePropertyOrFieldName, Action<IVersionMapper> versionMapping)
        {
            var member = typeof(TEntity).GetPropertyOrFieldMatchingName(notVisiblePropertyOrFieldName);
            ExplicitDeclarationsHolder.AddAsVersionProperty(member);
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Version(member, versionMapping));
        }

        public void Where(string whereClause)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Where(whereClause));
        }

        #endregion Implementation of IEntityAttributesMapper

        #region Implementation of IEntitySqlsMapper

        public void Loader(string namedQueryReference)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Loader(namedQueryReference));
        }

        public void SqlDelete(string sql)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SqlDelete(sql));
        }

        public void SqlDelete(string sql, SqlCheck sqlCheck)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SqlDelete(sql, sqlCheck));
        }

        public void SqlInsert(string sql)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SqlInsert(sql));
        }

        public void SqlInsert(string sql, SqlCheck sqlCheck)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SqlInsert(sql, sqlCheck));
        }

        public void SqlUpdate(string sql)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SqlUpdate(sql));
        }

        public void SqlUpdate(string sql, SqlCheck sqlCheck)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.SqlUpdate(sql, sqlCheck));
        }

        public void Subselect(string sql)
        {
            CustomizersHolder.AddCustomizer(typeof(TEntity), (IClassMapper m) => m.Subselect(sql));
        }

        #endregion Implementation of IEntitySqlsMapper

        //public void SetManyToOne<TProperty>(Expression<Func<TEntity, TProperty>> property, string refColumn, string fkName = null)
        //    where TProperty : class
        //{
        //    Action<IManyToOneMapper> mapping = map =>
        //    {
        //        map.Column(refColumn);
        //        if(fkName!=null)
        //        {
        //            map.ForeignKey(fkName);
        //        }
        //    };

        //    RegisterManyToOneMapping(property, mapping);
        //}
    }
}