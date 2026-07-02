using bs.Data;
using bs.Data.Interfaces;
using bs.Data.TestAsync;
using Microsoft.Extensions.DependencyInjection;
using System;

/// <summary>
/// Fixture for PostgreSQL integration tests.
/// Requires the environment variable BSDATA_POSTGRES_CONNSTRING to be set.
/// Example:
///   User ID=postgres;Password=yourpassword;Host=localhost;Port=5432;Database=ormtest;Pooling=true;
/// Tests are skipped if the variable is not defined.
/// </summary>
public class PostgresFixture : IDisposable
{
    private IServiceScope _scope;

    public PostgresFixture()
    {
        ConnectionString = Environment.GetEnvironmentVariable("BSDATA_POSTGRES_CONNSTRING");

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            // Skip: no connection string configured. Tests using this fixture will be skipped.
            return;
        }

        var dbContext = new DbContext
        {
            ConnectionString = ConnectionString,
            DatabaseEngineType = DbType.PostgreSQL83,
            Create = true,
            Update = true,
            LookForEntitiesDllInCurrentDirectoryToo = false,
            SetBatchSize = 25
        };

        var services = new ServiceCollection();
        services.AddBsData(dbContext);
        // Register as Scoped so it shares the same IUnitOfWork (NHibernate session) within the scope.
        services.AddScoped<BsDataRepository>();

        ServiceProvider = services.BuildServiceProvider();
        // Create a single scope for the fixture lifetime so that IUnitOfWork and
        // BsDataRepository are resolved as proper scoped instances, not from the root container.
        _scope = ServiceProvider.CreateScope();
    }

    public string ConnectionString { get; }
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString) && _scope != null;
    public BsDataRepository Repository => _scope?.ServiceProvider.GetRequiredService<BsDataRepository>();
    public ServiceProvider ServiceProvider { get; }
    public IUnitOfWork UnitOfWork => _scope?.ServiceProvider.GetRequiredService<IUnitOfWork>();

    public void Dispose()
    {
        _scope?.Dispose();
        if (ServiceProvider is IDisposable d)
            d.Dispose();
    }
}

/// <summary>
/// Fixture for SQL Server integration tests.
/// Requires the environment variable BSDATA_SQLSERVER_CONNSTRING to be set.
/// Example:
///   database=OrmTest;server=localhost;user=sa;password=yourpassword;TrustServerCertificate=true;
/// Tests are skipped if the variable is not defined.
/// </summary>
public class SqlServerFixture : IDisposable
{
    private IServiceScope _scope;

    public SqlServerFixture()
    {
        ConnectionString = Environment.GetEnvironmentVariable("BSDATA_SQLSERVER_CONNSTRING");

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            // Skip: no connection string configured. Tests using this fixture will be skipped.
            return;
        }

        var dbContext = new DbContext
        {
            ConnectionString = ConnectionString,
            DatabaseEngineType = DbType.MsSql2012,
            Create = true,
            Update = true,
            LookForEntitiesDllInCurrentDirectoryToo = false,
            SetBatchSize = 25
        };

        var services = new ServiceCollection();
        services.AddBsData(dbContext);
        // Register as Scoped so it shares the same IUnitOfWork (NHibernate session) within the scope.
        services.AddScoped<BsDataRepository>();

        ServiceProvider = services.BuildServiceProvider();
        // Create a single scope for the fixture lifetime so that IUnitOfWork and
        // BsDataRepository are resolved as proper scoped instances, not from the root container.
        _scope = ServiceProvider.CreateScope();
    }

    public string ConnectionString { get; }
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString) && _scope != null;
    public BsDataRepository Repository => _scope?.ServiceProvider.GetRequiredService<BsDataRepository>();
    public ServiceProvider ServiceProvider { get; }
    public IUnitOfWork UnitOfWork => _scope?.ServiceProvider.GetRequiredService<IUnitOfWork>();

    public void Dispose()
    {
        _scope?.Dispose();
        if (ServiceProvider is IDisposable d)
            d.Dispose();
    }
}
