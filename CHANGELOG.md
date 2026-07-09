# Changelog

All notable changes to bs.Data will be documented in this file.

---

## [5.4.3] - 2026-07-09

### Fixed

- **[BUG-06] Deadlock retry back-off state was a shared, never-reset, process-wide singleton** — `UnitOfWorkExtensions.RunInTransactionAsync` read its exponential back-off policy from `RetryPolicies.ExponentialBackOff`, a `static readonly` singleton shared by *every* call across *every* concurrent transaction for the lifetime of the process. Its mutable wait-interval field only ever grew and was never reset. In practice this meant: (1) the per-call `retry` limit was not actually enforced as documented, because a *new* `SqlServerRetryPolicy` (with its own retry counter) was constructed fresh on every iteration of the retry loop instead of once per call, so the counter never accumulated past 1; and (2) far more seriously, once enough *cumulative* deadlocks anywhere in the running application pushed the shared back-off interval past its 200ms cap, deadlock retry silently and permanently stopped working for the rest of the process's life, with each subsequent deadlock incurring an ever-growing `Task.Delay` before failing immediately with no retry. Fixed by replacing the singleton with `RetryPolicies.CreateExponentialBackOff()`, a factory called once per call to `RunInTransactionAsync`/`RunInTransactionAsync<T>` (before the retry loop begins), giving every transaction its own independent, correctly-scoped retry state. Covered by `RetryPolicyTests` (state-isolation unit tests) and a new live-deadlock integration test in `SqlServerTests`.
- **[BUG-07] Deadlock detection likely missed exceptions NHibernate wraps in `GenericADOException`** — The retry logic only caught a bare `Microsoft.Data.SqlClient.SqlException`. NHibernate commonly wraps raw ADO.NET provider exceptions (including deadlocks) in its own `ADOException`/`GenericADOException` hierarchy before they reach calling code, which would not match that `catch` clause and fell through to the generic `ADOException` handler — skipping the deadlock check and the retry entirely, and misclassifying `ORMException.ExceptionOrigin` as `"ADO"` instead of `"SQL"`. Fixed by unwrapping the exception chain (`InnerException`) to locate a wrapped `SqlException` before giving up on retry/classification, in `RunInTransaction<T>`, `RunInTransactionAsync(Action, int)` (obsolete overload), and `RunInTransactionAsync<T>`.
- **[BUG-08] Stale entities could linger across a deadlock retry** — On a deadlock retry, `RunInTransactionAsync` rolled back the failed transaction and began a new one on the *same* NHibernate session, without clearing the session's first-level cache. Entities loaded/modified during the rolled-back attempt remained attached and dirty, risking double-applied in-memory changes or a `StaleObjectStateException` on the retried attempt. Fixed by calling `Session.Clear()` immediately before retrying.
- **[BUG-09] `ExceptionOrigin` was always `null` on `RunInTransaction(Action)`** — Unlike every other `RunInTransaction*` overload, the plain `RunInTransaction(Action)` wrapped failures in `ORMException` without classifying `ExceptionOrigin` as `"SQL"`/`"ADO"`/`"GENERIC"`, so consumer code branching on `ExceptionOrigin` (e.g. `catch (ORMException ex) when (ex.ExceptionOrigin == "SQL")`) would never match here even for a genuine SQL error. Now classifies consistently with the other overloads (this overload still does not retry on deadlock, matching its previous behavior — only the value-returning/async overloads retry).
- **[BUG-10] Deadlock retry crashed with `NHibernate.TransactionException` instead of retrying** — When SQL Server kills a transaction to resolve a deadlock, the connection is left "zombied": `ITransaction.WasRolledBack` is still `false` (nothing here called `Rollback`), yet issuing `RollbackAsync()` against it throws `TransactionException` ("Transaction not connected, or was disconnected"). That exception was thrown from inside the `catch (ADOException)`/`catch (SqlException)` blocks that were trying to evaluate the deadlock for a retry, so it escaped uncaught instead of being retried — a sibling `catch` never catches an exception raised by another `catch` in the same `try`. Caught by the new live-deadlock integration test (below), which failed intermittently against a real, ephemeral SQL Server container until fixed. Fixed via a `SafeRollbackAsync` helper that tolerates an already-aborted transaction, used in `RunInTransaction`, `RunInTransaction<T>`, `RunInTransactionAsync(Action, int)` (obsolete overload) and `RunInTransactionAsync<T>`.
- **[BUG-11] `UnitOfWork.Dispose()`/`DisposeAsync()` silently *committed* an abandoned transaction instead of rolling it back** — If a `IUnitOfWork` was disposed (e.g. its DI scope ended) while a transaction was still active — because the caller's own `Commit`/`TryCommitOrRollback` never ran, typically due to an unhandled exception or an early `return` — `Dispose()` called `TryCommitOrRollback()`, which tries `Commit()` **first**. As long as nothing failed at the ADO/NHibernate level, this silently persisted whatever partial work had been done, rather than rolling it back. This contradicts how a bare NHibernate `ITransaction` (and essentially every other ADO.NET/ORM transaction type — `SqlTransaction`, EF Core's `IDbContextTransaction`, `System.Transactions`) behaves when disposed without an explicit `Commit()`: they roll back. Found by a new integration test that deliberately abandons a scope mid-transaction. Fixed: `Dispose()`/`DisposeAsync()` now call `Rollback()`/`RollbackAsync()` (never `Commit`) for a transaction still active at disposal time.

These bugs were found while auditing the exception-handling paths after the [5.4.2] `FlushAndClear` review (see "Documentation" below), and while stress-testing session/transaction lifecycle around DI scoping (BUG-11); none were reported by a consumer, but the shared-singleton bug and the commit-on-abandon bug in particular affect every application using this library against a real database and would silently degrade or corrupt data, so all consumers should upgrade.

### Documentation

- **README rewritten and corrected** — The "Entities" section previously showed `BaseEntity`/`BaseAuditableEntity` base classes and a `SubclassMap<T>` mapping example that do not exist in this library (entities are plain classes implementing the `IPersistentEntity` marker interface, mapped via `BsClassMapping<T>`). Examples now match the real API and are aligned with the working entities in `bs.Data.TestAsync`.
- **`FlushAndClear`/`FlushAndClearAsync` documented** — Added in 5.4.2 but never covered in the README ("Aggiornata la documentazione" in that commit only updated XML doc comments and `CHANGELOG.md`, not `README.md`). Now documented under "Transaction management", including the requirement to call it inside an explicit transaction for atomicity and the risk of reusing entity references after `Session.Clear()` evicts them (relevant in particular across `RunInTransactionAsync` deadlock retries).
- **Fixed incorrect default** — README stated `LookForEntitiesDllInCurrentDirectoryToo` defaults to `true`; `DbContext`'s constructor only initializes `UseExecutingAssemblyToo` and `SetBatchSize`, so the actual default is `false`. Corrected.
- **Clarified `EntitiesFileNameScannerPatterns`** — Previously documented as "glob patterns"; they are actually matched with `Regex.IsMatch` against the full scanned file path (regular expressions, not glob syntax). Documented accurately with the caveat that some glob-looking patterns work by coincidence.
- **New `docs/CONFIGURATION.md`** — Full reference for every `IDbContext` option (schema action semantics for every `Create`/`Update` combination, entity assembly discovery mechanism, `Imports`, `Filters`, database engine/dialect table). README keeps a concise quick-reference table and links to it.
- **New "Error handling" section in README** — Documents `ORMException` and `ExceptionOrigin` ("SQL"/"ADO"/"GENERIC"), previously undocumented.

The `FlushAndClear`/`FlushAndClearAsync` implementation added in [5.4.2] was reviewed for regressions as part of this pass and found to be additive and non-breaking within this repository (its sole implementer, `UnitOfWork`, is a `sealed` class) — the bugs above were found separately, while tracing how exceptions propagate through `RunInTransactionAsync` to answer that review.

### Testing

- **`[assembly: InternalsVisibleTo("bs.Data.TestAsync")]`** added (`bs.Data/Properties/AssemblyInfo.cs`) so the (internal) deadlock retry policies can be unit-tested directly without needing a live database connection to exercise their state-management logic.
- **New `RetryPolicyTests`** — fast, DB-free unit tests proving `ExponentialBackOffPolicy` state is no longer shared/leaked across independent calls (BUG-06).
- **New `SqlServerTests.RunInTransactionAsync_RetriesAndSucceeds_OnRealDeadlock`** — live integration test that forces a genuine SQL Server deadlock between two concurrent transactions and asserts both complete successfully via automatic retry.
- **`SqlServerFixture`/`PostgresFixture` rewritten to use [Testcontainers](https://dotnet.testcontainers.org/) instead of externally configured servers** — `bs.Data.TestAsync` now depends on `Testcontainers.MsSql`/`Testcontainers.PostgreSql`. Each fixture spins up a disposable, real SQL Server / PostgreSQL container for the lifetime of its test class (`IAsyncLifetime`), lets NHibernate build the schema against it (`Create`/`Update`), seeds a small baseline of reference `CountryModel` rows, and tears the container down afterwards. This removes the previous `BSDATA_SQLSERVER_CONNSTRING`/`BSDATA_POSTGRES_CONNSTRING` environment variables and the tests' runtime `Assert.Skip` when they were unset (which no longer compiled against xunit 2.9.3 — `Assert.Skip` is an xunit v3 API) — integration tests now always run against real engines, locally and in CI, with no external database to provision, only a Docker daemon.
- **CI (`.github/workflows/dotnet.yml`) now runs `dotnet test`** in addition to `dotnet build` — ubuntu-latest runners have Docker available out of the box for the Testcontainers-backed integration tests. Also installs the .NET 9 SDK alongside .NET 8 (`bs.Data` multi-targets `net8.0;net9.0`) and uploads TRX test results as a build artifact.
- **Fixed `bs.Data.sln`** — the `bs.Data.TestAsync` project had no `Build.0` mapping for `Debug|Any CPU`/`Debug|x86`/`Release|Any CPU`/`Release|x64`, so `dotnet build`/`dotnet test` against the solution (the default configuration) silently skipped the test project entirely without any error. Added the missing mappings.
- **`SqlServerFixture`/`PostgresFixture` moved to `ICollectionFixture`** (`[Collection("SqlServer collection")]`/`"Postgres collection"`) so every test class needing a given engine shares one container/`ISessionFactory` instead of paying for a fresh one per class. Fixtures also expose `DatabaseEngineType`, `BuildServiceProvider(connectionString, createSchema)` (an independently pooled provider against the *same*, already-provisioned database — schema is only ever (re)created once), `WithConstrainedPool(connectionString, maxPoolSize)` and `WithShortCommandTimeout(connectionString, seconds)`, so tests can exercise pool/timeout behavior without disturbing the shared fixture state.
- **New `TransactionAndSessionLifecycleTests`** — replicates the two most common real-world connection-management failure modes against SQL Server, PostgreSQL, **and now SQLite**:
  - *"Too many open sessions"*: many concurrent DI scopes under a deliberately constrained pool (`Max Pool Size=5`, 25 concurrent scopes) complete without leaking a connection; a sequential churn loop under a single-connection pool (20 iterations, `Max Pool Size=1`) proves every scope releases its connection on `Dispose`; an abandoned, never-committed scope both rolls back (BUG-11) and returns its connection to the pool, so the very next scope can still acquire one.
  - *"Shared sessions"*: firing concurrent queries through one `IUnitOfWork`/`ISession` (simulating a scoped service mistakenly fanned out to `Task.Run`/`Parallel.ForEach`, or registered as a singleton) fails loudly (`ThrowsAnyAsync<Exception>`) instead of silently corrupting data, since neither NHibernate sessions nor the ADO.NET connection they wrap are thread-safe; calling `BeginTransaction()` twice on the same `IUnitOfWork` without closing the first throws a clear `ORMException` rather than a confusing NHibernate-level failure; using a `IUnitOfWork` after its owning DI scope has been disposed throws `ObjectDisposedException` instead of operating on a closed/returned connection.
- **New `SqliteFixture`/`SqliteTests`** — SQLite is embedded/serverless, so its "ephemeral infrastructure" is a temp file created on the fly and deleted afterwards, not a container; `DatabaseFixtureBase`'s container-oriented abstract methods were renamed to engine-agnostic `ProvisionDatabaseAndGetConnectionStringAsync`/`TearDownAsync` to accommodate it. `SqliteTests` mirrors the SQL Server/PostgreSQL functional test minus the deadlock-retry scenario (deadlock detection here is SQL-Server-specific, keyed off `Microsoft.Data.SqlClient.SqlException`).
- **SQLite needed its own take on the "shared session" scenario** in `TransactionAndSessionLifecycleTests` (now `virtual`, overridden by `SqliteTransactionAndSessionLifecycleTests`): starting several async calls back-to-back never overlaps, since `System.Data.SQLite` executes against the local file essentially synchronously; even forcing genuine OS threads through a `Barrier` (so every thread reaches the shared session at the same instant) doesn't throw, because `System.Data.SQLite` is built with SQLite's "serialized" threading mode and tolerates concurrent multi-threaded use of one connection by silently queuing it. What it does **not** tolerate safely: this same test caught concurrent writers on a shared session **silently losing one of several concurrent inserts with no exception at all** — the persistence context NHibernate keeps in memory is a plain, non-thread-safe collection. Sharing a session across threads is still the wrong pattern (on every engine); for SQLite specifically the test now asserts the weaker, honest invariant that data is never *fabricated/duplicated*, while documenting that silent loss is the known, accepted symptom of this misuse here.

---

## [5.4.2] - 2026-07-02

### Added

- **`IUnitOfWork.FlushAndClear()` / `FlushAndClearAsync()`** — nuovi metodi per il pattern flush+clear di NHibernate. `FlushAsync()` invia i pending change al DB dentro la transazione corrente senza committare; `Clear()` svuota poi la first-level cache (identity map) liberando memoria. Utile nei processi batch per evitare la crescita illimitata della sessione:
  ```csharp
  for (int i = 0; i < rows.Count; i++)
  {
      await repo.CreateAsync(rows[i]);
      if (i % 100 == 0) await uow.FlushAndClearAsync();
  }
  ```

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
