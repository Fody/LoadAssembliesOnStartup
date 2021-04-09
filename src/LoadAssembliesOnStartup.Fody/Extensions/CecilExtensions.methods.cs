// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.methods.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public static partial class CecilExtensions
    {
        public static int FindBaseConstructorIndex(this MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            if (declaringType is not null)
            {
                var baseType = declaringType.BaseType;
                if (baseType is not null)
                {
                    var instructions = method.Body.Instructions;

                    for (var i = 0; i < instructions.Count; i++)
                    {
                        var instruction = instructions[i];
                        if (instruction.IsOpCode(OpCodes.Call))
                        {
                            var methodReference = instruction.Operand as MethodReference;
                            if (methodReference is not null)
                            {
                                if (methodReference.Name.Equals(".ctor"))
                                {
                                    var ctorDeclaringType = methodReference.DeclaringType?.FullName;
                                    if (baseType.FullName.Equals(ctorDeclaringType))
                                    {
                                        return i;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return -1;
        }

        public static SequencePoint GetFirstSequencePoint(this MethodDefinition method)
        {
            return method.DebugInformation.SequencePoints.FirstOrDefault();
        }

        public static SequencePoint GetSequencePoint(this MethodDefinition method, Instruction instruction)
        {
            var debugInfo = method.DebugInformation;
            return debugInfo.GetSequencePoint(instruction);
        }

        public static MethodReference MakeGeneric(this MethodReference method, TypeReference declaringType)
        {
            var reference = new MethodReference(method.Name, method.ReturnType)
            {
                DeclaringType = declaringType.MakeGenericIfRequired(),
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention,
            };

            foreach (var parameter in method.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            return reference;
        }

        public static MethodReference GetMethodReference(this MethodDefinition methodDefinition, Stack<TypeDefinition> typeDefinitions)
        {
            var methodReference = FodyEnvironment.ModuleDefinition.ImportReference(methodDefinition).GetGeneric();

            if (methodDefinition.IsStatic)
            {
                return methodReference;
            }

            typeDefinitions.Pop();
            while (typeDefinitions.Count > 0)
            {
                var definition = typeDefinitions.Pop();

                // Only call lower class if possible
                var containsMethod = (from method in definition.Methods
                                      where method.Name == methodDefinition.Name
                                      select method).Any();
                if (containsMethod)
                {
                    methodReference = methodReference.MakeGeneric(definition.BaseType);
                }
            }

            return methodReference;
        }
    }
}