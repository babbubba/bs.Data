# bs.Data

NHibernate-based repository with transaction management, retry policies, and ASP.NET Core integration.

Targets **.NET 8** and **.NET 9**. Supports **SQL Server**, **MySQL**, **PostgreSQL**, **SQLite**.

## Contents

- [Install](#install)
- [Quick start](#quick-start)
- [Configuration](#configuration)
- [Entities](#entities)
- [Repository](#repository)
- [Logical delete](#logical-delete)
- [Transaction management](#transaction-management)
- [Mapping helpers](#mapping-helpers)
- [Error handling](#error-handling)

---

## Install

```
Install-Package bs.Data
```

---

## Quick start

This is a complete, minimal, working setup: one entity, one mapping, one repository, DI registration, and a create/read call.

**1. Define the entity.** Entities are plain classes with `virtual` members (required by NHibernate for lazy-loading proxies) that implement the marker interface `IPersistentEntity`. There is no base class to inherit from — the interface is all that's required.

```csharp
using bs.Data.Interfaces.BaseEntities;

public class ProductModel : IPersistentEntity
{
    public virtual Guid Id { get; set; }
    public virtual string Name { get; set; }
    public virtual decimal Price { get; set; }
}
```

**2. Map it.** Extend `BsClassMapping<T>` and describe the table. `GuidId` wires up a GUID primary key generated client-side.

```csharp
using bs.Data.Mapping;

public class ProductModelMap : BsClassMapping<ProductModel>
{
    public ProductModelMap()
    {
        Table("Products");
        GuidId(x => x.Id);
        PropertyText(x => x.Name, 200);
        Property(x => x.Price); // standard NHibernate mapping API is also available
    }
}
```

**3. Write a repository.** Extend `Repository` and expose only what your aggregate needs; the base class's CRUD/query members are `protected`.

```csharp
using bs.Data;
using bs.Data.Interfaces;

public class ProductRepository : Repository
{
    public ProductRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    public async Task CreateAsync(ProductModel product) => await base.CreateAsync(product);

    public async Task<ProductModel> GetByIdAsync(Guid id) => await base.GetByIdAsync<ProductModel>(id);

    public async Task<IList<ProductModel>> GetAllAsync() => await Query<ProductModel>().ToListAsync();
}
```

**4. Register and wire up the middleware** (`Program.cs`):

```csharp
var dbContext = new DbContext
{
    ConnectionString = "Server=localhost;Database=MyDb;User Id=sa;Password=yourpassword;TrustServerCertificate=true;",
    DatabaseEngineType = DbType.MsSql2012,
    Update = true, // dev/staging: keep schema in sync; see Configuration below
};

builder.Services.AddBsData(dbContext);
builder.Services.AddScoped<ProductRepository>();

// ...

app.UseBsData(); // before UseAuthorization() and MapControllers()
```

**5. Use it** (e.g. in a controller or a service resolved from DI):

```csharp
public class ProductsController : ControllerBase
{
    private readonly ProductRepository _repo;
    private readonly IUnitOfWork _uow;

    public ProductsController(ProductRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    [HttpPost]
    public async Task<ProductModel> Create(string name, decimal price)
    {
        return await _uow.RunInTransactionAsync(async () =>
        {
            var product = new ProductModel { Name = name, Price = price };
            await _repo.CreateAsync(product);
            return product;
        });
    }

    [HttpGet("{id}")]
    public async Task<ProductModel> Get(Guid id) => await _repo.GetByIdAsync(id);
}
```

`AddBsData` registers `ISessionFactory` (singleton), `ISession` (scoped) and `IUnitOfWork` (scoped) in the DI container. `UseBsData()` only guarantees the `IUnitOfWork`/session is disposed at the end of each request — it does **not** manage transactions; use `RunInTransactionAsync` (or the manual APIs below) for that.

---

## Configuration

`DbContext` (implements `IDbContext`) is a plain settings object you build up and pass to `AddBsData`. Two properties are mandatory, everything else has a default:

```csharp
var dbContext = new DbContext
{
    ConnectionString = "...",       // required
    DatabaseEngineType = DbType.MsSql2012, // required
    Create = false,                 // default: false
    Update = false,                 // default: false — see "Schema management" below
    SetBatchSize = 20,              // default: 20
    LogSqlInConsole = false,        // default: false
    LogFormattedSql = false,        // default: false
    LookForEntitiesDllInCurrentDirectoryToo = false, // default: false
    UseExecutingAssemblyToo = true, // default: true
    FoldersWhereLookingForEntitiesDll = null,   // default: null
    EntitiesFileNameScannerPatterns = null,     // default: null
    Imports = null,                 // default: null
    Filters = null,                 // default: null
};
```

| Property | Type | Default | Summary |
|---|---|---|---|
| `ConnectionString` | `string` | — *(required)* | Connection string to the database. See [connection string examples](#connection-string-examples). |
| `DatabaseEngineType` | `DbType` | `Undefined` *(required)* | Selects the NHibernate driver/dialect. `AddBsData` throws if left `Undefined`. |
| `Create` | `bool` | `false` | Drop and rebuild the schema at startup. **Destructive.** See [Schema management](https://github.com/babbubba/bs.Data/blob/master/docs/CONFIGURATION.md#schema-management-create-and-update). |
| `Update` | `bool` | `false` | Add missing tables/columns to match the mappings, without dropping anything. See [Schema management](https://github.com/babbubba/bs.Data/blob/master/docs/CONFIGURATION.md#schema-management-create-and-update). |
| `SetBatchSize` | `short` | `20` | Number of `INSERT`/`UPDATE` statements grouped per DB round-trip. |
| `LogSqlInConsole` | `bool` | `false` | Print generated SQL to the console (dev only). |
| `LogFormattedSql` | `bool` | `false` | Pretty-print the SQL when `LogSqlInConsole` is on. |
| `LookForEntitiesDllInCurrentDirectoryToo` | `bool` | `false` | Also scan the app's base directory for entity DLLs not otherwise loaded. |
| `UseExecutingAssemblyToo` | `bool` | `true` | Also scan assemblies already loaded in the current `AppDomain`. Covers the common case (entities in the same/referenced project) with no extra config. |
| `FoldersWhereLookingForEntitiesDll` | `string[]` | `null` | Extra folders to scan for entity DLLs. |
| `EntitiesFileNameScannerPatterns` | `string[]` | `null` | Regex filters (not glob) applied to scanned file paths. |
| `Imports` | `IDictionary<string,string>` | `null` | HQL short-name → `AssemblyQualifiedName` aliases. |
| `Filters` | `ICollection<FilterDefinition>` | `null` | Raw NHibernate dynamic filter definitions. |

For the full explanation of each option — including exactly what happens for every `Create`/`Update` combination, how assembly discovery works, and worked examples for `Imports`/`Filters` — see **[docs/CONFIGURATION.md](https://github.com/babbubba/bs.Data/blob/master/docs/CONFIGURATION.md)**.

### Connection string examples

**SQL Server**
```csharp
DatabaseEngineType = DbType.MsSql2012,
ConnectionString = "Server=localhost;Database=MyDb;User Id=sa;Password=yourpassword;TrustServerCertificate=true;"
```

**MySQL 5.5**
```csharp
DatabaseEngineType = DbType.MySQL,
ConnectionString = "Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=yourpassword;SslMode=Required;"
```

**MySQL 5.7**
```csharp
DatabaseEngineType = DbType.MySQL57,
ConnectionString = "Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=yourpassword;SslMode=Required;"
```

**PostgreSQL**
```csharp
DatabaseEngineType = DbType.PostgreSQL83,
ConnectionString = "User ID=postgres;Password=yourpassword;Host=localhost;Port=5432;Database=mydb;Pooling=true;"
```

**SQLite**
```csharp
DatabaseEngineType = DbType.SQLite,
ConnectionString = "Data Source=.\\mydb.db;Version=3;BinaryGuid=False;"
```

See [docs/CONFIGURATION.md](https://github.com/babbubba/bs.Data/blob/master/docs/CONFIGURATION.md#database-engines-reference) for the full driver/dialect table.

---

## Entities

There is no `BaseEntity` base class to inherit from. An entity is any class that implements one or more of the marker interfaces below and maps its members with `BsClassMapping<T>` (or plain NHibernate mapping-by-code):

| Interface | Purpose |
|---|---|
| `IPersistentEntity` | Required on every entity. Empty marker interface; constrains generic `Repository` methods (`Create<T>`, `GetById<T>`, ...) to types that are actually mapped. |
| `IAuditableEntity` | Adds `CreationDate` / `LastUpdateDate` (`DateTime?`). The repository sets these automatically on `Create`/`CreateAsync` and `Update`/`UpdateAsync` — you never set them yourself. |
| `ILogicallyDeletableEntity` | Adds `IsDeleted` (`bool`) and `DeletionDate` (`DateTime?`). Enables the soft-delete helpers described in [Logical delete](#logical-delete). Extends `IPersistentEntity`, so implementing it alone is enough. |

All mapped properties must be declared `virtual` (NHibernate proxies rely on it for lazy loading and change tracking).

```csharp
using bs.Data.Interfaces.BaseEntities;
using bs.Data.Mapping;

public class OrderModel : IPersistentEntity, IAuditableEntity, ILogicallyDeletableEntity
{
    public virtual Guid Id { get; set; }
    public virtual string Reference { get; set; }
    public virtual decimal Total { get; set; }
    public virtual DateTime? CreationDate { get; set; }
    public virtual DateTime? LastUpdateDate { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime? DeletionDate { get; set; }
}

public class OrderModelMap : BsClassMapping<OrderModel>
{
    public OrderModelMap()
    {
        Table("Orders");
        GuidId(x => x.Id);
        PropertyText(x => x.Reference, 50);
        Property(x => x.Total);
        Property(x => x.CreationDate);
        Property(x => x.LastUpdateDate);
        Property(x => x.IsDeleted);
        Property(x => x.DeletionDate);
    }
}
```

`BsClassMapping<T>` gives you both the standard NHibernate mapping-by-code API (`Property`, `Table`, `Id`, ...) and the [bs.Data mapping helpers](#mapping-helpers) (`PropertyText`, `SetManyToOne`, ...) on the same class.

---

## Repository

Extend `Repository` and expose only the operations your aggregate needs — every CRUD/query member on the base class is `protected`, so consumers can only call what you deliberately expose:

```csharp
public class ProductRepository : Repository
{
    public ProductRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    public async Task CreateAsync(ProductModel product)
        => await base.CreateAsync(product);

    public async Task<ProductModel> GetByIdAsync(Guid id)
        => await base.GetByIdAsync<ProductModel>(id);

    public async Task UpdateAsync(ProductModel product)
        => await base.UpdateAsync(product);

    public async Task DeleteAsync(ProductModel product)
        => await base.DeleteAsync(product);

    public async Task<IList<ProductModel>> GetAllAsync()
        => await Query<ProductModel>().ToListAsync();

    public IQueryable<ProductModel> GetByPrice(decimal max)
        => Query<ProductModel>().Where(p => p.Price <= max);
}
```

`Repository` implements the empty marker interface `IRepository`; you don't need to implement it yourself, but you can depend on `IRepository` instead of a concrete repository type where you just need "some repository" (e.g. generic infrastructure code).

Register each concrete repository with the DI container (typically `AddScoped<ProductRepository>()`), alongside `AddBsData(...)`.

---

## Logical delete

Implement `ILogicallyDeletableEntity` on your entity, then use the built-in helpers exposed as `protected` members on `Repository`:

```csharp
// Soft-delete (sets IsDeleted = true, DeletionDate = UtcNow)
protected void DeleteLogically<T>(T entity) where T : IPersistentEntity, ILogicallyDeletableEntity
protected Task DeleteLogicallyAsync<T>(T entity) where T : IPersistentEntity, ILogicallyDeletableEntity

// Restore
protected void RestoreLogically<T>(T entity) where T : IPersistentEntity, ILogicallyDeletableEntity
protected Task RestoreLogicallyAsync<T>(T entity) where T : IPersistentEntity, ILogicallyDeletableEntity

// Query only active / only deleted records
protected IQueryable<T> QueryLogicallyNotDeleted<T>() where T : IPersistentEntity, ILogicallyDeletableEntity
protected IQueryable<T> QueryLogicallyDeleted<T>() where T : IPersistentEntity, ILogicallyDeletableEntity
```

These only flag the row — `DeleteAsync`/`Delete` still remove the row physically, they're independent operations.

---

## Transaction management

### Recommended: `RunInTransactionAsync`

The safest way to run operations in a transaction. Handles commit, rollback, and deadlock retry automatically.

```csharp
// Returns a value
var product = await _uow.RunInTransactionAsync(async () =>
{
    var p = new ProductModel { Name = "Widget", Price = 9.99m };
    await _repo.CreateAsync(p);
    return p;
});

// Void (no return value): wrap in a Task<bool> or use a local variable
await _uow.RunInTransactionAsync(async () =>
{
    await _repo.DeleteAsync(product);
    return true;
});
```

Deadlock detection and exponential back-off retry (default 3 attempts) are built in for SQL Server. Because the delegate can run more than once on retry, **it must be idempotent** — avoid side effects outside the ORM (e.g. sending an email) inside it, or guard them separately. Before each retry, the session's first-level cache is cleared automatically, so any entity you loaded during the failed attempt is stale/detached once the delegate runs again — reload it instead of reusing the reference.

> **Deprecated:** `RunInTransactionAsync(IUnitOfWork, Action, int)` (the overload taking a synchronous `Action`) is obsolete — it cannot correctly await async work inside the delegate. Use the `Func<Task<T>>` overload above.

### Manual transaction management

Use `BeginTransaction` / `TryCommitOrRollback` when you need explicit control:

```csharp
// Create
_uow.BeginTransaction();
await _repo.CreateAsync(entity);
await _uow.TryCommitOrRollbackAsync();

// Update
_uow.BeginTransaction();
entity.Price = 19.99m;
await _repo.UpdateAsync(entity);
await _uow.TryCommitOrRollbackAsync();

// Delete
_uow.BeginTransaction();
await _repo.DeleteAsync(entity);
await _uow.TryCommitOrRollbackAsync();

// Read (no transaction needed for reads)
var product = await _repo.GetByIdAsync(id);
```

### Explicit rollback

```csharp
_uow.BeginTransaction();
try
{
    await _repo.CreateAsync(entity);
    await _uow.CommitAsync();
}
catch
{
    await _uow.RollbackAsync();
    throw;
}
finally
{
    _uow.CloseTransaction();
}
```

### Batch loops: `FlushAndClear` / `FlushAndClearAsync`

NHibernate's session keeps every entity it touches in an in-memory identity map (the "first-level cache") for the lifetime of the session. In long batch loops (thousands of inserts/updates in one request or one background job), that map grows unbounded and can exhaust memory well before the transaction commits.

`IUnitOfWork.FlushAndClear()` / `FlushAndClearAsync()` push pending changes to the database (`Session.Flush`) and then evict everything from the identity map (`Session.Clear`), without ending the transaction:

```csharp
await _uow.RunInTransactionAsync(async () =>
{
    for (int i = 0; i < rows.Count; i++)
    {
        await _repo.CreateAsync(MapRow(rows[i]));
        if (i % 100 == 0) await _uow.FlushAndClearAsync();
    }
    return true;
});
```

Two things to keep in mind:

- **Call it inside an explicit transaction** (`RunInTransactionAsync` or `BeginTransaction`). `Flush()` only pushes SQL to the database within the current transaction — it does not commit. If there is no active transaction when you call `FlushAndClear`, each statement is effectively auto-committed individually by the underlying ADO.NET driver as it's flushed, which defeats the "all-or-nothing" guarantee you'd normally get from wrapping the loop in a transaction.
- **`Clear()` detaches every entity from the session**, including ones you may still be holding a reference to. Don't reuse an entity instance for a further `Update`/`Delete` call after a `FlushAndClear` unless you reload it first (`GetById`/`GetByIdAsync`) — passing a stale, detached instance back into NHibernate can throw or silently misbehave. This also applies across a deadlock retry inside `RunInTransactionAsync`: the delegate runs again from the top on the same session, so any local state built before the last `FlushAndClear` should be treated as gone.

---

## Mapping helpers

`BsPropertyContainerCustomizer<T>` (available on any `BsClassMapping<T>`) adds convenience methods on top of NHibernate's standard mapping-by-code API:

```csharp
// Text columns
PropertyText(x => x.Code, length: 50);          // nvarchar(50)
PropertyLongText(x => x.Description, 4000);     // nvarchar(max) / text
PropertyBlob(x => x.Photo);                     // varbinary(max) / bytea

// Unique constraint
PropertyUnique(x => x.Email, "UQ_User_Email");

// UTC datetime
PropertyUtcDate(x => x.EventDate);

// Relationships
SetManyToOne(x => x.Country, "CountryId");
SetOneToMany(x => x.Addresses, "PersonId", typeof(AddressModel));
SetManyToMany(x => x.Rooms, "PersonRoom", "PersonId", "RoomId", typeof(RoomModel), cascadeAll: false);
```

You can freely mix these with plain NHibernate calls (`Property`, `Component`, ...) on the same mapping class — `BsClassMapping<T>` doesn't hide or replace the underlying API, it extends it.

### `DelimitedList` user type

Store an `ICollection<string>` / `IList<string>` as a single delimited column:

```csharp
Property(x => x.Tags, map => map.Type<DelimitedList>());
```

---

## Error handling

Configuration and mapping failures raised during `AddBsData(...)` (bad connection string, missing/invalid `DatabaseEngineType`, schema validation failures, mapping errors) are wrapped in `bs.Data.Helpers.ORMException` so callers don't need to catch NHibernate-specific exception types directly.

At runtime, the value-returning overloads `RunInTransaction<T>(Func<T>)` and `RunInTransactionAsync<T>(Func<Task<T>>)` also wrap failures in `ORMException`, setting `ExceptionOrigin` to give you a coarse category without inspecting the inner exception:

| `ExceptionOrigin` | Raised from |
|---|---|
| `"SQL"` | A `Microsoft.Data.SqlClient.SqlException` (non-deadlock, or deadlock retries exhausted). |
| `"ADO"` | A generic `NHibernate.ADOException`. |
| `"GENERIC"` | Any other exception. |

The original exception is always available via `ORMException.InnerException`.

```csharp
try
{
    await _uow.RunInTransactionAsync(async () => { await _repo.CreateAsync(entity); return true; });
}
catch (ORMException ex) when (ex.ExceptionOrigin == "SQL")
{
    // e.g. a real (non-deadlock) SQL error
}
```

---

For release history see [CHANGELOG.md](https://github.com/babbubba/bs.Data/blob/master/CHANGELOG.md). For the full configuration reference see [docs/CONFIGURATION.md](https://github.com/babbubba/bs.Data/blob/master/docs/CONFIGURATION.md).
