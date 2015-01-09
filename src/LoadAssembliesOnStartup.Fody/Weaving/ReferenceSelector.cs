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
            "Microsoft.CSharp",
        });
        #endregion

        #region Fields
        private readonly ModuleWeaver _moduleWeaver;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly Configuration _configuration;
        #endregion

        #region Constructors
        public ReferenceSelector(ModuleWeaver moduleWeaver, ModuleDefinition moduleDefinition, Configuration configuration)
        {
            _moduleWeaver = moduleWeaver;
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
                    FodyEnvironment.LogInfo(string.Format("Including reference '{0}'", referenceName));

                    includedReferences.Add(assembly);
                }
            }

            if (!_configuration.ExcludeOptimizedAssemblies)
            {
                var splittedReferences = _moduleWeaver.References.Split(';');
                foreach (var splittedReference in splittedReferences)
                {
                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(splittedReference);

                    var isIncluded = (from reference in includedReferences
                        where string.Equals(reference.FullName, assemblyDefinition.FullName)
                        select reference).Any();

                    if (!isIncluded)
                    {
                        var referenceName = assemblyDefinition.Name.Name;

                        if (!ShouldReferenceBeIncluded(assemblyDefinition.Name))
                        {
                            continue;
                        }

                        var assembly = resolver.Resolve(referenceName);
                        if (assembly != null)
                        {
                            FodyEnvironment.LogInfo(string.Format("Including reference '{0}', it was optimized away by the compiler but still adding it",
                                referenceName));

                            includedReferences.Add(assembly);
                        }
                    }
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