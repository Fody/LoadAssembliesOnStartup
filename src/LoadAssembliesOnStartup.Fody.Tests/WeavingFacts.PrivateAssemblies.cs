namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using Catel.Reflection;
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

        [Test]
        public void IncludesPrivateAssemblies()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludesPrivateAssemblies", @"<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />");

            ApprovalHelper.AssertIlCode(assemblyInfo.AssemblyPath);
        }
    }
}
