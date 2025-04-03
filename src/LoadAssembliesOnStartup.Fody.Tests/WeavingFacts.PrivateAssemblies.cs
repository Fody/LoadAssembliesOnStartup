namespace LoadAssembliesOnStartup.Fody.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public partial class WeavingFacts
    {
        [Test]
        public void ExcludesPrivateAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("ExcludesPrivateAssemblies", @"<LoadAssembliesOnStartup ExcludePrivateAssemblies='true' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AssemblyPath);
        }

        [Test, Explicit("Unable to resolve private assets during unit tests in .NET 5")]
        public void IncludesPrivateAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludesPrivateAssemblies", @"<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AssemblyPath);
        }
    }
}
