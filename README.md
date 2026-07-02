# bs.Data

NHibernate-based repository with transaction management, retry policies, and ASP.NET Core integration.

Supports: **SQL Server**, **MySQL**, **PostgreSQL**, **SQLite**

## Install

```
Install-Package bs.Data
```

---

## Configuration

### Register in ASP.NET Core (Program.cs)

```csharp
var dbContext = new DbContext
{
    ConnectionString = "...",
    DatabaseEngineType = DbType.MsSql2012,
    Create = false,
    Update = true,
    SetBatchSize = 25
};

builder.Services.AddBsData(dbContext);
```

Then add the session middleware **before** `UseAuthorization()` and `MapControllers()`:

```csharp
app.UseBsData();
```

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

### DbContext options

| Property | Default | Description |
|---|---|---|
| `ConnectionString` | — | Connection string to the database |
| `DatabaseEngineType` | `Undefined` | Database engine (see `DbType` enum) |
| `Create` | `false` | Drop and recreate the schema on startup |
| `Update` | `false` | Update the schema to match the entity mappings |
| `SetBatchSize` | `20` | Number of INSERT/UPDATE statements per DB round-trip |
| `LogSqlInConsole` | `false` | Print SQL to console |
| `LogFormattedSql` | `false` | Pretty-print the SQL |
| `LookForEntitiesDllInCurrentDirectoryToo` | `true` | Scan the current directory for entity assemblies |
| `UseExecutingAssemblyToo` | `true` | Include the executing assembly in the entity scan |
| `FoldersWhereLookingForEntitiesDll` | `null` | Additional folders to scan for entity assemblies |
| `EntitiesFileNameScannerPatterns` | `null` | Glob patterns to filter DLLs (e.g. `["MyApp.Model.*.dll"]`) |

---

## Entities

### BaseEntity — Guid primary key

```csharp
public class ProductModel : BaseEntity
{
    public virtual string Name { get; set; }
    public virtual decimal Price { get; set; }
}

class ProductModelMap : SubclassMap<ProductModel>
{
    public ProductModelMap()
    {
        Abstract();
        Table("Products");
        Map(x => x.Name).Length(200);
        Map(x => x.Price);
    }
}
```

### BaseAuditableEntity — adds CreationDate and LastUpdateDate

`CreationDate` and `LastUpdateDate` are set automatically by the repository on create and update.

```csharp
public class OrderModel : BaseAuditableEntity
{
    public virtual string Reference { get; set; }
    public virtual decimal Total { get; set; }
}

class OrderModelMap : SubclassMap<OrderModel>
{
    public OrderModelMap()
    {
        Abstract();
        Table("Orders");
        Map(x => x.Reference).Length(50);
        Map(x => x.Total);
    }
}
```

---

## Repository

Extend `Repository` and expose only the operations your aggregate needs:

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

### Logical delete

Implement `ILogicallyDeletableEntity` on your entity, then use the built-in helpers:

```csharp
// Soft-delete (sets IsDeleted = true, DeletionDate = UtcNow)
protected void DeleteLogically<T>(T entity) where T : ILogicallyDeletableEntity

// Restore
protected void RestoreLogically<T>(T entity) where T : ILogicallyDeletableEntity

// Query only active records
protected IQueryable<T> QueryLogicallyNotDeleted<T>() where T : ILogicallyDeletableEntity

// Query only deleted records
protected IQueryable<T> QueryLogicallyDeleted<T>() where T : ILogicallyDeletableEntity
```

---

## Transaction management

### Recommended: RunInTransactionAsync

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

Deadlock detection and exponential back-off retry (default 3 attempts) are built in for SQL Server.

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

---

## Mapping helpers

`BsPropertyContainerCustomizer<T>` adds convenience methods on top of NHibernate's standard mapping:

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
SetManyToMany(x => x.Rooms, "PersonRoom", "PersonId", "RoomId");
```

### DelimitedList user type

Store a `ICollection<string>` as a single delimited column:

```csharp
Map(x => x.Tags).CustomType<DelimitedList>();
```
