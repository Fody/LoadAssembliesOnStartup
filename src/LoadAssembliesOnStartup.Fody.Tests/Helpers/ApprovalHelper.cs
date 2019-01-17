namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using ApprovalTests;
    using Catel;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    public static class ApprovalHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AssertIlCode(string assemblyFileName, [CallerFilePath]string expectedResourceName = "")
        {
            var slug = expectedResourceName.GetSlug();

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFileName);
            var loadAssembliesOnStartupTypeDefinition = assemblyDefinition.MainModule.GetType("LoadAssembliesOnStartup");
            var loadAssembliesOnStartupMethodDefinition = loadAssembliesOnStartupTypeDefinition.Methods.First(x => x.Name == "LoadTypesOnStartup");

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

            // Note: don't dispose, otherwise we can't use approvals
            var tempFileContext = new TemporaryFilesContext(slug);
            var actualFile = tempFileContext.GetFile("actual.txt", true);

            File.WriteAllText(actualFile, actualIl);

            Approvals.VerifyFile(actualFile);
        }
    }
}
