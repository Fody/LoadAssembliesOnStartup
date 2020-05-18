namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using Catel.Reflection;
    using NUnit.Framework;

    [TestFixture, NonParallelizable]
    public partial class WeavingFacts
    {
        [TestCase]
        public void HasRegisteredTypesInIncludedReferences()
        {
            // Load program to load assembly
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludedReferences", @"<LoadAssembliesOnStartup />");

            var programType = assemblyInfo.Assembly.GetType("LoadAssembliesOnStartup.Fody.TestAssembly.Program");
            var programInstance = Activator.CreateInstance(programType);

            var propertyInfo = programType.GetPropertyEx("IsRightAssemblyLoaded", true, true);
            Assert.IsTrue((bool)propertyInfo.GetValue(null, null));
        }

        [Test]
        public void HasRegisteredOrcFileSystemViaWildCards()
        {
            // Load program to load assembly
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludeOrcLibraries", "<LoadAssembliesOnStartup IncludeAssemblies=\"Orc.*\" />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath);
        }

        [Test]
        public void HasNotRegisteredOrcFileSystemViaWildCards()
        {
            // Load program to load assembly
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("ExcludeOrcLibraries", "<LoadAssembliesOnStartup ExcludeAssemblies=\"Orc.*\" />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath);
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
