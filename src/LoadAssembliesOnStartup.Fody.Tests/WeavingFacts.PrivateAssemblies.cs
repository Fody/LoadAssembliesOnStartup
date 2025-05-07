namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public partial class WeavingFacts
    {
        [Test]
        public async Task ExcludesPrivateAssembliesAsync()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("ExcludesPrivateAssemblies", @"<LoadAssembliesOnStartup ExcludePrivateAssemblies='true' />");

            await VerifyHelper.AssertIlCodeAsync(assemblyInfo.AssemblyPath);
        }

        [Test, Explicit("Unable to resolve private assets during unit tests in .NET 5")]
        public async Task IncludesPrivateAssembliesAsync()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludesPrivateAssemblies", @"<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />");

            await VerifyHelper.AssertIlCodeAsync(assemblyInfo.AssemblyPath);
        }
    }
}
