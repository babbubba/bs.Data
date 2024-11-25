using NHibernate.Engine;
using System.Collections.Generic;

namespace bs.Data.Interfaces
{
    /// <summary>
    /// The ORM configuration
    /// </summary>
    public interface IDbContext
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database schema will be created.
        /// </summary>
        /// <value>
        ///   <c>true</c> if create; otherwise, <c>false</c>.
        /// </value>
        bool Create { get; set; }

        /// <summary>
        /// Gets or sets the type of the database engine.
        /// </summary>
        /// <value>
        /// The type of the database engine.
        /// </value>
        DbType DatabaseEngineType { get; set; }

        /// <summary>
        /// Gets or sets the patterns used to search for libraries to treath as entities in the folders setted in the 'FoldersWhereLookingForEntitiesDll' field.
        /// </summary>
        /// <value>
        /// The entities file name scanner patterns.
        /// </value>
        string[] EntitiesFileNameScannerPatterns { get; set; }

        ICollection<FilterDefinition> Filters { get; set; }

        /// <summary>
        /// Gets or sets the folders where looking for entities DLL (for example external model in external libraries).
        /// </summary>
        /// <value>
        /// The folders where looking for entities DLL.
        /// </value>
        string[] FoldersWhereLookingForEntitiesDll { get; set; }

        /// <summary>
        /// Gets or sets the classes to imports to use as Model in CreateQuery (entityName/className -> AssemblyQualifiedName).
        /// </summary>
        /// <value>
        /// The imports.
        /// </value>
        IDictionary<string, string> Imports { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [log formatted SQL].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log formatted SQL]; otherwise, <c>false</c>.
        /// </value>
        bool LogFormattedSql { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [log SQL in console].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log SQL in console]; otherwise, <c>false</c>.
        /// </value>
        bool LogSqlInConsole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [look for entities DLL in current directory too].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [look for entities DLL in current directory too]; otherwise, <c>false</c>.
        /// </value>
        bool LookForEntitiesDllInCurrentDirectoryToo { get; set; }

        /// <summary>
        /// Gets or sets the number of inserts or writes to database that will be executed in unique roundtrip.
        /// </summary>
        /// <value>
        /// The size of the set batch.
        /// </value>
        short SetBatchSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database schema will be update.
        /// </summary>
        /// <value>
        ///   <c>true</c> if update; otherwise, <c>false</c>.
        /// </value>
        bool Update { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use executing assembly too] to search entities.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use executing assembly too]; otherwise, <c>false</c>.
        /// </value>
        bool UseExecutingAssemblyToo { get; set; }
    }
}