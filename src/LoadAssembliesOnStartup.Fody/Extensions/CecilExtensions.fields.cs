// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.fields.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using Mono.Cecil;

    public static partial class CecilExtensions
    {
        public static FieldReference MakeGeneric(this FieldReference field, TypeReference declaringType)
        {
            var reference = new FieldReference(field.Name, field.FieldType)
            {
                DeclaringType = declaringType.MakeGenericIfRequired(),
            };

            return reference;
        }
    }
}