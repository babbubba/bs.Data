﻿using bs.Data.Helpers;
using bs.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using System.Linq;

namespace bs.Data
{
    public static class BSDataExtensions
    {
        /// <summary>
        /// This method used in the Startup register as services the ORM's Session Factory, the ORM's Session and the ORM's Unit of Work used by the repositories you will implements.
        /// </summary>
        /// <param name="services">The services collection of the desired Dependency Controller Container of your application.</param>
        /// <param name="dbContext">The database context containing info about ORM's configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddBsData(this IServiceCollection services, IDbContext dbContext)
        {
            if (services is null)
            {
                throw new System.ArgumentNullException(nameof(services),"ServiceCollection is mandatory to handle dependency injection in consumer application");
            }

            if (dbContext is null)
            {
                throw new System.ArgumentNullException(nameof(dbContext),"The context is mandatory to init the ORM");
            }

            if (dbContext.DatabaseEngineType == DbType.Undefined)
            {
                throw new System.ArgumentOutOfRangeException(nameof(dbContext.DatabaseEngineType), "The Database Engine Type is mandatory to init the ORM");
            }

            if (string.IsNullOrWhiteSpace(dbContext.ConnectionString))
            {
                throw new System.ArgumentOutOfRangeException(nameof(dbContext.ConnectionString), "Connection String to database is mandatory to init the ORM");
            }

            // Create the mapper
            var mapper = new ModelMapper();

            // Add the entities defined in this assemblies
            mapper.AddMappings(typeof(BSDataExtensions).Assembly.ExportedTypes);
           

            // Add the entities defined in other assemblies
            try
            {
                var modelsAssemblies = ReflectionHelper.GetAssembliesFromFiles(dbContext.FoldersWhereLookingForEntitiesDll, dbContext.EntitiesFileNameScannerPatterns, dbContext.LookForEntitiesDllInCurrentDirectoryToo, dbContext.UseExecutingAssemblyToo);
                mapper.AddMappings(modelsAssemblies.SelectMany(a => a.ExportedTypes));
            }
            catch (System.Exception ex)
            {
                throw new ORMException("Error lookig for mapping's types to register using reflection. See inner exceptions for details.", ex);
            }
            //System.Diagnostics.Debug.WriteLine("ORM - Mapped external types: " + string.Join(",", modelsAssemblies.SelectMany(a => a.ExportedTypes).Select(t => t.Name)));

            // Compile mapped entities
            HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            domainMapping.autoimport = true;
            
       

            // Prepares configuration
            var configuration = new Configuration();
            var databaseIntegration = new NHibernate.Cfg.Loquacious.DbIntegrationConfigurationProperties(configuration);
          
            // import extra not persistent model
            foreach(var import in dbContext.Imports)
            {
                configuration.Imports.Add(import);
            }
         

            // It use the right database integration properties by the database type choosen
            switch (dbContext.DatabaseEngineType)
            {
                case DbType.MySQL:
                    databaseIntegration.Dialect<MySQL55Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    break;

                case DbType.SQLite:
                    databaseIntegration.Driver<NHibernate.Driver.SQLite20Driver>();
                    databaseIntegration.Dialect<SQLiteDialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    break;

                case DbType.MsSql2012:
                    databaseIntegration.Driver<NHibernate.Driver.SqlClientDriver>();
                    databaseIntegration.Dialect<MsSql2012Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    break;

                case DbType.MsSql2008:
                    databaseIntegration.Driver<NHibernate.Driver.SqlClientDriver>();
                    databaseIntegration.Dialect<MsSql2008Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    break;

                case DbType.PostgreSQL:
                    databaseIntegration.Dialect<PostgreSQL82Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    break;

                default:
                    throw new ORMException("The Database Engine Type selected is not supported in current version.");
            }

            // sets creation, updation, recreation or simple validation of schema
            if (dbContext.Create && dbContext.Update)
                databaseIntegration.SchemaAction = SchemaAutoAction.Recreate;
            else if (dbContext.Create)
                databaseIntegration.SchemaAction = SchemaAutoAction.Create;
            else if (dbContext.Update)
                databaseIntegration.SchemaAction = SchemaAutoAction.Update;
            else
                databaseIntegration.SchemaAction = SchemaAutoAction.Validate;

            databaseIntegration.LogFormattedSql = dbContext.LogFormattedSql;
            databaseIntegration.LogSqlInConsole = dbContext.LogSqlInConsole;
            databaseIntegration.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;

            // Add entities mapping to configuration
            try
            {
                configuration.AddMapping(domainMapping);
            }        
            catch (System.Exception ex)
            {
                throw new ORMException("Error adding mappings to ORM. See inner exceptions for details.", ex);
            }

            // Create the session factory
            ISessionFactory sessionFactory = null;
            try
            {
                sessionFactory = configuration.BuildSessionFactory();
            }
            catch (SchemaValidationException schemaValidationEx)
            {
                throw new ORMException($"Error validating schema:\n{string.Join(";\n ", schemaValidationEx.ValidationErrors)}", schemaValidationEx);
            }
            catch (System.Exception ex)
            {
                throw new ORMException("Error building ORM session factory. See inner exception for details", ex);
            }

            // Add to dependency injecton the factory (singleton) and session and unit of work scoped
            try
            {
                services.AddSingleton(sessionFactory);
                services.AddScoped(factory => sessionFactory.OpenSession());
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            }
            catch (System.Exception ex)
            {
                throw new ORMException("Error registering services in the provided Service Collection. See inner exception for details.", ex);
            }

            return services;
        }
    }
}