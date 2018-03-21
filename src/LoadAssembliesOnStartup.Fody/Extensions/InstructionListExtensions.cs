// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstructionListExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    public static class InstructionListExtensions
    {
        public static bool IsNextInstructionOpCode(this IList<Instruction> instructions, int index, params OpCode[] opCodes)
        {
            var instruction = (instructions.Count > index + 1) ? instructions[index + 1] : null;
            if (instruction == null)
            {
                return false;
            }

            return instruction.IsOpCode(opCodes);
        }

        public static void MoveInstructionsToPosition(this IList<Instruction> instructions, int startIndex, int length, int targetPosition)
        {
            var instructionsToMove = new List<Instruction>();
            for (var i = startIndex; i < startIndex + length; i++)
            {
                instructionsToMove.Add(instructions[startIndex]);
                instructions.RemoveAt(startIndex);
            }

            if (startIndex < targetPosition)
            {
                targetPosition -= instructionsToMove.Count;
            }

            Insert(instructions, targetPosition, instructionsToMove);
        }

        public static void MoveInstructionsToEnd(this IList<Instruction> instructions, int startIndex, int length)
        {
            MoveInstructionsToPosition(instructions, startIndex, length, instructions.Count - 1);
        }

        public static Instruction GetPreviousInstruction(this IList<Instruction> instructions, Instruction instruction)
        {
            var currentIndex = instructions.IndexOf(instruction);
            if (currentIndex <= 0)
            {
                return null;
            }

            return instructions[currentIndex - 1];
        }

        public static bool UsesType(this IList<Instruction> instructions, TypeDefinition typeDefinition, params OpCode[] opCodes)
        {
            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];
                if (instruction.UsesType(typeDefinition, opCodes))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UsesField(this IList<Instruction> instructions, FieldDefinition field)
        {
            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];
                if (instruction.UsesField(field))
                {
                    return true;
                }
            }

            return false;
        }

        public static void Prepend(this Collection<Instruction> collection, params Instruction[] instructions)
        {
            for (var index = 0; index < instructions.Length; index++)
            {
                var instruction = instructions[index];
                collection.Insert(index, instruction);
            }
        }

        public static void Append(this Collection<Instruction> collection, params Instruction[] instructions)
        {
            for (var index = 0; index < instructions.Length; index++)
            {
                collection.Insert(index, instructions[index]);
            }
        }

        public static void BeforeLast(this Collection<Instruction> collection, params Instruction[] instructions)
        {
            var index = collection.Count - 1;
            foreach (var instruction in instructions)
            {
                collection.Insert(index, instruction);
                index++;
            }
        }

        public static int Insert(this IList<Instruction> collection, int index, List<Instruction> instructions)
        {
            return Insert(collection, index, instructions.ToArray());
        }

        public static int Insert(this IList<Instruction> collection, int index, params Instruction[] instructions)
        {
            foreach (var instruction in instructions.Reverse())
            {
                collection.Insert(index, instruction);
            }
            return index + instructions.Length;
        }
    }
}