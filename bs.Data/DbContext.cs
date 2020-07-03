using bs.Data.Interfaces;

namespace bs.Data
{
    public class DbContext : IDbContext
    {
        public DbContext()
        {
            UseExecutingAssemblyToo = true;
            SetBatchSize = 20;
            SessionContext = SessionContext.call;
        }

        public string ConnectionString { get; set; }

        //public string DatabaseEngineType { get; set; }
        public DbType DatabaseEngineType { get; set; }

        public string[] FoldersWhereLookingForEntitiesDll { get; set; }
        public string[] EntitiesFileNameScannerPatterns { get; set; }
        public bool LookForEntitiesDllInCurrentDirectoryToo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use executing assembly too]. It would be true usually because it is needed using base types like BaseEntity class.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use executing assembly too]; otherwise, <c>false</c>.
        /// </value>
        public bool UseExecutingAssemblyToo { get; set; }

        public bool Create { get; set; }
        public bool Update { get; set; }
        public short SetBatchSize { get; set; }
        public SessionContext SessionContext { get; set; }
    }
}