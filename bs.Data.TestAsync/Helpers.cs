using bs.Data;
using bs.Data.Interfaces;
using bs.Data.TestAsync;
using Microsoft.Extensions.DependencyInjection;
using System;

public class PostgresFixture : IDisposable
{
    public PostgresFixture()
    {
        var dbContext = new DbContext
        {
            ConnectionString = "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=ormtest;Pooling=true;Connection Lifetime=0;",
            DatabaseEngineType = DbType.PostgreSQL83,
            Create = true,
            Update = true,
            LookForEntitiesDllInCurrentDirectoryToo = false,
            SetBatchSize = 25
        };

        var services = new ServiceCollection();
        services.AddBsData(dbContext);
        services.AddSingleton<BsDataRepository>();

        ServiceProvider = services.BuildServiceProvider();
    }

    public BsDataRepository Repository => ServiceProvider.GetRequiredService<BsDataRepository>();
    public ServiceProvider ServiceProvider { get; }
    public IUnitOfWork UnitOfWork => ServiceProvider.GetRequiredService<IUnitOfWork>();

    public void Dispose()
    {
        if (ServiceProvider is IDisposable d)
            d.Dispose();
    }
}

public class SqlServerFixture : IDisposable
{
    public SqlServerFixture()
    {
        var dbContext = new DbContext
        {
            ConnectionString = "database=OrmTest; server=192.168.254.13; user=sa; password=Password01; TrustServerCertificate=true;",
            DatabaseEngineType = DbType.MsSql2012,
            Create = true,
            Update = true,
            LookForEntitiesDllInCurrentDirectoryToo = false,
            SetBatchSize = 25
        };

        var services = new ServiceCollection();
        services.AddBsData(dbContext);
        services.AddSingleton<BsDataRepository>();

        ServiceProvider = services.BuildServiceProvider();
    }

    public BsDataRepository Repository => ServiceProvider.GetRequiredService<BsDataRepository>();
    public ServiceProvider ServiceProvider { get; }
    public IUnitOfWork UnitOfWork => ServiceProvider.GetRequiredService<IUnitOfWork>();

    public void Dispose()
    {
        if (ServiceProvider is IDisposable d)
            d.Dispose();
    }
}