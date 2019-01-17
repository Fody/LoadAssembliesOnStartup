// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Catel.Reflection;
using LoadAssembliesOnStartup.Fody;
using LoadAssembliesOnStartup.Fody.TestAssembly;
using LoadAssembliesOnStartup.Fody.Tests;
using Mono.Cecil;

public class AssemblyWeaver
{
    #region Constants
    public Assembly Assembly;
    public string BeforeAssemblyPath;
    public string AfterAssemblyPath;

    public List<string> Errors = new List<string>();
    #endregion

    #region Constructors
    static AssemblyWeaver()
    {
        Instance = new AssemblyWeaver();
    }

    public AssemblyWeaver(List<string> referenceAssemblyPaths = null)
    {
        if (referenceAssemblyPaths == null)
        {
            referenceAssemblyPaths = new List<string>();
        }

        //Force ref since MSTest is a POS
        var type = typeof(DummyDependencyInjectionClass);

        BeforeAssemblyPath = type.GetAssemblyEx().Location;
        //BeforeAssemblyPath =  Path.GetFullPath("Catel.Fody.TestAssembly.dll");
        AfterAssemblyPath = BeforeAssemblyPath.Replace(".dll", "2.dll");

        var oldPdb = Path.ChangeExtension(BeforeAssemblyPath, "pdb");
        var newPdb = Path.ChangeExtension(AfterAssemblyPath, "pdb");
        if (File.Exists(oldPdb))
        {
            File.Copy(oldPdb, newPdb, true);
        }

        Debug.WriteLine("Weaving assembly on-demand from '{0}' to '{1}'", BeforeAssemblyPath, AfterAssemblyPath);

        var assemblyResolver = new DefaultAssemblyResolver();
        assemblyResolver.AddSearchDirectory(AssemblyDirectoryHelper.GetCurrentDirectory());
        //foreach (var referenceAssemblyPath in referenceAssemblyPaths)
        //{
        //    var directoryName = Path.GetDirectoryName(referenceAssemblyPath);
        //    assemblyResolver.AddSearchDirectory(directoryName);
        //}

        var metadataResolver = new MetadataResolver(assemblyResolver);

        var readerParameters = new ReaderParameters
        {
            AssemblyResolver = assemblyResolver,
            MetadataResolver = metadataResolver,
            ReadSymbols = File.Exists(oldPdb),
        };

        using (var moduleDefinition = ModuleDefinition.ReadModule(BeforeAssemblyPath, readerParameters))
        {
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = assemblyResolver,
                LogError = LogError,
            };

            weavingTask.Execute();
            moduleDefinition.Write(AfterAssemblyPath);
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

        Assembly = Assembly.LoadFile(AfterAssemblyPath);
    }
    #endregion

    public static AssemblyWeaver Instance { get; private set; }

    #region Methods
    private void LogError(string error)
    {
        Errors.Add(error);
    }
    #endregion
}
