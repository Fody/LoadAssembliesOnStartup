// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoWeavingFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Tests
{
    using System;
    using Catel.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NoWeavingFacts
    {
        [TestMethod]
        public void HasRegisteredTypesInIncludedReferences()
        {
            AssemblyWeaver.Initialize();

            // Load program to load assembly
            var programType = AssemblyWeaver.Assembly.GetType("LoadAssembliesOnStartup.TestAssembly.Program");
            var programInstance = Activator.CreateInstance(programType);

            var propertyInfo = programType.GetPropertyEx("IsRightAssemblyLoaded", true, true);
            Assert.IsTrue((bool)propertyInfo.GetValue(null, null));
        }

        //[TestMethod]
        //public void HasNotRegisteredTypesInExcludedReferences()
        //{
        //    AssemblyWeaver.Initialize();

        //    var serviceLocator = ServiceLocator.Default;

        //    Assert.IsFalse(serviceLocator.IsTypeRegistered<IClassThatShouldNotBeRegistered>());
        //}
    }
}