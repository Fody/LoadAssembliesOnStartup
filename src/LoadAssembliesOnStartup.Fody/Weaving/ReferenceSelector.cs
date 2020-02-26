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
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Mono.Cecil;

    public class ReferenceSelector
    {
        #region Constants
        private static readonly List<string> KnownIgnoredPartialAssemblies = new List<string>(new[]
        {
            "Anotar.",
            "mscorlib",
            "netstandard",
            "netfx.force.conflicts",
            "PresentationFramework",
            "UIAutomationClient",
            "UIAutomationTypes",
            "UIAutomationProvider",
        });

        private static readonly List<string> KnownIgnoredExactAssemblies = new List<string>(new[]
        {
            "Anotar",
            "Catel.Fody",
            "Catel.Fody.Attributes",
            "Costura",
            "Costura.Fody",
            "FodyHelpers",
            "LoadAssembliesOnStartup",
            "LoadAssembliesOnStartup.Fody",
            "MethodTimer",
            "Obsolete",
            "Obsolete.Fody",
            "PropertyChanged",
            "Microsoft.CSharp",
            "WpfAnalyzers",
            "System",
            "Microsoft.mshtml" // broken reference, fails without any exceptions / error logging, so never include
        });

        private static readonly List<string> SystemAssemblyPrefixes = new List<string>(new[]
        {
            "Mono.",
            "System."
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
        /// <summary>
        /// Gets the included references.
        /// </summary>
        /// <returns></returns>
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
                var splittedReferences = _moduleWeaver.References.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
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

            return includedReferences.OrderBy(x => x.Name.Name);
        }

        private bool ShouldReferenceBeIncluded(AssemblyNameReference assemblyNameReference)
        {
            var assemblyName = assemblyNameReference.Name;
            var assemblyNameLowered = assemblyNameReference.Name.ToLower();

            foreach (var knownIgnoredAssembly in KnownIgnoredPartialAssemblies)
            {
                var name = knownIgnoredAssembly.ToLower();

                if (assemblyNameLowered.Contains(name))
                {
                    FodyEnvironment.LogInfo($"Ignoring '{assemblyName}' because it is a known assembly to be ignored (via partial match '{knownIgnoredAssembly}')");
                    return false;
                }
            }

            foreach (var knownIgnoredAssembly in KnownIgnoredExactAssemblies)
            {
                var name = knownIgnoredAssembly.ToLower();

                if (assemblyNameLowered.Equals(name))
                {
                    FodyEnvironment.LogInfo($"Ignoring '{name}' because it is a known assembly to be ignored (via exact match '{knownIgnoredAssembly}')");
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
                    return false;
                }

                // Don't return here, allow it to check for private assemblies, we don't want to include *everything*
                // just because 1 or 2 are being excluded
            }

            if (_configuration.ExcludeSystemAssemblies)
            {
                foreach (var systemAssemblyPrefix in SystemAssemblyPrefixes)
                {
                    // Special case: System.dll, we don't want to include "System" to the prefixes, that would be too strict
                    if (assemblyName.IndexOf(systemAssemblyPrefix, StringComparison.OrdinalIgnoreCase) == 0 ||
                        assemblyName.Equals("System", StringComparison.OrdinalIgnoreCase))
                    {
                        FodyEnvironment.LogInfo($"Ignoring '{assemblyName}' because it is a system assembly");
                        return false;
                    }
                }
            }

            if (_configuration.ExcludePrivateAssemblies)
            {
                if (IsPrivateReference(assemblyName))
                {
                    FodyEnvironment.LogInfo($"Ignoring '{assemblyName}' because it is a private assembly");
                    return false;
                }
                // TODO: How to determine private assemblies, do we have access to the csproj?
                //foreach (var systemAssemblyPrefix in SystemAssemblyPrefixes)
                //{
                //    if (assemblyNameLowered.IndexOf(systemAssemblyPrefix, StringComparison.OrdinalIgnoreCase) == 0)
                //    {
                //        FodyEnvironment.LogInfo($"Ignoring '{assemblyName}' because it is a system assembly");
                //        return false;
                //    }
                //}
            }

            return _configuration.OptOut;
        }

        private bool IsPrivateReference(string assemblyName)
        {
            var privateReferences = FindPrivateReferences();

            var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var nuGetRoot = Path.Combine(userProfilePath, ".nuget", "packages");

            // For now, ignore the target version, just check whether the package (version) contains the assembly
            foreach (var privateReference in privateReferences)
            {
                try
                {
                    // For some packages (such as Fody), there is no /lib folder, in that case we don't need
                    // to check anything
                    var path = Path.Combine(nuGetRoot, privateReference.PackageName, privateReference.Version, "lib");
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }

                    FodyEnvironment.LogDebug($"Checking private reference '{privateReference}' using '{path}'");

                    var isDll = Directory.GetFiles(path, $"{assemblyName}.dll", SearchOption.AllDirectories).Any();
                    if (isDll)
                    {
                        return true;
                    }

                    var isExe = Directory.GetFiles(path, $"{assemblyName}.exe", SearchOption.AllDirectories).Any();
                    if (isExe)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    FodyEnvironment.LogError($"Failed to check private reference '{privateReference}':\n{ex}");
                }
            }

            return false;
        }

        private List<PrivateReference> FindPrivateReferences()
        {
            var csProj = _moduleWeaver.ProjectFilePath;
            if (string.IsNullOrWhiteSpace(csProj) || !File.Exists(csProj))
            {
                return new List<PrivateReference>();
            }

            // Assembly name != package name, so we need to go through all *private* packages to 
            // see if it's a private reference. For now we have a *simple* reference structure,
            // which means only modern sdk project style is supported, and only references directly
            // listed in the csproj will be supported

            var privateReferencesCache = CacheHelper.GetCache<Dictionary<string, List<PrivateReference>>>(csProj);
            if (!privateReferencesCache.TryGetValue(csProj, out var privateReferences))
            {
                privateReferences = new List<PrivateReference>();

                try
                {
                    var element = XElement.Parse(File.ReadAllText(csProj));

                    var packageReferenceElements = element.XPathSelectElements("//PackageReference");

                    foreach (var packageReferenceElement in packageReferenceElements)
                    {
                        var includeAttribute = packageReferenceElement.Attribute("Include");
                        if (includeAttribute is null)
                        {
                            continue;
                        }

                        var packageName = includeAttribute.Value;

                        var versionAttribute = packageReferenceElement.Attribute("Version");
                        if (versionAttribute is null)
                        {
                            FodyEnvironment.LogWarning($"Could not find version attribute for '{packageName}'");
                            continue;
                        }

                        var version = versionAttribute.Value;

                        var privateAssetsAttribute = packageReferenceElement.Attribute("PrivateAssets");
                        if (privateAssetsAttribute != null)
                        {
                            if (string.Equals(privateAssetsAttribute.Value, "all", StringComparison.OrdinalIgnoreCase))
                            {
                                privateReferences.Add(new PrivateReference(packageName, version));
                                continue;
                            }
                        }

                        var privateAssetsElement = packageReferenceElement.Element("PrivateAssets");
                        if (privateAssetsElement != null)
                        {
                            if (string.Equals(privateAssetsElement.Value, "all", StringComparison.OrdinalIgnoreCase))
                            {
                                privateReferences.Add(new PrivateReference(packageName, version));
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FodyEnvironment.LogError($"Failed to search for private packages in project file '{csProj}':\n{ex}");
                }

                privateReferencesCache[csProj] = privateReferences;
            }

            return privateReferences;
        }
        #endregion
    }
}
