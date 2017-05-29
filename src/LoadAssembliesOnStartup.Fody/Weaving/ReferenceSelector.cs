// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSelector.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody.Weaving
{
    using System;
    using System.Collections.Generic;
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
            "Costura",
            "MethodTimer",
            "Obsolete",
            "PropertyChanged",
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
                if (!ShouldReferenceBeIncluded(assemblyReference))
                {
                    continue;
                }

                var assembly = resolver.Resolve(assemblyReference);
                if (assembly != null)
                {
                    FodyEnvironment.LogInfo($"Including reference '{assemblyReference.Name}'");

                    includedReferences.Add(assembly);
                }
            }

            if (!_configuration.ExcludeOptimizedAssemblies)
            {
                var splittedReferences = _moduleWeaver.References.Split(new [] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var splittedReference in splittedReferences)
                {
                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(splittedReference);

                    var isIncluded = (from reference in includedReferences
                        where string.Equals(reference.FullName, assemblyDefinition.FullName)
                        select reference).Any();

                    if (!isIncluded)
                    {
                        var referenceName = assemblyDefinition.Name;

                        if (!ShouldReferenceBeIncluded(assemblyDefinition.Name))
                        {
                            continue;
                        }

                        var assembly = resolver.Resolve(referenceName);
                        if (assembly != null)
                        {
                            FodyEnvironment.LogInfo($"Including reference '{referenceName.Name}', it was optimized away by the compiler but still adding it");

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
                    FodyEnvironment.LogInfo($"Ignoring '{assemblyName}' because it is a known assembly to be ignored");
                    return false;
                }
            }

            if (_configuration.IncludeAssemblies.Any())
            {
                var contains = _configuration.IncludeAssemblies.Any(x => string.Equals(assemblyNameLowered, x.ToLower()));
                if (!contains)
                {
                    FodyEnvironment.LogInfo($"Ignoring '{assemblyName}' because it is not in the included list");
                }

                return contains;
            }

            if (_configuration.ExcludeAssemblies.Any())
            {
                var contains = _configuration.ExcludeAssemblies.Any(x => string.Equals(assemblyNameLowered, x.ToLower()));
                if (contains)
                {
                    FodyEnvironment.LogInfo($"Ignoring '{assemblyName}' because it is in the excluded list");
                }

                return !contains;
            }

            return _configuration.OptOut;
        }
        #endregion
    }
}