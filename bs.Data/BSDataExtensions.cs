using bs.Data.Helpers;
using bs.Data.Interfaces;
using bs.Data.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using System;
using System.Linq;

namespace bs.Data
{

    public static class BSDataExtensions
    {
        /// <summary>
        /// Registers NHibernate Session Factory, Session, and Unit of Work in the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="ormContext">The database context containing ORM configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddBsData(this IServiceCollection services, IDbContext ormContext)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(ormContext);

            ValidateContext(ormContext);

            var sessionFactory = BuildSessionFactory(ormContext);
            RegisterServices(services, sessionFactory);

            return services;
        }

        /// <summary>
        /// Aggiunge il middleware per la gestione delle sessioni NHibernate.
        /// va messo prima di UseAuthorization() e MapControllers();
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseBsData(this IApplicationBuilder app)
        {
            return app.UseMiddleware<NHibernateSessionMiddleware>();
        }

        private static void ValidateContext(IDbContext context)
        {
            if (context.DatabaseEngineType == DbType.Undefined)
            {
                throw new ArgumentException("Database engine type must be specified.", nameof(context.DatabaseEngineType));
            }

            if (string.IsNullOrWhiteSpace(context.ConnectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(context.ConnectionString));
            }
        }

        private static ISessionFactory BuildSessionFactory(IDbContext context)
        {
            var mapper = CreateMapper(context);
            AddEntityMappings(mapper, context);

            var domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            domainMapping.autoimport = true;

            var configuration = ConfigureNHibernate(context, domainMapping);

            try
            {
                return configuration.BuildSessionFactory();
            }
            catch (SchemaValidationException ex)
            {
                var errors = string.Join("\n- ", ex.ValidationErrors);
                throw new ORMException($"Schema validation failed:\n- {errors}", ex);
            }
            catch (Exception ex)
            {
                throw new ORMException("Failed to build NHibernate session factory. See inner exception for details.", ex);
            }
        }

        private static ModelMapper CreateMapper(IDbContext context)
        {
            var mapper = new ModelMapper();

            // Configure decimal mapping for PostgreSQL
            if (context.DatabaseEngineType == DbType.PostgreSQL ||
                context.DatabaseEngineType == DbType.PostgreSQL83)
            {
                mapper.BeforeMapProperty += ConfigurePostgreSqlDecimalMapping;
            }

            return mapper;
        }

        private static void ConfigurePostgreSqlDecimalMapping(
            IModelInspector modelInspector,
            PropertyPath propertyPath,
            IPropertyMapper propertyCustomizer)
        {
            var memberType = propertyPath.LocalMember.GetPropertyOrFieldType();

            if (memberType == typeof(decimal) || memberType == typeof(decimal?))
            {
                propertyCustomizer.Type(NHibernateUtil.Decimal);
                propertyCustomizer.Precision(19);
                propertyCustomizer.Scale(5);
                propertyCustomizer.Column(c => c.SqlType("numeric(19,5)"));
            }
        }

        private static void AddEntityMappings(ModelMapper mapper, IDbContext context)
        {
            // Add mappings from current assembly
            mapper.AddMappings(typeof(BSDataExtensions).Assembly.ExportedTypes);

            // Add mappings from external assemblies
            try
            {
                var assemblies = ReflectionHelper.GetAssembliesFromFiles(
                    context.FoldersWhereLookingForEntitiesDll,
                    context.EntitiesFileNameScannerPatterns,
                    context.LookForEntitiesDllInCurrentDirectoryToo,
                    context.UseExecutingAssemblyToo);

                mapper.AddMappings(assemblies.SelectMany(a => a.ExportedTypes));
            }
            catch (Exception ex)
            {
                throw new ORMException("Failed to load entity mappings via reflection. See inner exception for details.", ex);
            }
        }

        private static Configuration ConfigureNHibernate(IDbContext context, HbmMapping domainMapping)
        {
            var configuration = new Configuration();
            var databaseIntegration = new NHibernate.Cfg.Loquacious.DbIntegrationConfigurationProperties(configuration);

            // Add imports and filters
            AddImportsAndFilters(configuration, context);

            // Configure database dialect and driver
            ConfigureDatabaseDialect(databaseIntegration, context.DatabaseEngineType);

            databaseIntegration.ConnectionString = context.ConnectionString;
            databaseIntegration.SchemaAction = DetermineSchemaAction(context);
            databaseIntegration.LogFormattedSql = context.LogFormattedSql;
            databaseIntegration.LogSqlInConsole = context.LogSqlInConsole;
            databaseIntegration.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;

            // Apply batch size from context configuration (controls number of INSERT/UPDATE per DB round-trip)
            if (context.SetBatchSize > 0)
                configuration.SetProperty("adonet.batch_size", context.SetBatchSize.ToString());

            try
            {
                configuration.AddMapping(domainMapping);
            }
            catch (Exception ex)
            {
                throw new ORMException("Failed to add entity mappings to NHibernate configuration. See inner exception for details.", ex);
            }

            return configuration;
        }

        private static void AddImportsAndFilters(Configuration configuration, IDbContext context)
        {
            if (context.Imports != null)
            {
                foreach (var import in context.Imports)
                {
                    configuration.Imports.Add(import);
                }
            }

            if (context.Filters != null)
            {
                foreach (var filter in context.Filters)
                {
                    configuration.AddFilterDefinition(filter);
                }
            }
        }

        private static void ConfigureDatabaseDialect(NHibernate.Cfg.Loquacious.DbIntegrationConfigurationProperties integration, DbType dbType)
        {
            switch (dbType)
            {
                case DbType.MySQL:
                    integration.Driver<MySqlDataDriver>();
                    integration.Dialect<MySQL55Dialect>();
                    break;

                case DbType.MySQL57:
                    integration.Driver<MySqlDataDriver>();
                    integration.Dialect<MySQL57Dialect>();
                    break;

                case DbType.SQLite:
                    integration.Driver<SQLite20Driver>();
                    integration.Dialect<SQLiteDialect>();
                    break;

                case DbType.MsSql2012:
                    integration.Driver<MicrosoftDataSqlClientDriver>();
                    integration.Dialect<MsSql2012Dialect>();
                    break;

                case DbType.MsSql2008:
                    integration.Driver<MicrosoftDataSqlClientDriver>();
                    integration.Dialect<MsSql2008Dialect>();
                    break;

                case DbType.PostgreSQL:
                    integration.Dialect<PostgreSQL82Dialect>();
                    break;

                case DbType.PostgreSQL83:
                    integration.Dialect<PostgreSQL83Dialect>();
                    break;

                default:
                    throw new ORMException($"Database engine type '{dbType}' is not supported.");
            }
        }

        private static SchemaAutoAction DetermineSchemaAction(IDbContext context)
        {
            if (context.Create && context.Update)
                return SchemaAutoAction.Recreate;

            if (context.Create)
                return SchemaAutoAction.Create;

            if (context.Update)
                return SchemaAutoAction.Update;

            return SchemaAutoAction.Validate;
        }

        private static void RegisterServices(IServiceCollection services, ISessionFactory sessionFactory)
        {
            try
            {
                // Register SessionFactory as singleton
                services.AddSingleton(sessionFactory);

                // Register ISession as scoped with proper disposal
                services.AddScoped(provider =>
                {
                    var factory = provider.GetRequiredService<ISessionFactory>();
                    var session = factory.OpenSession();

                    // Il container DI chiamerà (dovrebbe) Dispose automaticamente a fine scope
                    return session;
                });

                // Register UnitOfWork as scoped
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            }
            catch (Exception ex)
            {
                throw new ORMException("Failed to register services in dependency injection container. See inner exception for details.", ex);
            }
        }
    }
}