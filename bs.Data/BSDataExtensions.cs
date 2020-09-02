using bs.Data.Helpers;
using bs.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using System.Linq;

namespace bs.Data
{
    public static class BSDataExtensions
    {
        public static IServiceCollection AddBsData(this IServiceCollection services, IDbContext dbContext)
        {
            // Create the mapper
            var mapper = new ModelMapper();

            // Add the entities defined in this assemblies
            mapper.AddMappings(typeof(BSDataExtensions).Assembly.ExportedTypes);
            System.Diagnostics.Debug.WriteLine("ORM - Mapped internal types: " + string.Join(",", typeof(BSDataExtensions).Assembly.ExportedTypes.Select(t => t.Name)));

            // Add the entities defined in other assemblies
            var modelsAssemblies = ReflectionHelper.GetAssembliesFromFiles(dbContext.FoldersWhereLookingForEntitiesDll, dbContext.EntitiesFileNameScannerPatterns, dbContext.LookForEntitiesDllInCurrentDirectoryToo, dbContext.UseExecutingAssemblyToo);
            mapper.AddMappings(modelsAssemblies.SelectMany(a => a.ExportedTypes));
            System.Diagnostics.Debug.WriteLine("ORM - Mapped external types: " + string.Join(",", modelsAssemblies.SelectMany(a => a.ExportedTypes).Select(t => t.Name)));

            // Compile mapped entities
            HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            // Prepares configuration
            var configuration = new Configuration();
            var databaseIntegration = new NHibernate.Cfg.Loquacious.DbIntegrationConfigurationProperties(configuration);

            // It use the right database integration properties by the database type choosen
            switch (dbContext.DatabaseEngineType)
            {
                case DbType.MySQL:
                    databaseIntegration.Dialect<MySQL55Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    databaseIntegration.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                    databaseIntegration.LogFormattedSql = true;
                    databaseIntegration.LogSqlInConsole = true;
                    break;

                case DbType.SQLite:
                    databaseIntegration.Driver<NHibernate.Driver.SQLite20Driver>();
                    databaseIntegration.Dialect<SQLiteDialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    databaseIntegration.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                    databaseIntegration.LogFormattedSql = true;
                    databaseIntegration.LogSqlInConsole = true;
                    break;

                case DbType.MsSql2012:
                    databaseIntegration.Driver<NHibernate.Driver.SqlClientDriver>();
                    databaseIntegration.Dialect<MsSql2012Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    databaseIntegration.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                    databaseIntegration.LogFormattedSql = true;
                    databaseIntegration.LogSqlInConsole = true;
                    break;

                case DbType.MsSql2008:
                    databaseIntegration.Driver<NHibernate.Driver.SqlClientDriver>();
                    databaseIntegration.Dialect<MsSql2008Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    databaseIntegration.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                    databaseIntegration.LogFormattedSql = true;
                    databaseIntegration.LogSqlInConsole = true;
                    break;

                case DbType.PostgreSQL:
                    databaseIntegration.Dialect<PostgreSQL82Dialect>();
                    databaseIntegration.ConnectionString = dbContext.ConnectionString;
                    databaseIntegration.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                    databaseIntegration.LogFormattedSql = true;
                    databaseIntegration.LogSqlInConsole = true;
                    break;

                default:
                    break;
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

            // Add entities mapping to configuration
            configuration.AddMapping(domainMapping);

            // Create the session factory
            var sessionFactory = configuration.BuildSessionFactory();

            // Add to dependency injecton the factory (singleton) and session and unit of work scoped
            services.AddSingleton(sessionFactory);
            services.AddScoped(factory => sessionFactory.OpenSession());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}