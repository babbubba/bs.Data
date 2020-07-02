using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces
{
   public interface IDbContext
    {
        string ConnectionString { get; set; }
        //string DatabaseEngineType { get; set; }
        DbType DatabaseEngineType { get; set; }
        string[] FoldersWhereLookingForEntitiesDll { get; set; }
        string[] EntitiesFileNameScannerPatterns { get; set; }
        bool LookForEntitiesDllInCurrentDirectoryToo { get; set; }
        bool UseExecutingAssemblyToo { get; set; }
        bool Create { get; set; }
        bool Update { get; set; }

        /// <summary>
        /// Gets or sets the size of concurrent call to database.
        /// </summary>
        /// <value>
        /// The size of the set batch.
        /// </value>
        short SetBatchSize { get; set; }
    }
}
