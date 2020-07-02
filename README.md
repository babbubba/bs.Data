# bs.Data
Nhibernate based repository using Fluent Nhibernate.

# Install
### Nuget
     Install-Package bs.Data
# Configuration
Example config for Sqlite database:

     private static IUnitOfWork CreateUnitOfWork_Sqlite()
     {
         var dbContext = new DbContext
         {
             ConnectionString = "Data Source=.\\bs.Data.Test.db;Version=3;BinaryGuid=False;",
             DatabaseEngineType = "sqlite",
             Create = true,
             Update = true,
             LookForEntitiesDllInCurrentDirectoryToo = true
         };
         var uOW = new UnitOfWork(dbContext);
         return uOW;
     }

Example config for MySql database:

     private static IUnitOfWork CreateUnitOfWork_Mysql()
     {
        string server_ip = "localhost";
        string server_port = "3307";
        string database_name = "database";
        string db_user_name = "root";
        string db_user_password = "password";
        var dbContext = new DbContext
        {
            ConnectionString = $"Server={server_ip};Port={server_port};Database={database_name};Uid={db_user_name};Pwd={db_user_password};SslMode=none",
            DatabaseEngineType = "mysql",
            Create = true,
            Update = true,
            LookForEntitiesDllInCurrentDirectoryToo = true
         };
         var uOW = new UnitOfWork(dbContext);
         return uOW;
     }

# Models (entities)
Use Fluent Nhibernate to map your entities.

## BaseEntity
Use BaseEntity class for normal entities. It implements Guid type Id field.

Example:

    public class TestEntityModel : BaseEntity
    {
        public virtual string StringValue { get; set ; }
        public virtual int IntValue { get; set ; }
        public virtual DateTime DateTimeValue { get; set ; }
    }

    class TestEntityModelMap : SubclassMap<TestEntityModel>
    {
        public TestEntityModelMap()
        {
            // indicates that the base class is abstract
            Abstract();

            Table("TestEntity");
            Map(x => x.StringValue);
            Map(x => x.IntValue);
            Map(x => x.DateTimeValue);
        }
    }

## BaseAuditableEntity
Use BaseAuditableEntity class for auditable entities. 

It implements Guid type Id field and DateTime? type CreationDate and LastUpdateDate fields. 

The base repository will automatically populate the fields on creation and on update.

Example:

    public class TestAuditableEntityModel : BaseAuditableEntity
    {
        public virtual string StringValue { get; set; }
        public virtual int IntValue { get; set; }
        public virtual DateTime DateTimeValue { get; set; }
    }

    class TestAuditableEntityModelMap : SubclassMap<TestAuditableEntityModel>
    {
        public TestAuditableEntityModelMap()
        {
            // indicates that the base class is abstract
            Abstract();

            Table("TestAuditableEntity");
            Map(x => x.StringValue);
            Map(x => x.IntValue);
            Map(x => x.DateTimeValue);
        }
    }

# Repository
## Basic repository example

    public class TestRepository : Repository
    {
        public TestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        internal new void Create<T>(T entityToCreate) where T : IPersistentEntity
        {
            base.Create<T>(entityToCreate);
        }

        internal new T GetById<T>(Guid id) where T : IPersistentEntity
        {
           return base.GetById<T>(id);
        }

        internal new void Update<T>(T entity) where T : IPersistentEntity
        {
            base.Update<T>(entity);
        }

        internal new void Delete<T>(Guid id) where T : IPersistentEntity
        {
            base.Delete<T>(id);
        }
    }

## Using Repository

       public void TestRepositoryEntities()
        {
            IUnitOfWork uOW = CreateUnitOfWork(); //See the 'Configuration' chapter above...
            var repository = new TestRepository(uOW); //See the 'Basic repository example' chapter above...

            #region Create Entity
            var entityToCreate = new TestEntityModel
            {
                DateTimeValue = DateTime.Now,
                IntValue = 1,
                StringValue = "Test"
            };
            using (var transaction = uOW.BeginTransaction())
            {            
                repository.Create<TestEntityModel>(entityToCreate);
                // Transaction autocommit or rollback if exception occurs when disposed.
            }
            #endregion

            #region Retrieve Entity
            var entity = repository.GetById<TestEntityModel>(entityToCreate.Id);
            #endregion

            #region Update Entity
            var transaction = uOW.BeginTransaction()
            entity.IntValue = 2;
            entity.StringValue = "edited";
            repository.Update(entity);
            uOW.Commit(transaction); // simply commit (it can rise exceptions)
            #endregion

            #region Delete Entity
            var transaction = uOW.BeginTransaction()
            repository.Delete<TestEntityModel>(entity.Id);
            uOW.TryCommitOrRollback(transaction); // It tries to commit but if error occurs it execute rollback
            #endregion

            uOW.Dispose();
        }
