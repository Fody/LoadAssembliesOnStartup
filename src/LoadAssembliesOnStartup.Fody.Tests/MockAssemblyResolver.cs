// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockAssemblyResolver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Diagnostics;
using Mono.Cecil;
using System.Collections.Generic;

public class MockAssemblyResolver : DefaultAssemblyResolver
{
    private readonly Dictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();

    public override AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        var fullName = name.FullName;

        lock (_cache)
        {
            AssemblyDefinition definition = null;
            if (!_cache.TryGetValue(fullName, out definition))
            {
                if (name.Name == "System")
                {
                    var codeBase = typeof(Debug).Assembly.CodeBase.Replace("file:///", "");
                    definition = AssemblyDefinition.ReadAssembly(codeBase);
                }
                else
                {
                    definition = base.Resolve(name);
                }

                _cache[fullName] = definition;
            }

            return definition;
        }
    }
}