// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstructionExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LoadAssembliesOnStartup.Fody
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public static class InstructionExtensions
    {
        public static bool IsOpCode(this Instruction instruction, params OpCode[] opCodes)
        {
            if (opCodes.Length == 0)
            {
                return true;
            }

            foreach (var opCode in opCodes)
            {
                if (instruction.OpCode == opCode)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UsesType(this Instruction instruction, TypeDefinition typeDefinition, params OpCode[] opCodes)
        {
            if (instruction.IsOpCode(opCodes))
            {
                var fieldDefinition = instruction.Operand as FieldDefinition;
                if (fieldDefinition != null)
                {
                    if (string.Equals(fieldDefinition.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                var fieldReference = instruction.Operand as FieldReference;
                if (fieldReference != null)
                {
                    if (string.Equals(fieldReference.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                var methodDefinition = instruction.Operand as MethodDefinition;
                if (methodDefinition != null)
                {
                    if (string.Equals(methodDefinition.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }

                var methodReference = instruction.Operand as MethodReference;
                if (methodReference != null)
                {
                    if (string.Equals(methodReference.DeclaringType.Name, typeDefinition.Name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool UsesField(this Instruction instruction, FieldDefinition fieldDefinition)
        {
            var usedFieldDefinition = instruction.Operand as FieldDefinition;
            if (usedFieldDefinition != null)
            {
                if (string.Equals(usedFieldDefinition.Name, fieldDefinition.Name))
                {
                    return true;
                }
            }

            var usedFieldReference = instruction.Operand as FieldReference;
            if (usedFieldReference != null)
            {
                if (string.Equals(usedFieldReference.Name, fieldDefinition.Name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}