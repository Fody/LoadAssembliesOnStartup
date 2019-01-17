// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyDependencyInjectionClass.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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
