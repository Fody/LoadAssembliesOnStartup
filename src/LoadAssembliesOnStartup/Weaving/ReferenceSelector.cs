// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSelector.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody.Weaving
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;

    public class ReferenceSelector
    {
        #region Constants
        private static readonly List<string> KnownIgnoredAssemblies = new List<string>(new []
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
        #endregion

        #region Constructors
        public ReferenceSelector(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
        }
        #endregion

        #region Methods
        public IEnumerable<AssemblyDefinition> GetIncludedReferences()
        {
            var includedReferences = new List<AssemblyDefinition>();

            var resolver = _moduleDefinition.AssemblyResolver;
            foreach (var assemblyReference in _moduleDefinition.AssemblyReferences)
            {
                bool ignoreReference = false;
                var referenceName = assemblyReference.Name;
                foreach (var knownIgnoredAssembly in KnownIgnoredAssemblies)
                {
                    if (referenceName.ToLower().Contains(knownIgnoredAssembly.ToLower()))
                    {
                        ignoreReference = true;
                        break;
                    }
                }

                if (ignoreReference)
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
        #endregion
    }
}