namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using Catel.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public partial class NoWeavingFacts
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
        
        //[TestCase]
        //public void HasNotRegisteredTypesInExcludedReferences()
        //{
        //    AssemblyWeaver.Initialize();

        //    var serviceLocator = ServiceLocator.Default;

        //    Assert.IsFalse(serviceLocator.IsTypeRegistered<IClassThatShouldNotBeRegistered>());
        //}
    }
}
