using bs.Data.Interfaces;

namespace bs.Data
{
    public class DbContext : IDbContext
    {
        public DbContext()
        {
            UseExecutingAssemblyToo = true;
            SetBatchSize = 20;
        }

        public string ConnectionString { get; set; }
        public DbType DatabaseEngineType { get; set; }
        public string[] FoldersWhereLookingForEntitiesDll { get; set; }
        public string[] EntitiesFileNameScannerPatterns { get; set; }
        public bool LookForEntitiesDllInCurrentDirectoryToo { get; set; }
        public bool UseExecutingAssemblyToo { get; set; }
        public bool Create { get; set; }
        public bool Update { get; set; }
        public short SetBatchSize { get; set; }
    }
}