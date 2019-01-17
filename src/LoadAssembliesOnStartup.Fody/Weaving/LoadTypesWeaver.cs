// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LoadAssembliesOnStartup.Fody.Weaving
{
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using MethodAttributes = Mono.Cecil.MethodAttributes;
    using TypeAttributes = Mono.Cecil.TypeAttributes;

    public class LoadTypesWeaver
    {
        #region Constants
        #endregion

        #region Fields
        private readonly ModuleDefinition _moduleDefinition;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;
        private readonly Configuration _configuration;
        private readonly ModuleWeaver _moduleWeaver;
        #endregion

        #region Constructors
        public LoadTypesWeaver(ModuleDefinition moduleDefinition, MsCoreReferenceFinder msCoreReferenceFinder,
            Configuration configuration, ModuleWeaver moduleWeaver)
        {
            _moduleDefinition = moduleDefinition;
            _msCoreReferenceFinder = msCoreReferenceFinder;
            _configuration = configuration;
            _moduleWeaver = moduleWeaver;
        }
        #endregion

        #region Methods
        public MethodDefinition Execute()
        {
            var debugWriteLineMethod = FindDebugWriteLineMethod();
            if (debugWriteLineMethod == null)
            {
                FodyEnvironment.LogInfo("Can't find Debug.WriteLine, won't be writing debug info during assembly loading");
            }

            var voidType = _msCoreReferenceFinder.GetCoreTypeReference("Void");
            var loadMethod = new MethodDefinition("LoadTypesOnStartup", MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig, _moduleDefinition.ImportReference(voidType));

            var type = _msCoreReferenceFinder.GetCoreTypeReference("Type").Resolve();
            var typeImported = _moduleDefinition.ImportReference(type);
            var getTypeFromHandleMethod = type.Methods.First(x => string.Equals(x.Name, "GetTypeFromHandle"));
            var getTypeFromHandle = _moduleDefinition.ImportReference(getTypeFromHandleMethod);

            var body = loadMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;
            if (instructions.Count == 0)
            {
                instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            var referenceSelector = new ReferenceSelector(_moduleWeaver, _moduleDefinition, _configuration);

            // Note: we are looping reversed to easily add try/catch mechanism
            foreach (var assembly in referenceSelector.GetIncludedReferences().Reverse())
            {
                var firstType = assembly.MainModule.Types.FirstOrDefault(x => x.IsClass && x.IsPublic);
                if (firstType != null)
                {
                    FodyEnvironment.LogInfo($"Adding code to force load assembly '{assembly.Name}'");

                    if (debugWriteLineMethod != null)
                    {
                        // L_0001: ldstr "Loading assembly TestAssemblyToReference"
                        //L_0006: call void [System]System.Diagnostics.Debug::WriteLine(string)

                        // Temporarily disabled because we first need to investigate if this is ever useful
                        //instructions.Add(Instruction.Create(OpCodes.Ldstr, string.Format("Loading assembly {0}", assembly.Name)));
                        //instructions.Add(Instruction.Create(OpCodes.Call, debugWriteLineMethod));
                    }

                    // var type = typeof(FirstTypeInAssembly);
                    // ==
                    //L_000a: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)

                    var firstTypeImported = _moduleDefinition.ImportReference(firstType);

                    var variable = new VariableDefinition(typeImported);
                    body.Variables.Insert(0, variable);

                    var instructionsToAdd = new[]
                    {
                        Instruction.Create(OpCodes.Ldtoken, firstTypeImported),
                        Instruction.Create(OpCodes.Call, getTypeFromHandle),
                        Instruction.Create(OpCodes.Stloc, variable)
                    };

                    instructions.Insert(0, instructionsToAdd);

                    if (_configuration.WrapInTryCatch)
                    {
                        var firstInstructionAfterInjectedSet = instructions[instructionsToAdd.Length];

                        // Pop means empty catch
                        var emptyCatchInstructions = new[]
                        {
                            Instruction.Create(OpCodes.Leave_S, firstInstructionAfterInjectedSet),
                            Instruction.Create(OpCodes.Pop),
                            Instruction.Create(OpCodes.Leave_S, firstInstructionAfterInjectedSet)
                        };

                        instructions.Insert(instructionsToAdd.Length, emptyCatchInstructions);

                        var tryStartInstruction = instructionsToAdd.First();
                        var tryEndInstruction = emptyCatchInstructions.Skip(1).First();
                        var handlerStartInstruction = emptyCatchInstructions.Skip(1).First();
                        var handlerEndInstruction = firstInstructionAfterInjectedSet;

                        var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                        {
                            TryStart = tryStartInstruction,
                            TryEnd = tryEndInstruction,
                            HandlerStart = handlerStartInstruction,
                            HandlerEnd = handlerEndInstruction,
                            CatchType = _moduleDefinition.ImportReference(_msCoreReferenceFinder.GetCoreTypeReference("Exception"))
                        };

                        body.ExceptionHandlers.Insert(0, handler);
                    }
                }
            }

            instructions.Add(Instruction.Create(OpCodes.Ret));

            body.OptimizeMacros();

            //.class public abstract auto ansi sealed beforefieldinit LoadAssembliesOnStartup extends [mscorlib]System.Object
            var typeDefinition = new TypeDefinition(string.Empty, "LoadAssembliesOnStartup", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                _moduleDefinition.ImportReference(_msCoreReferenceFinder.GetCoreTypeReference("System.Object")));

            typeDefinition.Methods.Add(loadMethod);
            _moduleDefinition.Types.Add(typeDefinition);

            return loadMethod;
        }

        private MethodReference FindDebugWriteLineMethod()
        {
            var debugTypeReference = _msCoreReferenceFinder.GetCoreTypeReference("Debug");
            if (debugTypeReference == null)
            {
                return null;
            }

            var resolvedDebugTypeReference = debugTypeReference.Resolve();
            if (resolvedDebugTypeReference == null)
            {
                return null;
            }

            var debugWriteLineMethod = (from method in resolvedDebugTypeReference.Methods
                                        where string.Equals(method.Name, "WriteLine") &&
                                              method.Parameters.Count == 1 &&
                                              string.Equals(method.Parameters[0].ParameterType.Name, "String")
                                        select method).FirstOrDefault();
            if (debugWriteLineMethod == null)
            {
                return null;
            }

            return _moduleDefinition.ImportReference(debugWriteLineMethod);
        }
        #endregion
    }
}
