// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LoadAssembliesOnStartup.Weaving
{
    using System;
    using System.IO;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class LoadTypesWeaver
    {
        #region Constants
        #endregion

        #region Fields
        private readonly ModuleDefinition _moduleDefinition;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        #endregion

        #region Constructors
        public LoadTypesWeaver(ModuleDefinition moduleDefinition, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _moduleDefinition = moduleDefinition;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }
        #endregion

        #region Methods
        public MethodReference Execute()
        {
            var loadMethod = new MethodDefinition("LoadTypedOnStartup", new MethodAttributes(), _msCoreReferenceFinder.GetCoreTypeReference("Void"));

            var type = (TypeDefinition)_msCoreReferenceFinder.GetCoreTypeReference("Type");
            var getTypeFromHandleMethod = type.Methods.First(x => string.Equals(x.Name, "GetTypeFromHandle"));
            var getTypeFromHandle = _moduleDefinition.Import(getTypeFromHandleMethod);

            var body = loadMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;
            var resolver = _moduleDefinition.AssemblyResolver;
            foreach (var assemblyReference in _moduleDefinition.AssemblyReferences)
            {
                var assembly = resolver.Resolve(assemblyReference.Name);
                if (assembly != null)
                {
                    //var assemblyReference = ModuleDefinition.AssemblyResolver.Resolve(reference);
                    //if (assemblyReference != null)
                    //{
                    //var firstType = assemblyReference.MainModule.Types.FirstOrDefault();
                    var firstType = assembly.MainModule.Types.FirstOrDefault(x => x.IsClass && x.IsPublic);
                    if (firstType != null)
                    {
                        // var type = typeof(FirstTypeInAssembly);
                        // ==
                        //L_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)

                        var importedFirstType = _moduleDefinition.Import(firstType);

                        var variable = new VariableDefinition(importedFirstType);
                        body.Variables.Add(variable);

                        var insertLocation = Math.Max(0, instructions.Count - 2);

                        instructions.Insert(insertLocation++, Instruction.Create(OpCodes.Ldtoken, importedFirstType));
                        instructions.Insert(insertLocation++, Instruction.Create(OpCodes.Call, getTypeFromHandle));
                        instructions.Insert(insertLocation++, Instruction.Create(OpCodes.Stloc, variable));
                    }
                    //}
                }
            }

            body.OptimizeMacros();

            return loadMethod;
        }
        #endregion
    }
}