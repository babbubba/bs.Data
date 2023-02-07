using bs.Data.Interfaces;
using NHibernate.Engine;
using System.Collections.Generic;

namespace bs.Data
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="bs.Data.Interfaces.IDbContext" />
    public class DbContext : IDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        public DbContext()
        {
            UseExecutingAssemblyToo = true;
            SetBatchSize = 20;
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database schema will be created.
        /// </summary>
        /// <value>
        ///   <c>true</c> if create; otherwise, <c>false</c>.
        /// </value>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the type of the database engine.
        /// </summary>
        /// <value>
        /// The type of the database engine.
        /// </value>
        public DbType DatabaseEngineType { get; set; }

        /// <summary>
        /// Gets or sets the patterns used to search for libraries to treath as entities in the folders setted in the 'FoldersWhereLookingForEntitiesDll' field.
        /// </summary>
        /// <value>
        /// The entities file name scanner patterns.
        /// </value>
        public string[] EntitiesFileNameScannerPatterns { get; set; }

        public ICollection<FilterDefinition> Filters { get; set; }

        /// <summary>
        /// Gets or sets the folders where looking for entities DLL (for example external model in external libraries).
        /// </summary>
        /// <value>
        /// The folders where looking for entities DLL.
        /// </value>
        public string[] FoldersWhereLookingForEntitiesDll { get; set; }

        /// <summary>
        /// Gets or sets the classes to imports to use as Model in CreateQuery (entityName/className -> AssemblyQualifiedName).
        /// </summary>
        /// <value>
        /// The imports.
        /// </value>
        public IDictionary<string, string> Imports { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [log formatted SQL].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log formatted SQL]; otherwise, <c>false</c>.
        /// </value>
        public bool LogFormattedSql { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [log SQL in console].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log SQL in console]; otherwise, <c>false</c>.
        /// </value>
        public bool LogSqlInConsole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [look for entities DLL in current directory too].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [look for entities DLL in current directory too]; otherwise, <c>false</c>.
        /// </value>
        public bool LookForEntitiesDllInCurrentDirectoryToo { get; set; }

        /// <summary>
        /// Gets or sets the number of inserts or writes to database that will be executed in unique roundtrip.
        /// </summary>
        /// <value>
        /// The size of the set batch.
        /// </value>
        public short SetBatchSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database schema will be update.
        /// </summary>
        /// <value>
        ///   <c>true</c> if update; otherwise, <c>false</c>.
        /// </value>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use executing assembly too] to search entities.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use executing assembly too]; otherwise, <c>false</c>.
        /// </value>
        public bool UseExecutingAssemblyToo { get; set; }
    }
}