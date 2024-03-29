﻿using bs.Data.Interfaces.BaseEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace bs.Data.Helpers
{
    internal static class ReflectionHelper
    {
        /// <summary>Gets the assemblies that contains one or more implementations of the interface 'IPersisterEntity' from the specified DLLs dinamically loaded. If the same assembly is present in more than one DLL file it will be loaded once (the first time only).</summary>
        /// <param name="foldersWhereLookingForDll">The folders where recursively (current folder and sub folders) looking for DLL files.</param>
        /// <param name="fileNameScannerPattern">
        ///   <para>
        ///  The file name scanner patterns. The jolly char '*' is allowed. </para>
        ///   <para>
        ///     <em>For example: 'bs.model.*.dll' matchs with alla files taht start with 'bs.model.' and ends with '.dll'.</em>
        ///     <br />
        ///   </para>
        /// </param>
        /// <param name="useCurrentdirectoryToo">if set to <c>true</c> [use currentdirectory too].</param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesFromFiles(string[] foldersWhereLookingForDll, string[] fileNameScannerPattern, bool useCurrentdirectoryToo, bool useExecutingAssemblyToo = false)
        {
            var currentDirectory = "";
            var candidateFiles = new List<string>();
            var resultantAssemblies = new Dictionary<string, Assembly>();

            if (useCurrentdirectoryToo)
            {
                currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                candidateFiles.AddRange(Directory.EnumerateFiles(currentDirectory, "*.dll", SearchOption.AllDirectories));
            }

            if (foldersWhereLookingForDll != null)
            {
                foreach (var folder in foldersWhereLookingForDll)
                {
                    candidateFiles.AddRange(Directory.EnumerateFiles(folder, "*.dll", SearchOption.AllDirectories));
                }
            }

            IEnumerable<string> dllsToLoad;

            if (fileNameScannerPattern != null)
            {
                dllsToLoad = candidateFiles
                      .Where(filename => fileNameScannerPattern.Any(pattern => Regex.IsMatch(filename, pattern)));
            }
            else dllsToLoad = candidateFiles;

            var allAssemblies = dllsToLoad
                      .Select(Assembly.LoadFrom);

            if (useExecutingAssemblyToo)
            {
                var lst = new List<Assembly>();
                lst.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.Contains("Microsoft", StringComparison.InvariantCultureIgnoreCase) && !a.FullName.Contains("PresentationCore", StringComparison.InvariantCultureIgnoreCase)));
                lst.AddRange(allAssemblies.Where(a => !a.FullName.Contains("Microsoft", StringComparison.InvariantCultureIgnoreCase) && !a.FullName.Contains("PresentationCore", StringComparison.InvariantCultureIgnoreCase)));
                allAssemblies = lst;
            }

            var iPersistentEntityType = typeof(IPersistentEntity);

            var entitiesAssemblies = (from a in allAssemblies
                                      from t in a.GetTypes()
                                      where iPersistentEntityType.IsAssignableFrom(t) && t.IsClass
                                      select a).Distinct();

            foreach (var assembly in entitiesAssemblies)
            {
                if (!resultantAssemblies.ContainsKey(assembly.FullName))
                {
                    resultantAssemblies.Add(assembly.FullName, assembly);
                }
                else
                {
                    Debug.WriteLine($"Assembly: '{assembly.FullName}' will be added again to ORM mapping assemblies list.");
                    resultantAssemblies[assembly.FullName] = assembly;
                }
            }

            return resultantAssemblies.Select(x => x.Value);
        }
    }
}