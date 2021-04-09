// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public static partial class CecilExtensions
    {
        private static readonly Dictionary<string, TypeDefinition> _cachedTypeDefinitions = CacheHelper.GetCache<Dictionary<string, TypeDefinition>>("CecilExtensions");

        public static bool IsBoxingRequired(this TypeReference typeReference, TypeReference expectedType)
        {
            if (expectedType.IsValueType && string.Equals(typeReference.FullName, expectedType.FullName))
            {
                // Boxing is never required if type is expected
                return false;
            }

            if (typeReference.IsValueType || typeReference.IsGenericParameter)
            {
                return true;
            }

            return false;
        }

        public static TypeReference Import(this TypeReference typeReference, bool checkForNullableValueTypes = false)
        {
            var module = FodyEnvironment.ModuleDefinition;

            if (checkForNullableValueTypes)
            {
                var nullableValueType = typeReference.GetNullableValueType();
                if (nullableValueType is not null)
                {
                    return module.ImportReference(nullableValueType);
                }
            }

            return module.ImportReference(typeReference);
        }

        public static MethodReference FindConstructor(this TypeDefinition typeReference, List<TypeDefinition> types)
        {
            foreach (var ctor in typeReference.GetConstructors())
            {
                if (ctor.Parameters.Count == types.Count)
                {
                    var isValid = true;

                    for (var i = 0; i < ctor.Parameters.Count; i++)
                    {
                        var parameter = ctor.Parameters[i];
                        if (!string.Equals(parameter.ParameterType.FullName, types[i].FullName))
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        return ctor;
                    }
                }
            }

            return null;
        }

        public static TypeReference MakeGenericIfRequired(this TypeReference typeReference)
        {
            if (typeReference.HasGenericParameters)
            {
                var genericDeclaringType = new GenericInstanceType(typeReference);
                foreach (var genericParameter in typeReference.GenericParameters)
                {
                    genericDeclaringType.GenericArguments.Add(genericParameter);
                }

                typeReference = genericDeclaringType;
            }

            return typeReference;
        }

        public static TypeReference GetNullableValueType(this TypeReference typeReference)
        {
            if (!typeReference.IsGenericInstance)
            {
                return null;
            }

            if (!typeReference.FullName.Contains("System.Nullable`1"))
            {
                return null;
            }

            var genericInstanceType = typeReference as GenericInstanceType;
            if (genericInstanceType is null)
            {
                return null;
            }

            if (genericInstanceType.GenericArguments.Count != 1)
            {
                return null;
            }

            var genericParameter = genericInstanceType.GenericArguments[0];
            if (!genericParameter.IsValueType)
            {
                return null;
            }

            return genericParameter.GetElementType();
        }

        public static bool IsNullableValueType(this TypeReference typeReference)
        {
            return GetNullableValueType(typeReference) is not null;
        }

        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType, self.DeclaringType.MakeGenericInstanceType(arguments))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParameter in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));
            }

            return reference;
        }

        public static AssemblyDefinition ResolveAssembly(this ModuleDefinition moduleDefinition, string assemblyName)
        {
            var assemblyWithoutExtension = moduleDefinition.Name.Substring(0, moduleDefinition.Name.LastIndexOf("."));
            if (string.Equals(assemblyWithoutExtension, assemblyName))
            {
                return moduleDefinition.Assembly;
            }

            var assemblyResolver = moduleDefinition.AssemblyResolver;
            var resolvedAssembly = assemblyResolver.Resolve(assemblyName);
            return resolvedAssembly;
        }

        public static TypeDefinition FindType(this ModuleDefinition moduleDefinition, string assemblyName, string typeName)
        {
            var cacheKey = $"{typeName}, {assemblyName}|{moduleDefinition.Name}";
            if (_cachedTypeDefinitions.ContainsKey(cacheKey))
            {
                return _cachedTypeDefinitions[cacheKey];
            }

            var resolvedAssembly = moduleDefinition.ResolveAssembly(assemblyName);
            if (resolvedAssembly is null)
            {
                return null;
            }

            foreach (var module in resolvedAssembly.Modules)
            {
                var allTypes = module.GetAllTypeDefinitions().OrderBy(x => x.FullName);

                var type = (from typeDefinition in allTypes
                            where typeDefinition.FullName == typeName
                            select typeDefinition).FirstOrDefault();
                if (type is null)
                {
                    type = (from typeDefinition in allTypes
                            where typeDefinition.Name == typeName
                            select typeDefinition).FirstOrDefault();
                }

                if (type is not null)
                {
                    _cachedTypeDefinitions[cacheKey] = type;
                    return type;
                }
            }

            return null;
        }

        public static PropertyReference GetProperty(this TypeReference typeReference, string propertyName)
        {
            var typeDefinition = typeReference as TypeDefinition;
            if (typeDefinition is null)
            {
                typeDefinition = typeReference.Resolve();
            }

            return GetProperty(typeDefinition, propertyName);
        }

        public static PropertyReference GetProperty(this TypeDefinition typeDefinition, string propertyName)
        {
            var type = typeDefinition;
            while (type is not null && !type.FullName.Contains("System.Object"))
            {
                var propertyDefinition = (from property in type.Properties
                                          where string.Equals(propertyName, property.Name)
                                          select property).FirstOrDefault();

                if (propertyDefinition is not null)
                {
                    return propertyDefinition;
                }

                type = type.BaseType.Resolve();
            }

            return null;
        }

        public static MethodReference GetMethodAndImport(this ModuleDefinition module, string methodName)
        {
            var method = GetMethod(module, methodName);
            if (method is null)
            {
                return method;
            }

            return module.ImportReference(method);
        }

        public static MethodReference GetMethod(this ModuleDefinition module, string methodName)
        {
            var resolver = module.AssemblyResolver;
            foreach (var assemblyReference in module.AssemblyReferences)
            {
                var assembly = resolver.Resolve(assemblyReference.Name);
                if (assembly is not null)
                {
                    foreach (var type in assembly.MainModule.GetAllTypeDefinitions())
                    {
                        var methodReference = (from method in type.Methods
                                               where method.Name == methodName
                                               select method).FirstOrDefault();
                        if (methodReference is not null)
                        {
                            return methodReference;
                        }
                    }
                }
            }

            return null;
        }

        public static string GetName(this PropertyDefinition propertyDefinition)
        {
            return $"{propertyDefinition.DeclaringType.FullName}.{propertyDefinition.Name}";
        }

        public static bool IsCall(this OpCode opCode)
        {
            return (opCode.Code == Code.Call) || (opCode.Code == Code.Callvirt);
        }

        public static string GetName(this MethodDefinition methodDefinition)
        {
            return $"{methodDefinition.DeclaringType.FullName}.{methodDefinition.Name}";
        }

        public static MethodDefinition Constructor(this TypeDefinition typeDefinition, bool isStatic)
        {
            return (from method in typeDefinition.Methods
                    where method.IsConstructor && method.IsStatic == isStatic
                    select method).FirstOrDefault();
        }

        public static FieldReference GetGeneric(this FieldDefinition definition)
        {
            if (definition.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(definition.DeclaringType);
                foreach (var parameter in definition.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }
                return new FieldReference(definition.Name, definition.FieldType, declaringType);
            }

            return definition;
        }

        public static MethodReference GetGeneric(this MethodReference reference)
        {
            if (reference.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(reference.DeclaringType);
                foreach (var parameter in reference.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }
                var methodReference = new MethodReference(reference.Name, reference.MethodReturnType.ReturnType, declaringType);
                foreach (var parameterDefinition in reference.Parameters)
                {
                    methodReference.Parameters.Add(parameterDefinition);
                }
                methodReference.HasThis = reference.HasThis;
                return methodReference;
            }

            return reference;
        }

        public static CustomAttribute GetAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
        {
            return attributes.FirstOrDefault(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
        }

        public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
        {
            return attributes.Any(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
        }

        public static List<TypeDefinition> GetAllTypeDefinitions(this ModuleDefinition moduleDefinition)
        {
            var definitions = new List<TypeDefinition>();
            //First is always module so we will skip that;
            GetTypes(moduleDefinition.Types.Skip(1), definitions);
            return definitions;
        }

        private static void GetTypes(IEnumerable<TypeDefinition> typeDefinitions, List<TypeDefinition> definitions)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                GetTypes(typeDefinition.NestedTypes, definitions);
                definitions.Add(typeDefinition);
            }
        }

        public static IEnumerable<TypeReference> GetBaseTypes(this TypeDefinition type, bool includeIfaces)
        {
            var result = new List<TypeReference>();

            var current = type;

            var mappedFromSuperType = new List<TypeReference>();

            var previousGenericArgsMap = GetGenericArgsMap(type, new Dictionary<string, TypeReference>(), mappedFromSuperType);

            do
            {
                var currentBase = current.BaseType;
                if (currentBase is null)
                {
                    break;
                }

                if (currentBase is GenericInstanceType)
                {
                    previousGenericArgsMap = GetGenericArgsMap(current.BaseType, previousGenericArgsMap, mappedFromSuperType);

                    if (mappedFromSuperType.Any())
                    {
                        currentBase = ((GenericInstanceType)currentBase).ElementType.MakeGenericInstanceType(previousGenericArgsMap.Select(x => x.Value).ToArray());
                        mappedFromSuperType.Clear();
                    }
                }
                else
                {
                    previousGenericArgsMap = new Dictionary<string, TypeReference>();
                }

                result.Add(currentBase);

                current = current.BaseType.Resolve();

                if (includeIfaces)
                {
                    result.AddRange(BuildIFaces(current, previousGenericArgsMap));
                }
            } while (current.BaseType is not null);

            return result;
        }

        private static IEnumerable<TypeReference> BuildIFaces(TypeDefinition type, IDictionary<string, TypeReference> genericArgsMap)
        {
            var mappedFromSuperType = new List<TypeReference>();

            foreach (var iface in type.Interfaces)
            {
                var result = iface.InterfaceType;

                var genericIface = iface.InterfaceType as GenericInstanceType;
                if (genericIface is not null)
                {
                    var map = GetGenericArgsMap(genericIface, genericArgsMap, mappedFromSuperType);

                    if (mappedFromSuperType.Any())
                    {
                        result = genericIface.ElementType.MakeGenericInstanceType(map.Select(x => x.Value).ToArray()).Import();
                    }
                }

                yield return result;
            }
        }

        private static IDictionary<string, TypeReference> GetGenericArgsMap(TypeReference type, IDictionary<string, TypeReference> superTypeMap,
                                                                            IList<TypeReference> mappedFromSuperType)
        {
            var result = new Dictionary<string, TypeReference>();

            if (type is GenericInstanceType == false)
            {
                return result;
            }

            var genericArgs = ((GenericInstanceType)type).GenericArguments;
            var genericPars = ((GenericInstanceType)type).ElementType.Resolve().GenericParameters;

            /*

         * Now genericArgs contain concrete arguments for the generic
         * parameters (genericPars).
         *
         * However, these concrete arguments don't necessarily have
         * to be concrete TypeReferences, these may be referencec to
         * generic parameters from super type.
         *
         * Example:
         *
         *      Consider following hierarchy:
         *          StringMap<T> : Dictionary<string, T>
         *
         *          StringIntMap : StringMap<int>
         *
         *      What would happen if we walk up the hierarchy from StringIntMap:
         *          -> StringIntMap
         *              - here dont have any generic agrs or params for StringIntMap.
         *              - but when we reesolve StringIntMap we get a
         *                  reference to the base class StringMap<int>,
         *          -> StringMap<int>
         *              - this reference will have one generic argument
         *                  System.Int32 and it's ElementType,
         *                which is StringMap<T>, has one generic argument 'T'.
         *              - therefore we need to remember mapping T to System.Int32
         *              - when we resolve this class we'll get StringMap<T> and it's base
         *              will be reference to Dictionary<string, T>
         *          -> Dictionary<string, T>
         *              - now *genericArgs* will be System.String and 'T'
         *              - genericPars will be TKey and TValue from Dictionary
         *                  declaration Dictionary<TKey, TValue>
         *              - we know that TKey is System.String and...
         *              - because we have remembered a mapping from T to
         *                  System.Int32 and now we see a mapping from TValue to T,
         *                  we know that TValue is System.Int32, which bring us to
         *                  conclusion that StringIntMap is instance of
         *          -> Dictionary<string, int>
         */

            for (int i = 0; i < genericArgs.Count; i++)
            {
                var arg = genericArgs[i];

                var param = genericPars[i];

                if (arg is GenericParameter)
                {
                    TypeReference mapping;

                    if (superTypeMap.TryGetValue(arg.Name, out mapping))
                    {
                        mappedFromSuperType.Add(mapping);

                        result.Add(param.Name, mapping);
                    }
                    //else
                    //{
                    //            throw new Exception(string.Format(
                    //"GetGenericArgsMap: A mapping from supertype was not found. " +
                    //"Program searched for generic argument of name {0} in supertype generic arguments map " +
                    //"as it should server as value form generic argument for generic parameter {1} in the type {2}",
                    //arg.Name,
                    //param.Name,
                    //type.FullName));
                    //}
                }
                else
                {
                    result.Add(param.Name, arg);
                }
            }

            return result;
        }
    }
}