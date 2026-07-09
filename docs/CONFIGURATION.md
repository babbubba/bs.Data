# Configuration reference

Full reference for every `IDbContext` / `DbContext` option. For a quick working example see the [README](https://github.com/babbubba/bs.Data/blob/master/README.md#quick-start).

- [Minimal configuration](#minimal-configuration)
- [Schema management: `Create` and `Update`](#schema-management-create-and-update)
- [Entity assembly discovery](#entity-assembly-discovery)
- [`Imports`](#imports)
- [`Filters`](#filters)
- [Batch size and logging](#batch-size-and-logging)
- [Database engines reference](#database-engines-reference)

---

## Minimal configuration

Every option below lives on `bs.Data.DbContext` (implements `IDbContext`). Only two are required:

```csharp
var dbContext = new DbContext
{
    ConnectionString = "...",
    DatabaseEngineType = DbType.MsSql2012,
};

builder.Services.AddBsData(dbContext);
```

With no other option set, `AddBsData` will: validate the schema against your mappings at startup (see [Schema management](#schema-management-create-and-update)), discover entities from assemblies already loaded in the current `AppDomain` (see [Entity assembly discovery](#entity-assembly-discovery)), and batch ADO.NET writes 20 at a time.

`AddBsData` builds the NHibernate `ISessionFactory` synchronously, as part of the call itself — configuration errors (bad connection string, schema mismatch, mapping errors) surface immediately as an `ORMException` thrown from `AddBsData`, not lazily on first use.

---

## Schema management: `Create` and `Update`

`Create` and `Update` are independent booleans that together select NHibernate's schema action (`hbm2ddl.auto` equivalent) at startup:

| `Create` | `Update` | Action | Effect |
|---|---|---|---|
| `false` | `false` (default) | **Validate** | No schema changes. Compares the existing database schema against your entity mappings; if they don't match, `AddBsData` throws `ORMException` ("Schema validation failed") listing every mismatch, **before the application starts**. This is the safe default — recommended for staging/production, where schema changes should go through a separate migration process. |
| `false` | `true` | **Update** | Non-destructive: adds missing tables/columns to match the mappings. Does **not** drop or rename anything, and does not reliably alter the type of an existing column. Useful in development or for additive, backward-compatible migrations. |
| `true` | `false` | **Create** | Builds the schema from your mappings. Depending on the database dialect this typically drops pre-existing tables first — treat it as **destructive**. |
| `true` | `true` | **Recreate** | Same as `Create`: schema is dropped and rebuilt from scratch. **Destructive.** |

**Never set `Create = true` against a database whose data you want to keep.** Both destructive options are meant for throwaway/dev/test databases (e.g. the SQLite/PostgreSQL/SQL Server fixtures used by `bs.Data.TestAsync`). In real environments, leave both `false` (schema validation) and manage schema changes with a dedicated migration tool.

---

## Entity assembly discovery

bs.Data locates your entity mapping classes (any class whose companion mapping extends `BsClassMapping<T>` / implements `IPersistentEntity`) via reflection, not via explicit registration. Four options control where it looks:

| Property | Type | Default | What it does |
|---|---|---|---|
| `UseExecutingAssemblyToo` | `bool` | `true` | Scans every assembly already loaded in the current `AppDomain` (i.e. your web app and anything it references), excluding assemblies whose full name contains `"Microsoft"` or `"PresentationCore"`. This is the mechanism that makes the common case — entities live in the same project or a referenced project — work with zero extra configuration. |
| `LookForEntitiesDllInCurrentDirectoryToo` | `bool` | `false` | If `true`, also recursively scans `AppDomain.CurrentDomain.BaseDirectory` for `*.dll` files and loads them with `Assembly.LoadFrom`. Use this when entity assemblies are deployed alongside the app but are **not** referenced/loaded by the host project (e.g. a plugin-style layout). |
| `FoldersWhereLookingForEntitiesDll` | `string[]` | `null` | Additional folders (recursive) to scan for `*.dll` files, on top of (or instead of) the current directory. Same loading mechanism as above. |
| `EntitiesFileNameScannerPatterns` | `string[]` | `null` | Filters that narrow which DLL files (matched by full file path) are loaded, when scanning folders. **These are regular expressions matched with `Regex.IsMatch`, not glob patterns** — despite the visual similarity, `"MyApp.Model.*.dll"` works as a regex too (`.` matches any character, `*` repeats it), but true glob syntax such as `?` for a single character will **not** behave as expected. If left `null`, every `.dll` found in the scanned folders is loaded. |

Only classes that implement `IPersistentEntity` (directly or via a marker interface such as `ILogicallyDeletableEntity`) are picked up from a scanned assembly — unrelated types are ignored, so it's safe to point these settings at folders containing non-entity DLLs.

Typical setups:

```csharp
// Common case: entities live in a project referenced by the host app.
// No extra configuration needed — UseExecutingAssemblyToo already covers it.
var dbContext = new DbContext { ConnectionString = "...", DatabaseEngineType = DbType.MsSql2012 };
```

```csharp
// Entities ship as separate plugin DLLs dropped into a "Plugins" folder
// that isn't referenced by the host project.
var dbContext = new DbContext
{
    ConnectionString = "...",
    DatabaseEngineType = DbType.MsSql2012,
    UseExecutingAssemblyToo = true,
    FoldersWhereLookingForEntitiesDll = new[] { @"C:\MyApp\Plugins" },
    EntitiesFileNameScannerPatterns = new[] { "MyApp\\.Model\\..*\\.dll" }, // regex
};
```

---

## `Imports`

`IDictionary<string, string>`, default `null`. Maps a short name (used in HQL queries via `session.CreateQuery(...)`) to a fully qualified type name (`Type.AssemblyQualifiedName`). This mirrors NHibernate's `Configuration.Imports` and is only needed if you write raw HQL and want to refer to entities by a short alias instead of their full mapped name.

```csharp
dbContext.Imports = new Dictionary<string, string>
{
    { "Product", typeof(ProductModel).AssemblyQualifiedName }
};

// Later, in HQL:
var expensive = await session.CreateQuery("from Product p where p.Price > :p")
    .SetParameter("p", 100m)
    .ListAsync<ProductModel>();
```

Most consumers using LINQ (`Query<T>()`) or `QueryOver<T>()` never need this.

---

## `Filters`

`ICollection<NHibernate.Engine.FilterDefinition>`, default `null`. Passed straight through to `Configuration.AddFilterDefinition(...)` for each entry, wiring up NHibernate's dynamic filters (e.g. row-level filtering such as multi-tenancy or "active only" views that can be toggled per-session with `session.EnableFilter("name")`).

This is a thin pass-through to raw NHibernate — the entity-side of a filter is declared with `.Filter(...)` inside your `BsClassMapping<T>` (exposed via `BsClassCustomizer<T>`), and the filter definition itself follows NHibernate's own API. See the [NHibernate filters documentation](https://nhibernate.info/doc/nhibernate-reference/filters.html) for the exact `FilterDefinition` construction and parameter typing, since the constructor shape can vary across NHibernate versions.

---

## Batch size and logging

| Property | Type | Default | What it does |
|---|---|---|---|
| `SetBatchSize` | `short` | `20` | ADO.NET batch size: how many `INSERT`/`UPDATE` statements NHibernate groups into a single database round-trip. Higher values reduce round-trips for large bulk operations at the cost of larger individual batches; `0` disables ADO.NET batching. This is **not** the same as the internal `25`-item collection-fetch batch size used for lazy-loaded associations (`Global.BATCH_SIZE`), which is fixed and not user-configurable. |
| `LogSqlInConsole` | `bool` | `false` | Prints every generated SQL statement to the console. Development/debugging only — leave `false` in production (verbose, and can leak parameter values into logs/console output). |
| `LogFormattedSql` | `bool` | `false` | When `LogSqlInConsole` is enabled, pretty-prints the SQL instead of a single line. Has no effect on its own. |

---

## Database engines reference

`DatabaseEngineType` selects both the NHibernate `IDriver` and `Dialect`. Supported values:

| `DbType` | Driver | Dialect | Notes |
|---|---|---|---|
| `MsSql2012` | `MicrosoftDataSqlClientDriver` | `MsSql2012Dialect` | SQL Server 2012 or newer. Preferred value for SQL Server. |
| `MsSql2008` | `MicrosoftDataSqlClientDriver` | `MsSql2008Dialect` | For SQL Server 2008 only; use `MsSql2012` for anything newer. |
| `MySQL` | `MySqlDataDriver` | `MySQL55Dialect` | MySQL 5.5. |
| `MySQL57` | `MySqlDataDriver` | `MySQL57Dialect` | MySQL 5.7. |
| `PostgreSQL` | (NHibernate default Npgsql driver) | `PostgreSQL82Dialect` | Legacy; prefer `PostgreSQL83` unless targeting PostgreSQL 8.2. |
| `PostgreSQL83` | (NHibernate default Npgsql driver) | `PostgreSQL83Dialect` | Preferred value for PostgreSQL. `decimal`/`decimal?` properties are automatically mapped to `numeric(19,5)` when this or `PostgreSQL` is selected. |
| `SQLite` | `SQLite20Driver` | `SQLiteDialect` | |
| `Undefined` | — | — | Default value of the enum; `AddBsData` throws `ArgumentException` if this is left unset. |

Connection string examples are in the [README](https://github.com/babbubba/bs.Data/blob/master/README.md#connection-string-examples).
