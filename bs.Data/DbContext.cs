using bs.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data
{
    public class DbContext : IDbContext
    {
        public string ConnectionString {get;set;}
        public string DatabaseEngineType { get; set; }
        public string[] FoldersWhereLookingForEntitiesDll {get;set;}
        public string[] EntitiesFileNameScannerPatterns {get;set;}
        public bool LookForEntitiesDllInCurrentDirectoryToo {get;set;}
        public bool Create {get;set;}
        public bool Update {get;set;}
    }
}
