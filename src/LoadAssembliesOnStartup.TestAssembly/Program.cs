// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.TestAssembly
{
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
            var typeToLoad1 = typeof(ClassThatShouldBeRegistered);
        }
        #endregion

        public static bool IsRightAssemblyLoaded { get { return ServiceLocator.Default.IsTypeRegistered<IClassThatShouldBeRegistered>(); } }
        public static bool IsRightAssemblyUnloaded { get { return ServiceLocator.Default.IsTypeRegistered<IClassThatShouldNotBeRegistered>(); } }
    }
}