﻿using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces
{
   public interface IDbContext
    {
        string ConnectionString { get; set; }
        string[] FoldersWhereLookingForEntitiesDll { get; set; }
        string[] EntitiesFileNameScannerPatterns { get; set; }
        bool LookForEntitiesDllInCurrentDirectoryToo { get; set; }
        bool Create { get; set; }
        bool Update { get; set; }
    }
}