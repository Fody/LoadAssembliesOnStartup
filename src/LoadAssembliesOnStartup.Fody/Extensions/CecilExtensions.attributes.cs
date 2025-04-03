namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Collections.Generic;

    public static partial class CecilExtensions
    {
        public static CustomAttribute GetAttribute(this TypeDefinition typeDefinition, string attributeName)
        {
            return GetAttribute(typeDefinition.CustomAttributes, attributeName);
        }

        public static CustomAttribute GetAttribute(this PropertyDefinition propertyDefinition, string attributeName)
        {
            return GetAttribute(propertyDefinition.CustomAttributes, attributeName);
        }

        public static CustomAttribute GetAttribute(Collection<CustomAttribute> customAttributes, string attributeName)
        {
            return GetAttributes(customAttributes, attributeName).FirstOrDefault();
        }

        public static IEnumerable<CustomAttribute> GetAttributes(this TypeDefinition typeDefinition, string attributeName)
        {
            return GetAttributes(typeDefinition.CustomAttributes, attributeName);
        }

        public static IEnumerable<CustomAttribute> GetAttributes(this PropertyDefinition propertyDefinition, string attributeName)
        {
            return GetAttributes(propertyDefinition.CustomAttributes, attributeName);
        }

        public static IEnumerable<CustomAttribute> GetAttributes(Collection<CustomAttribute> customAttributes, string attributeName)
        {
            return (from attribute in customAttributes
                    where attribute.Constructor.DeclaringType.FullName.Contains(attributeName)
                    select attribute);
        }

        public static bool IsDecoratedWithAttribute(this TypeDefinition typeDefinition, string attributeName)
        {
            return IsDecoratedWithAttribute(typeDefinition.CustomAttributes, attributeName);
        }
        
        public static bool IsDecoratedWithAttribute(this ParameterDefinition parameterDefinition, string attributeName)
        {
            return IsDecoratedWithAttribute(parameterDefinition.CustomAttributes, attributeName);
        }

        public static bool IsDecoratedWithAttribute(this PropertyDefinition propertyDefinition, string attributeName)
        {
            return IsDecoratedWithAttribute(propertyDefinition.CustomAttributes, attributeName);
        }

        public static bool IsDecoratedWithAttribute(Collection<CustomAttribute> customAttributes, string attributeName)
        {
            return GetAttribute(customAttributes, attributeName) is not null;
        }

        public static void RemoveAttribute(this TypeDefinition typeDefinition, string attributeName)
        {
            RemoveAttribute(typeDefinition.CustomAttributes, attributeName);
        }

        public static void RemoveAttribute(this ParameterDefinition typeDefinition, string attributeName)
        {
            RemoveAttribute(typeDefinition.CustomAttributes, attributeName);
        }

        public static void RemoveAttribute(this PropertyDefinition propertyDefinition, string attributeName)
        {
            RemoveAttribute(propertyDefinition.CustomAttributes, attributeName);
        }

        public static void RemoveAttribute(Collection<CustomAttribute> customAttributes, string attributeName)
        {
            var attributes = (from attribute in customAttributes
                              where attribute.Constructor.DeclaringType.FullName.Contains(attributeName)
                              select attribute).ToList();

            foreach (var attribute in attributes)
            {
                customAttributes.Remove(attribute);
            }
        }
    }
}