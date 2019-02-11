// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Catel.Reflection;
using LoadAssembliesOnStartup.Fody;
using LoadAssembliesOnStartup.Fody.TestAssembly;
using LoadAssembliesOnStartup.Fody.Tests;
using Mono.Cecil;

public class AssemblyWeaver
{
    public class AssemblyInfo
    {
        public Assembly Assembly;
        public string BeforeAssemblyPath;
        public string AfterAssemblyPath;
        public List<string> Errors = new List<string>();
    }

    #region Constants
    private readonly Dictionary<string, AssemblyInfo> _assemblies = new Dictionary<string, AssemblyInfo>();
    #endregion

    #region Constructors
    static AssemblyWeaver()
    {
        Instance = new AssemblyWeaver();
    }

    public AssemblyWeaver(List<string> referenceAssemblyPaths = null)
    {
        if (referenceAssemblyPaths is null)
        {
            referenceAssemblyPaths = new List<string>();
        }
    }

    public AssemblyInfo GetAssembly(string testCaseName, string configString)
    {
        if (!_assemblies.ContainsKey(configString))
        {
            var type = typeof(DummyDependencyInjectionClass);

            var assemblyInfo = new AssemblyInfo
            {
                BeforeAssemblyPath = type.GetAssemblyEx().Location
            };

            var rootDirectory = Directory.GetParent(assemblyInfo.BeforeAssemblyPath).FullName;
            var testCaseDirectory = Path.Combine(rootDirectory, testCaseName);
            Directory.CreateDirectory(testCaseDirectory);

            assemblyInfo.AfterAssemblyPath = Path.Combine(testCaseDirectory, Path.GetFileName(assemblyInfo.BeforeAssemblyPath));

            var oldPdb = Path.ChangeExtension(assemblyInfo.BeforeAssemblyPath, "pdb");
            var newPdb = Path.ChangeExtension(assemblyInfo.AfterAssemblyPath, "pdb");
            if (File.Exists(oldPdb))
            {
                File.Copy(oldPdb, newPdb, true);
            }

            Debug.WriteLine("Weaving assembly on-demand from '{0}' to '{1}'", assemblyInfo.BeforeAssemblyPath, assemblyInfo.AfterAssemblyPath);

            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(testCaseDirectory);
            assemblyResolver.AddSearchDirectory(rootDirectory);

            var metadataResolver = new MetadataResolver(assemblyResolver);

            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = assemblyResolver,
                MetadataResolver = metadataResolver,
                ReadSymbols = File.Exists(oldPdb),
            };

            using (var moduleDefinition = ModuleDefinition.ReadModule(assemblyInfo.BeforeAssemblyPath, readerParameters))
            {
                var projectName = Path.GetFileName(assemblyInfo.BeforeAssemblyPath).Replace(".dll", string.Empty);
                var relativeCsProjectFilePath = Path.Combine(Directory.GetParent(assemblyInfo.BeforeAssemblyPath).FullName, "..", "..", "..", "..", "src", projectName, $"{projectName}.csproj");
                var csProjectFilePath = Path.GetFullPath(relativeCsProjectFilePath);

                var weavingTask = new ModuleWeaver
                {
                    Config = XElement.Parse(configString),
                    ModuleDefinition = moduleDefinition,
                    AssemblyResolver = assemblyResolver,
                    ProjectFilePath = csProjectFilePath,
                    LogError = (x) =>
                    {
                        assemblyInfo.Errors.Add(x);
                    },
                };

                weavingTask.Execute();
                moduleDefinition.Write(assemblyInfo.AfterAssemblyPath);
            }

            //        if (Debugger.IsAttached)
            //        {
            //#if DEBUG
            //            var output = "debug";
            //#else
            //            var output = "release";
            //#endif

            //            var targetFile = $@"C:\Source\Catel.Fody\output\{output}\Catel.Fody.Tests\Catel.Fody.TestAssembly2.dll";
            //            var targetDirectory = Path.GetDirectoryName(targetFile);
            //            Directory.CreateDirectory(targetDirectory);
            //            File.Copy(AfterAssemblyPath, targetFile, true);
            //        }

            assemblyInfo.Assembly = Assembly.LoadFile(assemblyInfo.AfterAssemblyPath);

            _assemblies[configString] = assemblyInfo;
        }

        return _assemblies[configString];
    }
    #endregion

    public static AssemblyWeaver Instance { get; private set; }
}
