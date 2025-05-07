namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Reflection;
    using Microsoft.Win32.SafeHandles;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;
    using VerifyNUnit;
    using VerifyTests;

    public static class VerifyHelper
    {
        static VerifyHelper()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static async Task AssertIlCodeAsync(string assemblyFileName, [CallerMemberName]string callerMemberName = "")
        {
            var slug = callerMemberName.GetSlug();

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFileName);
            var loadAssembliesOnStartupTypeDefinition = assemblyDefinition.MainModule.GetType("LoadAssembliesOnStartup");
            var loadAssembliesOnStartupMethodDefinition = loadAssembliesOnStartupTypeDefinition.Methods.First(_ => _.Name == "LoadTypesOnStartup");

            var methodBody = loadAssembliesOnStartupMethodDefinition.Body;
            methodBody.SimplifyMacros();

            var actualIlBuilder = new StringBuilder();

            foreach (var instruction in methodBody.Instructions)
            {
                var line = instruction.ToString();

                if (instruction.Operand is TypeReference operationTypeReference)
                {
                    line += $" | {operationTypeReference.Scope}";
                }

                actualIlBuilder.AppendLine(line);
            }

            var actualIl = actualIlBuilder.ToString();

            var settings = new VerifySettings
            {
                
            };

            settings.UniqueForAssemblyConfiguration();

            // Replace versions so it never breaks on updates
            foreach (var assembly in new[]
            {
                typeof(Catel.CoreModule).Assembly,
                typeof(VerifyHelper).Assembly, // test assembly version
                typeof(CriticalHandleMinusOneIsInvalid).Assembly, // System.Private.CoreLib
            })
            {
                var search = $"Version={assembly.Version()}";

                settings.ScrubLinesWithReplace(replaceLine: _ =>
                {
                    return _.Replace(search, "Version=Version");
                });
            }

            await Verifier.Verify(actualIl, settings);
        }
    }
}
