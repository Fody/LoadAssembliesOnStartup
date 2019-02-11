namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using ApprovalTests;
    using ApprovalTests.Namers;
    using ApprovalTests.Writers;
    using Catel;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    public static class ApprovalHelper
    {
        private static readonly string _configurationName;

        static ApprovalHelper()
        {
#if DEBUG
            _configurationName = "debug";
#else
            _configurationName = "release";
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AssertIlCode(string assemblyFileName, [CallerMemberName]string callerMemberName = "")
        {
            var slug = callerMemberName.GetSlug();

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

            var actualFile = tempFileContext.GetFile($"actual_il_{_configurationName}.txt", true);

            File.WriteAllText(actualFile, actualIl);

            var writer = new ExistingFileWriter(actualFile);
            var namer = new ApprovalNamer();
            
            Approvals.Verify(writer, namer, Approvals.GetReporter());

            //Approvals.VerifyFile(actualFile);
        }

        private class ApprovalNamer : UnitTestFrameworkNamer
        {
            public override string Name => $"{base.Name}.{_configurationName}";
        }
    }
}
