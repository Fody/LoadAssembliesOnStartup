// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LoadAssembliesOnStartup.Fody.Weaving
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
        public MethodDefinition Execute()
        {
            var loadMethod = new MethodDefinition("LoadTypesOnStartup", MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig, _moduleDefinition.Import(_msCoreReferenceFinder.GetCoreTypeReference("Void")));

            var type = _msCoreReferenceFinder.GetCoreTypeReference("Type").Resolve();
            var getTypeFromHandleMethod = type.Methods.First(x => string.Equals(x.Name, "GetTypeFromHandle"));
            var getTypeFromHandle = _moduleDefinition.Import(getTypeFromHandleMethod);

            var body = loadMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;

            int counter = 1;
            var referenceSelector = new ReferenceSelector(_moduleDefinition);
            foreach (var assembly in referenceSelector.GetIncludedReferences())
            {
                var firstType = assembly.MainModule.Types.FirstOrDefault(x => x.IsClass && x.IsPublic);
                if (firstType != null)
                {
                    // var type = typeof(FirstTypeInAssembly);
                    // ==
                    //L_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)

                    var importedFirstType = _moduleDefinition.Import(firstType);

                    var variable = new VariableDefinition(string.Format("typeToLoad{0}", counter++), importedFirstType);
                    body.Variables.Add(variable);

                    instructions.Add(Instruction.Create(OpCodes.Ldtoken, importedFirstType));
                    instructions.Add(Instruction.Create(OpCodes.Call, getTypeFromHandle));
                    instructions.Add(Instruction.Create(OpCodes.Stloc, variable));
                }
            }

            instructions.Add(Instruction.Create(OpCodes.Ret));

            body.OptimizeMacros();

            //.class public abstract auto ansi sealed beforefieldinit LoadAssembliesOnStartup extends [mscorlib]System.Object
            var typeDefinition = new TypeDefinition(string.Empty, "LoadAssembliesOnStartup", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                _moduleDefinition.Import(_msCoreReferenceFinder.GetCoreTypeReference("System.Object")));

            typeDefinition.Methods.Add(loadMethod);
            _moduleDefinition.Types.Add(typeDefinition);

            return loadMethod;
        }
        #endregion
    }
}