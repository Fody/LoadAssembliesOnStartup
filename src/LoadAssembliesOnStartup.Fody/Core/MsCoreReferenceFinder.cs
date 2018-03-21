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
            var msCoreLibDefinition = _assemblyResolver.Resolve(new AssemblyNameReference("mscorlib", null));
            var msCoreTypes = msCoreLibDefinition.MainModule.Types;

            var objectDefinition = msCoreTypes.FirstOrDefault(x => string.Equals(x.Name, "Object"));
            if (objectDefinition == null)
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

                _typeReferences[typeName] = (type != null) ? _moduleWeaver.ModuleDefinition.ImportReference(type) : null;
            }

            return _typeReferences[typeName];
        }

        private IEnumerable<TypeReference> GetTypes()
        {
            var msCoreLibDefinition = _assemblyResolver.Resolve("mscorlib");
            var msCoreTypes = msCoreLibDefinition.MainModule.Types.Cast<TypeReference>().ToList();

            var objectDefinition = msCoreTypes.FirstOrDefault(x => string.Equals(x.Name, "Object"));
            if (objectDefinition == null)
            {
                if (msCoreLibDefinition.IsNetStandardLibrary())
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

            return msCoreTypes;
        }

        private IEnumerable<TypeReference> GetDotNetTypes()
        {
            var systemDefinition = _assemblyResolver.Resolve("System");
            var systemTypes = systemDefinition.MainModule.Types;

            return systemTypes;
        }

        private IEnumerable<TypeReference> GetWinRtTypes()
        {
            var systemRuntime = _assemblyResolver.Resolve("System.Runtime");
            var systemRuntimeTypes = systemRuntime.MainModule.Types;

            return systemRuntimeTypes;
        }

        private IEnumerable<TypeReference> GetNetStandardTypes()
        {
            // Load all assemblies, it's slower but then we are sure we have all types
            var allTypes = new List<TypeReference>();

            foreach (var assembly in _moduleWeaver.ModuleDefinition.AssemblyReferences)
            {
                var resolvedAssembly = _assemblyResolver.Resolve(assembly);
                if ((resolvedAssembly != null) && resolvedAssembly.IsNetStandardLibrary())
                {
                    allTypes.AddRange(resolvedAssembly.MainModule.Types);
                }
            }

            return allTypes;
        }
    }
}