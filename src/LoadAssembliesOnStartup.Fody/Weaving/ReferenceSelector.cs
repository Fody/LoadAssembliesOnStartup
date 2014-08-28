// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSelector.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody.Weaving
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Mono.Cecil;

    public class ReferenceSelector
    {
        #region Constants
        private static readonly List<string> KnownIgnoredAssemblies = new List<string>(new[]
        {
            "mscorlib",
            "Anotar",
            "Catel.Fody.Attributes",
            //"Obsolete",
            //"PropertyChanged"
        });
        #endregion

        #region Fields
        private readonly ModuleDefinition _moduleDefinition;
        private readonly Configuration _configuration;
        #endregion

        #region Constructors
        public ReferenceSelector(ModuleDefinition moduleDefinition, Configuration configuration)
        {
            _moduleDefinition = moduleDefinition;
            _configuration = configuration;
        }
        #endregion

        #region Methods
        public IEnumerable<AssemblyDefinition> GetIncludedReferences()
        {
            var includedReferences = new List<AssemblyDefinition>();

            var resolver = _moduleDefinition.AssemblyResolver;
            foreach (var assemblyReference in _moduleDefinition.AssemblyReferences)
            {
                var referenceName = assemblyReference.Name;

                if (!ShouldReferenceBeIncluded(assemblyReference))
                {
                    continue;
                }

                var assembly = resolver.Resolve(referenceName);
                if (assembly != null)
                {
                    includedReferences.Add(assembly);
                }
            }

            return includedReferences;
        }

        private bool ShouldReferenceBeIncluded(AssemblyNameReference assemblyNameReference)
        {
            var assemblyName = assemblyNameReference.Name;
            var assemblyNameLowered = assemblyNameReference.Name.ToLower();

            foreach (var knownIgnoredAssembly in KnownIgnoredAssemblies)
            {
                if (assemblyNameLowered.Contains(knownIgnoredAssembly.ToLower()))
                {
                    FodyEnvironment.LogInfo(string.Format("Ignoring '{0}' because it is a known assembly to be ignored", assemblyName));
                    return false;
                }
            }

            if (_configuration.IncludeAssemblies.Any())
            {
                bool contains = _configuration.IncludeAssemblies.Any(x => string.Equals(assemblyNameLowered, x.ToLower()));

                if (!contains)
                {
                    FodyEnvironment.LogInfo(string.Format("Ignoring '{0}' because it is not in the included list", assemblyName));
                }

                return contains;
            }

            if (_configuration.ExcludeAssemblies.Any())
            {
                var contains = _configuration.ExcludeAssemblies.Any(x => string.Equals(assemblyNameLowered, x.ToLower()));

                if (contains)
                {
                    FodyEnvironment.LogInfo(string.Format("Ignoring '{0}' because it is in the excluded list", assemblyName));
                }

                return !contains;
            }

            return true;
        }
        #endregion
    }
}