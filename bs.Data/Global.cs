namespace bs.Data
{
    internal static class Global
    {
        /// <summary>
        /// Default NHibernate collection batch size for lazy-loaded associations.
        /// Controls how many collection proxies are initialised in a single SELECT when
        /// NHibernate detects multiple uninitialized collections of the same type.
        /// This is distinct from <see cref="Interfaces.IDbContext.SetBatchSize"/>, which
        /// controls the ADO.NET batch size for INSERT/UPDATE statements.
        /// </summary>
        public const int BATCH_SIZE = 25;
    }
}