using bs.Data.Helpers;
using bs.Data.Interfaces;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace bs.Data
{
    /// <summary>The Session Factory Builder Class</summary>
    internal sealed class SessionFactoryBuilder
    {
        /// <summary>Builds the Nhibernate Session Factory from the IDbContext implementation provided.</summary>
        /// <param name="dbContext">The IDbContext implementation.</param>
        /// <returns>The Nhibernate ISessionFactory.</returns>
        /// <exception cref="ApplicationException">Invalid connection string</exception>
        public static NHibernate.ISessionFactory BuildSessionFactory(IDbContext dbContext)
        {
            if (string.IsNullOrWhiteSpace(dbContext.ConnectionString))
                throw new ApplicationException("Invalid connection string");

            var modelsAssemblies = ReflectionHelper.GetAssembliesFromFiles(dbContext.FoldersWhereLookingForEntitiesDll, dbContext.EntitiesFileNameScannerPatterns, dbContext.LookForEntitiesDllInCurrentDirectoryToo);

            return Fluently.Configure()
            .Database(SQLiteConfiguration.Standard
            .ConnectionString(dbContext.ConnectionString))
            .Mappings(m => MapAssemblies(modelsAssemblies, m))
            .CurrentSessionContext("call")
            .ExposeConfiguration(cfg => BuildSchema(cfg, dbContext.Create, dbContext.Update))
            .BuildSessionFactory();
        }
        /// <summary>
        /// Build the schema of the database creating or updating it.
        /// </summary>
        /// <param name="config">Configuration.</param>
        private static void BuildSchema(Configuration config, bool create = false, bool update = false)
        {
            if (create)
            {
                new SchemaExport(config).Create(false, true);
            }
            else
            {
                new SchemaUpdate(config).Execute(false, update);
            }
        }

        /// <summary>Maps the assemblies using FluentMapping Configuration.</summary>
        /// <param name="modelsAssemblies">The models assemblies.</param>
        /// <param name="mappingConfig">The mapping configuration.</param>
        private static void MapAssemblies(IEnumerable<Assembly> modelsAssemblies, MappingConfiguration mappingConfig)
        {
            foreach (var a in modelsAssemblies)
            {
                mappingConfig.FluentMappings.AddFromAssembly(a);
            }
        }
    }
}
