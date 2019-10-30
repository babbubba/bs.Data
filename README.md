# bs.Data
Nhibernate based repository

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

# Example repository

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
