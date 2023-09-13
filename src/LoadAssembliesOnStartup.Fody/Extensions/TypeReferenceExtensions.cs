// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeReferenceExtensions.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System.Linq;
    using Mono.Cecil;

    public static class TypeReferenceExtensions
    {
        #region Methods
        public static bool IsAssignableFrom(this TypeReference target, TypeReference type)
        {
            target = type.Module.ImportReference(target).Resolve();

            var typeDefinition = type.Resolve();

            while (typeDefinition is not null)
            {
                if (typeDefinition.Equals(target))
                {
                    return true;
                }

                if (typeDefinition.Interfaces.Any(_ => _.InterfaceType.Equals(target)))
                {
                    return true;
                }

                typeDefinition = typeDefinition.BaseType?.Resolve();
            }

            return false;
        }
        #endregion
    }
}