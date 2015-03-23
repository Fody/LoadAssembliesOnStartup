// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    public static class Extensions
    {
        public static IEnumerable<string> NonEmpty(this IEnumerable<string> list)
        {
            return list.Select(x => x.Trim()).Where(x => x != string.Empty);
        }

        public static int Insert(this Collection<Instruction> collection, int index, List<Instruction> instructions)
        {
            return Insert(collection, index, instructions.ToArray());
        }

        public static int Insert(this Collection<Instruction> collection, int index, params Instruction[] instructions)
        {
            foreach (var instruction in instructions.Reverse())
            {
                collection.Insert(index, instruction);
            }
            return index + instructions.Length;
        }
    }
}