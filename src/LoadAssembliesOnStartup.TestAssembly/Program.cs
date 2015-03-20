// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.TestAssembly
{
    using System;
    using System.Diagnostics;
    using Catel.IoC;
    using TestAssemblyToReference.Services;
    using TestAssemblyToReferenceWithIgnores.Services;

    public class Program
    {
        #region Methods
        private static void Main(string[] args)
        {
        }

        public static void ExampleCallForIlInspection()
        {
            Debug.WriteLine("Loading assembly TestAssemblyToReference");
            var typeToLoad1 = typeof(ClassThatShouldBeRegistered);
        }

        public static void ExampleCallForIlInspectionWithTryCatch()
        {
            try
            {
                Debug.WriteLine("Loading assembly TestAssemblyToReference");
                var typeToLoad1 = typeof(ClassThatShouldBeRegistered);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public static bool IsRightAssemblyLoaded { get { return ServiceLocator.Default.IsTypeRegistered<IClassThatShouldBeRegistered>(); } }
        public static bool IsRightAssemblyUnloaded { get { return ServiceLocator.Default.IsTypeRegistered<IClassThatShouldNotBeRegistered>(); } }
    }
}