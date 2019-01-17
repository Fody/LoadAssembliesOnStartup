namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using Catel.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public partial class NoWeavingFacts
    {
        [Test]
        public void ExcludesSystemAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly(@"<LoadAssembliesOnStartup ExcludeSystemAssemblies='true' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath);
        }

        [Test]
        public void IncludesSystemAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly(@"<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath);
        }
    }
}
