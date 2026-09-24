namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using System.Threading.Tasks;
    using Catel.Reflection;
    using TUnit.Assertions;
    using TUnit.Core;

    [NotInParallel]
    public partial class WeavingFacts
    {
        [Test]
        public async Task HasRegisteredTypesInIncludedReferences()
        {
            // Load program to load assembly
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludedReferences", @"<LoadAssembliesOnStartup />");

            var programType = assemblyInfo.Assembly.GetType("LoadAssembliesOnStartup.Fody.TestAssembly.Program");
            var programInstance = Activator.CreateInstance(programType);

            var propertyInfo = programType.GetPropertyEx("IsRightAssemblyLoaded", true, true);
            await Assert.That((bool)propertyInfo.GetValue(null, null)).IsTrue();
        }

        [Test, Explicit]  // Unable to resolve private assets during unit tests in .NET 5
        public async Task HasRegisteredOrcFileSystemViaWildCardsAsync()
        {
            // Load program to load assembly
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludeOrcLibraries", "<LoadAssembliesOnStartup IncludeAssemblies=\"Orc.*\" />");

            await VerifyHelper.AssertIlCodeAsync(assemblyInfo.AssemblyPath);
        }

        [Test]
        public async Task HasNotRegisteredOrcFileSystemViaWildCardsAsync()
        {
            // Load program to load assembly
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("ExcludeOrcLibraries", "<LoadAssembliesOnStartup ExcludeAssemblies=\"Orc.*\" />");

            await VerifyHelper.AssertIlCodeAsync(assemblyInfo.AssemblyPath);
        }

        //[Test]
        //public async Task HasNotRegisteredTypesInExcludedReferences()
        //{
        //    AssemblyWeaver.Initialize();

        //    var serviceLocator = ServiceLocator.Default;

        //    Assert.IsFalse(serviceLocator.IsTypeRegistered<IClassThatShouldNotBeRegistered>());
        //}
    }
}
