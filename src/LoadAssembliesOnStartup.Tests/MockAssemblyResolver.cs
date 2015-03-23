// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockAssemblyResolver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Tests
{
    using System;
    using System.Diagnostics;
    using Mono.Cecil;

    public class MockAssemblyResolver : IAssemblyResolver
    {
        #region IAssemblyResolver Members
        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return AssemblyDefinition.ReadAssembly(name.Name + ".dll");
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            throw new NotImplementedException();
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            if (fullName == "System")
            {
                var codeBase = typeof (Debug).Assembly.CodeBase.Replace("file:///", string.Empty);
                return AssemblyDefinition.ReadAssembly(codeBase);
            }
            else
            {
                var codeBase = typeof (string).Assembly.CodeBase.Replace("file:///", string.Empty);
                return AssemblyDefinition.ReadAssembly(codeBase);
            }
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}