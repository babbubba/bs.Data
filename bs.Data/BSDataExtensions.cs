using bs.Data.Helpers;
using bs.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
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
                throw new System.ArgumentNullException(nameof(services), "ServiceCollection is mandatory to handle dependency injection in consumer application");
            }

            if (dbContext is null)
            {
                throw new System.ArgumentNullException(nameof(dbContext), "The context is mandatory to init the ORM");
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

            mapper.BeforeMapProperty += (modelInspector, propertyPath, propertyCustomizer) =>
            {
                if(dbContext.DatabaseEngineType == DbType.PostgreSQL || dbContext.DatabaseEngineType == DbType.PostgreSQL83)
                {
                    // Risolve l'ambiguità  tra il tipo numeric con precisione 19,5 di postgres ed il tipo decimal di c#
                    // Ricavo il tipo .NET della property
                    var memberType = propertyPath.LocalMember.GetPropertyOrFieldType();

                    // Se è un decimal o decimal?...
                    if (memberType == typeof(decimal) || memberType == typeof(decimal?))
                    {
                        // 1) Dico a NH che è decimal
                        propertyCustomizer.Type(NHibernateUtil.Decimal);

                        // 2) Imposto precision e scale
                        propertyCustomizer.Precision(19);
                        propertyCustomizer.Scale(5);

                        // 3) Forzo l'uso di "numeric(19,5)" come SqlType
                        propertyCustomizer.Column(c => {
                            c.SqlType("numeric(19,5)");
                        });
                    }
                }
               
            };


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

            // Compile mapped entities
            HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            domainMapping.autoimport = true;

            // Prepares configuration
            var configuration = new Configuration();
            var databaseIntegration = new NHibernate.Cfg.Loquacious.DbIntegrationConfigurationProperties(configuration);

            // import extra not persistent model
            if (dbContext.Imports is not null)
            {
                foreach (var import in dbContext.Imports)
                {
                    configuration.Imports.Add(import);
                }
            }

            if (dbContext.Filters is not null)
            {
                foreach (var filter in dbContext.Filters)
                {
                    configuration.AddFilterDefinition(filter);
                }
            }


            // It use the right database integration properties by the database type choosen
            switch (dbContext.DatabaseEngineType)
            {
                case DbType.MySQL:
                    databaseIntegration.Dialect<MySQL55Dialect>();
                    break;

                case DbType.MySQL57:
                    databaseIntegration.Dialect<MySQL57Dialect>();
                    break;

                case DbType.SQLite:
                    databaseIntegration.Driver<SQLite20Driver>();
                    databaseIntegration.Dialect<SQLiteDialect>();
                    break;

                case DbType.MsSql2012:
                    databaseIntegration.Driver<MicrosoftDataSqlClientDriver>();
                    databaseIntegration.Dialect<MsSql2012Dialect>();
                    break;

                case DbType.MsSql2008:
                    databaseIntegration.Driver<MicrosoftDataSqlClientDriver>();
                    databaseIntegration.Dialect<MsSql2008Dialect>();
                    break;

                case DbType.PostgreSQL:
                    databaseIntegration.Dialect<PostgreSQL82Dialect>();
                    break;

                case DbType.PostgreSQL83:
                    databaseIntegration.Dialect<PostgreSQL83Dialect>();
                    break;

                default:
                    throw new ORMException("The Database Engine Type selected is not supported in current version.");
            }
            databaseIntegration.ConnectionString = dbContext.ConnectionString;


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

            // Add to dependency injecton the factory (singleton) and session and unit of work (scoped)
            try
            {
                services.AddSingleton(sessionFactory);

                services.AddScoped((provider) =>
                {
                    // Replace the current instance of session factory with the injected one... it may help DI to avoid premature destruction of the session
                    var factory = provider.GetService<ISessionFactory>();
                    return factory.OpenSession();
                });
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