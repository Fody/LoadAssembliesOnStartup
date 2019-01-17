// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoWeavingFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using Catel.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class NoWeavingFacts
    {
        [TestCase]
        public void HasRegisteredTypesInIncludedReferences()
        {
            // Load program to load assembly
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly(@"<LoadAssembliesOnStartup />");

            var programType = assemblyInfo.Assembly.GetType("LoadAssembliesOnStartup.Fody.TestAssembly.Program");
            var programInstance = Activator.CreateInstance(programType);

            var propertyInfo = programType.GetPropertyEx("IsRightAssemblyLoaded", true, true);
            Assert.IsTrue((bool)propertyInfo.GetValue(null, null));
        }

        [Test]
        public void ExcludesSystemAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly(@"<LoadAssembliesOnStartup ExcludeSystemAssemblies='true' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath, nameof(ExcludesSystemAssemblies));
        }

        [Test]
        public void IncludesSystemAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly(@"<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath, nameof(ExcludesSystemAssemblies));
        }

        [Test]
        public void ExcludesPrivateAssemblies()
        {
            // Not sure yet how to test...
        }

        //[TestCase]
        //public void HasNotRegisteredTypesInExcludedReferences()
        //{
        //    AssemblyWeaver.Initialize();

        //    var serviceLocator = ServiceLocator.Default;

        //    Assert.IsFalse(serviceLocator.IsTypeRegistered<IClassThatShouldNotBeRegistered>());
        //}
    }
}
