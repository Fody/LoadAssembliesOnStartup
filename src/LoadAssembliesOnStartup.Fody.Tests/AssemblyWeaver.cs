// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;
    using Catel.Reflection;
    using Fody;
    using Mono.Cecil;
    using TestAssembly;

    public class AssemblyWeaver
    {
        #region Constants
        public static Assembly Assembly;

        public static string BeforeAssemblyPath;
        public static string AfterAssemblyPath;

        public static List<string> Errors = new List<string>();

        private static AssemblyWeaver _instance;
        #endregion

        #region Constructors
        public AssemblyWeaver(List<string> referenceAssemblyPaths = null)
        {
            if (referenceAssemblyPaths == null)
            {
                referenceAssemblyPaths = new List<string>();
            }

            var directory = GetTargetAssemblyDirectory();

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += (sender, e) =>
            {
                var finalFile = Path.Combine(directory, $"{e.Name}.dll");

                Console.WriteLine("Loading assembly '{0}' from '{1}'", e.Name, finalFile);

                return Assembly.LoadFrom(finalFile);
            };

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

            var assemblyResolver = new MockAssemblyResolver();
            foreach (var referenceAssemblyPath in referenceAssemblyPaths)
            {
                //var directoryName = Path.GetDirectoryName(referenceAssemblyPath);
                //assemblyResolver.AddSearchDirectory(directoryName);
            }

            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = assemblyResolver,
                ReadSymbols = File.Exists(oldPdb),
            };

            using (var moduleDefinition = ModuleDefinition.ReadModule(BeforeAssemblyPath, readerParameters))
            {
                var weavingTask = new ModuleWeaver
                {
                    ModuleDefinition = moduleDefinition,
                    AssemblyResolver = assemblyResolver,
                    LogError = LogError,
                    Config = XElement.Parse(@"<LoadAssembliesOnStartup WrapInTryCatch='true' />")
                    //Config = XElement.Parse(@"<LoadAssembliesOnStartup WrapInTryCatch='true' IncludeAssemblies='' />")
                };

                weavingTask.Execute();
                moduleDefinition.Write(AfterAssemblyPath);
            }

            Assembly = Assembly.LoadFile(AfterAssemblyPath);
        }
        #endregion

        #region Methods
        private static string GetTargetAssemblyDirectory()
        {
            // Fix unit tests on build server
            var originalDirectory = Path.GetDirectoryName(typeof(AssemblyWeaver).GetAssemblyEx().Location);
            var buildServerDirectory = Path.GetFullPath($@"{originalDirectory}\..\..\output\Test");

            var finalDirectory =  Directory.Exists(buildServerDirectory) ? buildServerDirectory : originalDirectory;
            return finalDirectory;
        }

        public static void Initialize()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = new AssemblyWeaver();
        }

        private static void LogError(string error)
        {
            Errors.Add(error);
        }
        #endregion
    }
}