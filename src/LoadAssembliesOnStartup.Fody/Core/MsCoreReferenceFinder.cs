// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsCoreReferenceFinder.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class MsCoreReferenceFinder
    {
        public const string CompilerGeneratedAttributeTypeName = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";
        public const string GeneratedCodeAttributeTypeName = "System.CodeDom.Compiler.GeneratedCodeAttribute";
        public const string DebuggerNonUserCodeAttributeTypeName = "System.Diagnostics.DebuggerNonUserCodeAttribute";

        private readonly ModuleWeaver _moduleWeaver;
        private readonly IAssemblyResolver _assemblyResolver;

        private readonly IDictionary<string, TypeReference> _typeReferences = new Dictionary<string, TypeReference>();

        public MethodReference GetTypeFromHandle;
        public TypeReference GeneratedCodeAttribute;
        public TypeReference CompilerGeneratedAttribute;
        public TypeReference DebuggerNonUserCodeAttribute;

        public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver)
        {
            _moduleWeaver = moduleWeaver;
            _assemblyResolver = assemblyResolver;
        }

        public void Execute()
        {
            var objectDefinition = _moduleWeaver.FindTypeDefinition("System.Object");
            if (objectDefinition is null)
            {
                return;
            }

            var type = GetCoreTypeReference("System.Type").Resolve();
            GetTypeFromHandle = _moduleWeaver.ModuleDefinition.ImportReference(type.Methods.First(m => m.Name == "GetTypeFromHandle"));

            GeneratedCodeAttribute = GetCoreTypeReference(GeneratedCodeAttributeTypeName);
            CompilerGeneratedAttribute = GetCoreTypeReference(CompilerGeneratedAttributeTypeName);
            DebuggerNonUserCodeAttribute = GetCoreTypeReference(DebuggerNonUserCodeAttributeTypeName);
        }

        public TypeReference GetCoreTypeReference(string typeName)
        {
            if (!_typeReferences.ContainsKey(typeName))
            {
                var types = GetTypes();
                var type = types.FirstOrDefault(x => string.Equals(x.Name, typeName) || string.Equals(x.FullName, typeName));

                _typeReferences[typeName] = (type is not null) ? _moduleWeaver.ModuleDefinition.ImportReference(type) : null;
            }

            return _typeReferences[typeName];
        }

        private IEnumerable<TypeReference> GetTypes()
        {
            var msCoreLibDefinition = _assemblyResolver.Resolve("mscorlib");
            var msCoreTypes = msCoreLibDefinition.MainModule.Types.Cast<TypeReference>().ToList();

            var objectDefinition = msCoreTypes.FirstOrDefault(x => string.Equals(x.Name, "Object"));
            if (objectDefinition is null)
            {
                if (msCoreLibDefinition.IsNetCoreLibrary())
                {
                    msCoreTypes.AddRange(GetDotNetCoreTypes());
                }
                else if (msCoreLibDefinition.IsNetStandardLibrary())
                {
                    msCoreTypes.AddRange(GetNetStandardTypes());
                }
                else
                {
                    msCoreTypes.AddRange(GetWinRtTypes());
                }
            }
            else
            {
                msCoreTypes.AddRange(GetDotNetTypes());
            }

            return msCoreTypes.OrderBy(x => x.FullName);
        }

        private IEnumerable<TypeReference> GetDotNetTypes()
        {
            var allTypes = new List<TypeReference>();

            allTypes.AddRange(GetTypesFromAssembly("System"));

            return allTypes;
        }

        private IEnumerable<TypeReference> GetDotNetCoreTypes()
        {
            var allTypes = new List<TypeReference>();

            allTypes.AddRange(GetTypesFromAssembly("System"));
            allTypes.AddRange(GetTypesFromAssembly("System.Core"));
            allTypes.AddRange(GetTypesFromAssembly("System.ComponentModel"));
            allTypes.AddRange(GetTypesFromAssembly("System.Diagnostics.Debug"));
            allTypes.AddRange(GetTypesFromAssembly("System.Diagnostics.Tools"));
            allTypes.AddRange(GetTypesFromAssembly("System.ObjectModel"));
            allTypes.AddRange(GetTypesFromAssembly("System.Runtime"));

            // Fallback mechanism
            allTypes.AddRange(GetTypesFromAssembly("System.Private.CoreLib"));

            return allTypes;
        }

        private IEnumerable<TypeReference> GetWinRtTypes()
        {
            var allTypes = new List<TypeReference>();

            allTypes.AddRange(GetTypesFromAssembly("System.Runtime"));

            return allTypes;
        }

        private IEnumerable<TypeReference> GetNetStandardTypes()
        {
            // Load all assemblies, it's slower but then we are sure we have all types
            var allTypes = new List<TypeReference>();

            foreach (var assembly in _moduleWeaver.ModuleDefinition.AssemblyReferences)
            {
                var resolvedAssembly = _assemblyResolver.Resolve(assembly);
                if ((resolvedAssembly is not null) && resolvedAssembly.IsNetStandardLibrary())
                {
                    allTypes.AddRange(resolvedAssembly.MainModule.Types);
                }
            }

            return allTypes;
        }

        private IEnumerable<TypeReference> GetTypesFromAssembly(string assemblyName)
        {
            var assembly = _assemblyResolver.Resolve(assemblyName);
            if (assembly is null)
            {
                return Array.Empty<TypeReference>();
            }

            var systemTypes = assembly.MainModule.Types;
            return systemTypes;
        }
    }
}
