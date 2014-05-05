// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsCoreReferenceFinder.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class MsCoreReferenceFinder
    {
        #region Fields
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly ModuleWeaver _moduleWeaver;

        private readonly IDictionary<string, TypeReference> _typeReferences = new Dictionary<string, TypeReference>();
        #endregion

        #region Constructors
        public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver)
        {
            _moduleWeaver = moduleWeaver;
            _assemblyResolver = assemblyResolver;
        }
        #endregion

        #region Properties
        public MethodReference GetTypeFromHandle { get; private set; }
        #endregion

        #region Methods
        public void Execute()
        {
            var msCoreLibDefinition = _assemblyResolver.Resolve("mscorlib");
            var msCoreTypes = msCoreLibDefinition.MainModule.Types;

            var objectDefinition = msCoreTypes.FirstOrDefault(x => string.Equals(x.Name, "Object"));
            if (objectDefinition == null)
            {
                return;
            }

            var type = GetCoreTypeReference("System.Type").Resolve();
            GetTypeFromHandle = _moduleWeaver.ModuleDefinition.Import(type.Methods.First(m => m.Name == "GetTypeFromHandle"));
        }

        public TypeReference GetCoreTypeReference(string typeName)
        {
            if (!_typeReferences.ContainsKey(typeName))
            {
                var types = GetTypes();
                var type = types.FirstOrDefault(x => string.Equals(x.Name, typeName) || string.Equals(x.FullName, typeName));

                _typeReferences[typeName] = (type != null) ? _moduleWeaver.ModuleDefinition.Import(type) : null;
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
                msCoreTypes.AddRange(GetWinRtTypes());
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
        #endregion
    }
}