namespace LoadAssembliesOnStartup.Fody.TestAssembly
{
    using TestAssemblyToReference.Services;

    public class DummyDependencyInjectionClass
    {
        #region Constructors
        public DummyDependencyInjectionClass(IClassThatShouldBeRegistered classThatShouldBeRegistered)
        {
        }
        #endregion
    }
}
