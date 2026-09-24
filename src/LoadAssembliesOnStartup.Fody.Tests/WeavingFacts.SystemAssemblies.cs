namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.Threading.Tasks;
    using TUnit.Assertions;
    using TUnit.Core;

    public partial class WeavingFacts
    {
        [Test]
        public async Task ExcludesSystemAssembliesAsync()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("ExcludesSystemAssemblies", @"<LoadAssembliesOnStartup ExcludeSystemAssemblies='true' />");

            await VerifyHelper.AssertIlCodeAsync(assemblyInfo.AssemblyPath);
        }

        [Test]
        public async Task IncludesSystemAssembliesAsync()
        {
            var assemblyInfo = AssemblyWeaver.Instance.GetAssembly("IncludesSystemAssemblies", @"<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />");

            await VerifyHelper.AssertIlCodeAsync(assemblyInfo.AssemblyPath);
        }
    }
}
