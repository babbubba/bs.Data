using bs.Data;
using bs.Data.Interfaces;
using bs.Data.TestAsync;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

/// <summary>
/// Base fixture that provisions a real, disposable database for integration tests, builds the
/// schema through NHibernate (DbContext.Create/Update) and applies a baseline seed of reference
/// data. The database is provisioned fresh for every test collection run and torn down
/// afterwards. For SQL Server/PostgreSQL this means a Testcontainers container (only a working
/// Docker daemon is required, no external server); for SQLite - embedded and serverless - it means
/// a temp file created on the fly and deleted afterwards.
/// </summary>
public abstract class DatabaseFixtureBase : IAsyncLifetime
{
    /// <summary>
    /// Baseline reference data seeded into every fresh database before the tests run, so tests
    /// exercise a realistic, already-populated schema instead of a completely empty one.
    /// </summary>
    private static readonly string[] SeedCountries = ["Italy", "France", "Germany", "Spain"];

    private IServiceScope _scope;

    protected DatabaseFixtureBase(DbType databaseEngineType)
    {
        DatabaseEngineType = databaseEngineType;
    }

    public string ConnectionString { get; private set; }
    public bool IsConfigured => _scope != null;
    public BsDataRepository Repository => _scope?.ServiceProvider.GetRequiredService<BsDataRepository>();
    public ServiceProvider ServiceProvider { get; private set; }
    public IUnitOfWork UnitOfWork => _scope?.ServiceProvider.GetRequiredService<IUnitOfWork>();

    /// <summary>
    /// The NHibernate database engine/dialect this fixture's container speaks. Exposed so tests
    /// can stand up additional, independently configured <see cref="ServiceProvider"/> instances
    /// (e.g. against a connection string with a constrained pool size) without duplicating the
    /// engine-specific fixture logic.
    /// </summary>
    public DbType DatabaseEngineType { get; }

    public async Task InitializeAsync()
    {
        ConnectionString = await ProvisionDatabaseAndGetConnectionStringAsync();

        ServiceProvider = BuildServiceProvider(ConnectionString, createSchema: true);
        // Create a single scope for the fixture lifetime so that IUnitOfWork and
        // BsDataRepository are resolved as proper scoped instances, not from the root container.
        _scope = ServiceProvider.CreateScope();

        await SeedAsync();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        if (ServiceProvider is IDisposable d)
            d.Dispose();

        await TearDownAsync();
    }

    /// <summary>
    /// Builds an independent <see cref="ServiceProvider"/> (its own <see cref="ISessionFactory"/>,
    /// hence its own ADO.NET connection pool) against this fixture's already-provisioned database.
    /// </summary>
    /// <remarks>
    /// Used by tests that need to control the connection pool directly (e.g. constraining
    /// <c>Max Pool Size</c> to make a connection leak fail fast and deterministically) without
    /// disturbing the schema or the data seeded/created by other tests sharing this fixture: the
    /// schema is only ever (re)created once, by <see cref="InitializeAsync"/> - every additional
    /// provider built here targets the existing schema (<c>Create = false, Update = false</c>).
    /// </remarks>
    public ServiceProvider BuildServiceProvider(string connectionString, bool createSchema = false)
    {
        var dbContext = new DbContext
        {
            ConnectionString = connectionString,
            DatabaseEngineType = DatabaseEngineType,
            Create = createSchema,
            Update = createSchema,
            LookForEntitiesDllInCurrentDirectoryToo = false,
            SetBatchSize = 25
        };

        var services = new ServiceCollection();
        services.AddBsData(dbContext);
        services.AddScoped<BsDataRepository>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Returns a variant of <paramref name="connectionString"/> with the ADO.NET connection pool
    /// capped at <paramref name="maxPoolSize"/> and a short connect/pool-wait timeout, so that a
    /// leaked connection turns into a fast, deterministic test failure instead of a slow hang or
    /// only showing up in production once the default (100-connection) pool is exhausted.
    /// </summary>
    public abstract string WithConstrainedPool(string connectionString, int maxPoolSize);

    /// <summary>
    /// Returns a variant of <paramref name="connectionString"/> with a short default command
    /// timeout, so a command left queued behind another one wedged on the same, improperly shared
    /// connection fails the test quickly instead of waiting out the driver's default (30s).
    /// </summary>
    public abstract string WithShortCommandTimeout(string connectionString, int commandTimeoutSeconds);

    /// <summary>
    /// Provisions the ephemeral database (container, or temp file for SQLite) and returns a
    /// connection string pointing at a ready-to-use, empty database.
    /// </summary>
    protected abstract Task<string> ProvisionDatabaseAndGetConnectionStringAsync();

    /// <summary>
    /// Tears down whatever <see cref="ProvisionDatabaseAndGetConnectionStringAsync"/> provisioned
    /// (stops/removes the container, or deletes the temp file).
    /// </summary>
    protected abstract Task TearDownAsync();

    private async Task SeedAsync()
    {
        var uow = _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repo = _scope.ServiceProvider.GetRequiredService<BsDataRepository>();

        uow.BeginTransaction();
        foreach (var name in SeedCountries)
            await repo.CreateCountryAsync(new CountryModel { Name = name });
        await uow.TryCommitOrRollbackAsync();
    }
}

/// <summary>
/// Fixture for PostgreSQL integration tests. Spins up a disposable PostgreSQL container
/// (Testcontainers) for the lifetime of the test collection - no external server or configuration
/// required.
/// </summary>
public class PostgresFixture : DatabaseFixtureBase
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("ormtest")
        .Build();

    public PostgresFixture() : base(DbType.PostgreSQL83)
    {
    }

    public override string WithConstrainedPool(string connectionString, int maxPoolSize) =>
        new NpgsqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = maxPoolSize,
            MinPoolSize = 0,
            Timeout = 5
        }.ConnectionString;

    public override string WithShortCommandTimeout(string connectionString, int commandTimeoutSeconds) =>
        new NpgsqlConnectionStringBuilder(connectionString)
        {
            CommandTimeout = commandTimeoutSeconds
        }.ConnectionString;

    protected override async Task<string> ProvisionDatabaseAndGetConnectionStringAsync()
    {
        await _container.StartAsync();
        return _container.GetConnectionString();
    }

    protected override Task TearDownAsync() => _container.DisposeAsync().AsTask();
}

/// <summary>
/// Fixture for SQL Server integration tests. Spins up a disposable SQL Server container
/// (Testcontainers) for the lifetime of the test collection - no external server or configuration
/// required.
/// </summary>
public class SqlServerFixture : DatabaseFixtureBase
{
    private const string DatabaseName = "OrmTest";

    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public SqlServerFixture() : base(DbType.MsSql2012)
    {
    }

    public override string WithConstrainedPool(string connectionString, int maxPoolSize) =>
        new SqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = maxPoolSize,
            MinPoolSize = 0,
            ConnectTimeout = 5
        }.ConnectionString;

    public override string WithShortCommandTimeout(string connectionString, int commandTimeoutSeconds) =>
        new SqlConnectionStringBuilder(connectionString)
        {
            CommandTimeout = commandTimeoutSeconds
        }.ConnectionString;

    protected override async Task<string> ProvisionDatabaseAndGetConnectionStringAsync()
    {
        await _container.StartAsync();

        // The SQL Server image only ever boots with its system databases, so the target
        // database has to be created explicitly before NHibernate can build the schema in it.
        await _container.ExecScriptAsync($"CREATE DATABASE [{DatabaseName}];");

        var connectionStringBuilder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = DatabaseName
        };
        return connectionStringBuilder.ConnectionString;
    }

    protected override Task TearDownAsync() => _container.DisposeAsync().AsTask();
}

/// <summary>
/// Fixture for SQLite integration tests. SQLite is embedded/serverless, so "ephemeral
/// infrastructure" here means a fresh temp file created on the fly and deleted at the end of the
/// test collection - no server or container of any kind is involved.
/// </summary>
public class SqliteFixture : DatabaseFixtureBase
{
    private string _databaseFilePath;

    public SqliteFixture() : base(DbType.SQLite)
    {
    }

    public override string WithConstrainedPool(string connectionString, int maxPoolSize) =>
        // System.Data.SQLite's connection pool only exposes an on/off switch, not a numeric max
        // size (maxPoolSize is intentionally unused). Disabling it means every connection is a
        // fresh native handle, so one that isn't properly closed leaves the database file's write
        // lock held for the next attempt - the closest SQLite analogue to exhausting a network
        // connection pool. A generous busy timeout lets genuinely concurrent (non-leaked) writers
        // queue up and wait for the file lock rather than failing with SQLITE_BUSY immediately.
        new SQLiteConnectionStringBuilder(connectionString)
        {
            Pooling = false,
            BusyTimeout = 5000
        }.ConnectionString;

    public override string WithShortCommandTimeout(string connectionString, int commandTimeoutSeconds) =>
        // SQLite has no server-side command timeout; BusyTimeout (how long to wait for another
        // connection's lock on the database file before giving up) is the closest analogue.
        new SQLiteConnectionStringBuilder(connectionString)
        {
            BusyTimeout = commandTimeoutSeconds * 1000
        }.ConnectionString;

    protected override Task<string> ProvisionDatabaseAndGetConnectionStringAsync()
    {
        _databaseFilePath = Path.Combine(Path.GetTempPath(), $"bsdata-test-{Guid.NewGuid():N}.sqlite");

        var connectionString = new SQLiteConnectionStringBuilder
        {
            DataSource = _databaseFilePath,
            Pooling = true
        }.ConnectionString;

        return Task.FromResult(connectionString);
    }

    protected override Task TearDownAsync()
    {
        // SQLite keeps a native handle open per pooled connection; clear the pool first so the
        // temp file isn't still locked when we try to delete it.
        SQLiteConnection.ClearAllPools();

        if (_databaseFilePath != null && File.Exists(_databaseFilePath))
            File.Delete(_databaseFilePath);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Groups every test class that needs a SQL Server container under one collection so xunit
/// creates (and tears down) a single <see cref="SqlServerFixture"/>/container shared by all of
/// them, instead of one per test class.
/// </summary>
[CollectionDefinition(Name)]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer collection";
}

/// <summary>
/// Groups every test class that needs a PostgreSQL container under one collection so xunit
/// creates (and tears down) a single <see cref="PostgresFixture"/>/container shared by all of
/// them, instead of one per test class.
/// </summary>
[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres collection";
}

/// <summary>
/// Groups every test class that needs a SQLite database under one collection so xunit creates
/// (and tears down) a single <see cref="SqliteFixture"/>/temp file shared by all of them, instead
/// of one per test class.
/// </summary>
[CollectionDefinition(Name)]
public class SqliteCollection : ICollectionFixture<SqliteFixture>
{
    public const string Name = "Sqlite collection";
}
