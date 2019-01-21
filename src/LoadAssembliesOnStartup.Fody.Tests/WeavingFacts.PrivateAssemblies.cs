namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using Catel.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public partial class NoWeavingFacts
    {
        [Test]
        public void ExcludesPrivateAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly(@"<LoadAssembliesOnStartup ExcludePrivateAssemblies='true' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath);
        }

        [Test]
        public void IncludesPrivateAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly(@"<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AfterAssemblyPath);
        }
    }
}
