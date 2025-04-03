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
            if (!_cache.TryGetValue(fullName, out var definition))
            {
                if (name.Name == "System")
                {
                    var codeBase = typeof(Debug).Assembly.Location.Replace("file:///", string.Empty);
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
