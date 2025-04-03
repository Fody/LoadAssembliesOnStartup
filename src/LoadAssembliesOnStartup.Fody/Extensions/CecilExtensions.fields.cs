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