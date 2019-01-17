// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.TestAssembly
{
    using System;
    using Catel.IoC;
    using TestAssemblyToReference.Services;
    using TestAssemblyToReferenceWithIgnores.Services;

    public class Program
    {
        #region Methods

        public static void ExampleCallForIlInspection()
        {
            //Debug.WriteLine("Loading assembly TestAssemblyToReference");
            var typeToLoad1 = typeof(ClassThatShouldBeRegistered);

#if !DEBUG
            Console.WriteLine(typeToLoad1);
#endif
        }

        public static void ExampleCallForIlInspectionWithTryCatch()
        {
            try
            {
                //Debug.WriteLine("Loading assembly TestAssemblyToReference");
                var typeToLoad1 = typeof(ClassThatShouldBeRegistered);

#if !DEBUG
                Console.WriteLine(typeToLoad1);
#endif
            }
            catch (Exception)
            {
            }

            try
            {
                //Debug.WriteLine("Loading assembly TestAssemblyToReference");
                var typeToLoad2 = typeof(ClassThatShouldBeRegistered);

#if !DEBUG
                Console.WriteLine(typeToLoad2);
#endif
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