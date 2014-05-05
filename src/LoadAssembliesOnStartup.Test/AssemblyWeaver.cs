// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Catel.Reflection;
    using Mono.Cecil;

    public static class AssemblyWeaver
    {
        #region Constants
        public static Assembly Assembly;

        public static string BeforeAssemblyPath;
        public static string AfterAssemblyPath;

        public static List<string> Errors = new List<string>();
        #endregion

        #region Constructors
        static AssemblyWeaver()
        {
            var directory = GetTargetAssemblyDirectory();
            
            BeforeAssemblyPath = Path.Combine(directory, "LoadAssembliesOnStartup.TestAssembly.dll");
            AfterAssemblyPath = BeforeAssemblyPath.Replace(".dll", "2.dll");

            Console.WriteLine("Weaving assembly on-demand from '{0}' to '{1}'", BeforeAssemblyPath, AfterAssemblyPath);

            File.Copy(BeforeAssemblyPath, AfterAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(AfterAssemblyPath);

            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = new MockAssemblyResolver(),
                LogError = LogError,
            };

            weavingTask.Execute();
            moduleDefinition.Write(AfterAssemblyPath);

            Assembly = Assembly.LoadFile(AfterAssemblyPath);
        }
        #endregion

        #region Methods
        private static string GetTargetAssemblyDirectory()
        {
            // Fix unit tests on build server
            var originalDirectory = Path.GetDirectoryName(typeof(AssemblyWeaver).GetAssemblyEx().Location);
            var buildServerDirectory = Path.GetFullPath(string.Format(@"{0}\..\..\output\Test", originalDirectory));

            var finalDirectory =  Directory.Exists(buildServerDirectory) ? buildServerDirectory : originalDirectory;
            return finalDirectory;
        }

        public static void Initialize()
        {

        }

        private static void LogError(string error)
        {
            Errors.Add(error);
        }
        #endregion
    }
}