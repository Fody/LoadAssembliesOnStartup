namespace LoadAssembliesOnStartup.Fody.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public partial class WeavingFacts
    {
        [Test]
        public void ExcludesSystemAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("ExcludesSystemAssemblies", @"<LoadAssembliesOnStartup ExcludeSystemAssemblies='true' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AssemblyPath);
        }

        [Test]
        public void IncludesSystemAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludesSystemAssemblies", @"<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AssemblyPath);
        }
    }
}
