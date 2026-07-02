# Changelog

All notable changes to bs.Data will be documented in this file.

---

## [5.4.1] - 2026-07-02

### Dependencies

- **NHibernate** `5.5.2` → `5.6.1` — aggiunge supporto nativo per net9.0 e correzioni varie.
- **Microsoft.Data.SqlClient** `6.0.1` → `7.0.1` — miglioramenti TLS/autenticazione, correzioni di sicurezza. Salto major: testare se si usano funzionalità avanzate di SqlClient direttamente (bs.Data lo usa solo tramite interfacce ADO.NET standard, impatto atteso nullo).
- **MySql.Data** `9.2.0` → `9.7.0` — patch/minor cumulativi, nessun breaking change nella serie 9.x.
- **Npgsql** `9.0.3` → `9.0.5` — patch di stabilità. Npgsql 10 (major) NON viene adottato: NHibernate 5.x non è stato ancora verificato con Npgsql 10.
- **System.Text.Json** `9.0.4` → `9.0.16` — patch di sicurezza e performance.
- **Microsoft.Extensions.DependencyInjection.Abstractions** `9.0.0` → `9.0.16` — patch di sicurezza.
- **System.Data.SQLite.Core** `1.0.119` — già all'ultima versione disponibile, nessuna modifica.

---

## [5.4.0] - 2026-07-02

### Changed

- **[QA-03] XML documentation added to marker interfaces** — `IPersistentEntity` and `IRepository` now carry `<summary>` and `<remarks>` XML doc comments explaining the intentional empty-interface pattern and its role as a compile-time constraint. Improves IntelliSense for consumers.

- **[QA-04] Dead code removed from BsClassCustomizer** — Removed the commented-out `SetManyToOne` override at the bottom of `BsClassCustomizer<TEntity>`. The method is already implemented in the base class `BsPropertyContainerCustomizer<TEntity>` and the comment was stale.

- **[QA-04] Global.BATCH_SIZE documented** — Added XML doc to `Global.BATCH_SIZE` clarifying that this constant controls NHibernate collection batch-fetching (lazy-loaded association initialisation), which is distinct from `IDbContext.SetBatchSize` (ADO.NET INSERT/UPDATE batching).

- **[INFRA-01] Multi-targeting: net8.0 + net9.0** — `bs.Data.csproj` now targets `net8.0;net9.0`. This allows consumers on .NET 9 to use native framework bindings without retargeting.

### Fixed

- **[TEST-03] DI scoping in integration test fixtures** — `PostgresFixture` and `SqlServerFixture` were resolving `IUnitOfWork` and `BsDataRepository` directly from the root `ServiceProvider`, capturing scoped services as singletons (captive dependency). Both fixtures now create an `IServiceScope` and resolve all per-test services from its `ServiceProvider`. `BsDataRepository` registration changed from `AddSingleton` to `AddScoped`.

---

## [5.3.0] - 2026-07-02

### Changed

- **[QA-01] Retry policy is now fully async** — `IRetryPolicy` (internal) now exposes `PerformRetryAsync(SqlException)` returning `Task<bool>`. `ExponentialBackOffPolicy` uses `Task.Delay` instead of `Thread.Sleep`, releasing the thread during deadlock back-off waits. `ChainingPolicy` and `SqlServerRetryPolicy` updated accordingly. No public API change.

- **[QA-02] Removed redundant NuGet package references** — `System.Net.Http` (4.3.4) and `System.Text.RegularExpressions` (4.3.1) are included in the net8.0 SDK and have been removed from the explicit package references. `Microsoft.AspNetCore.Http.Abstractions` (2.2.0) has been replaced by a `FrameworkReference` to `Microsoft.AspNetCore.App`, aligning with the recommended approach for net8.0 class libraries that depend on ASP.NET Core types.

- **[DOC-01] README rewritten for API v5** — All code examples now use the current API (`BeginTransaction()` returns `void`, `TryCommitOrRollback()`, `RunInTransactionAsync`). Added a complete transaction management section with examples for `RunInTransactionAsync`, manual transactions, and explicit rollback.

- **[DOC-02] MySQL connection string example updated** — Replaced `SslMode=none` with `SslMode=Required` in the MySQL configuration example.

### Fixed

- **[TEST-01] Fixed fire-and-forget async in integration tests** — `rooms.ForEach(async a => await ...)` replaced with `foreach (var room in rooms) await ...` in `PostgresTests` and `SqlServerTests`. Previously, exceptions during room creation were silently swallowed and tests could pass with incomplete data.

### Changed (tests)

- **[TEST-02] Hardcoded credentials removed from test source** — `PostgresFixture` and `SqlServerFixture` now read connection strings from environment variables `BSDATA_POSTGRES_CONNSTRING` and `BSDATA_SQLSERVER_CONNSTRING`. Tests are skipped gracefully if the variables are not set.

---

## [5.2.2] - 2026-07-02

### Fixed

- **[BUG-01] Deadlock retry now works correctly** — `SqlServerExceptions.IsThisADeadlock()` was comparing against `SqlException.ErrorCode` (the COM HResult, always `-2146232060`) instead of `SqlException.Number` (the SQL Server error number, `1205` for deadlock victim). As a result, the exponential back-off retry in `RunInTransactionAsync` never triggered on deadlock. Now correctly uses `realException.Number == 1205`.

- **[BUG-02] SetBatchSize configuration is now applied** — `IDbContext.SetBatchSize` was exposed in the configuration interface but never passed to NHibernate. The value is now applied via `configuration.SetProperty("adonet.batch_size", ...)` during session factory setup.

- **[BUG-05] MySQL NHibernate driver is now explicitly set** — `DbType.MySQL` and `DbType.MySQL57` now explicitly configure `MySqlDataDriver`, consistent with the SQLite and SQL Server cases. Previously the driver was left to NHibernate's default resolution.

### Deprecated

- **[BUG-04] `RunInTransactionAsync(IUnitOfWork, Action, int)`** — This overload accepts a synchronous `Action` delegate which cannot properly await async operations inside the delegate body. Marked `[Obsolete]`. Use `RunInTransactionAsync<T>(IUnitOfWork, Func<Task<T>>, int)` instead.

### Notes

- `UnitOfWork.Dispose()` intentionally attempts to commit an open transaction as a safety net. Applications should always close transactions explicitly (via `RunInTransaction`/`RunInTransactionAsync`) before the DI container disposes the scope. This behavior is documented but unchanged.

---

## [5.2.1] - previous release

Initial tracked version.
