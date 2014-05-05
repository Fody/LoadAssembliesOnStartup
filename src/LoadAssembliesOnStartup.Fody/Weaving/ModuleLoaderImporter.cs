// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleLoaderImporter.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LoadAssembliesOnStartup.Fody.Weaving
{
    using System;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class ModuleLoaderImporter
    {
        #region Methods
        public void ImportModuleLoader(ModuleDefinition moduleDefinition, MethodReference methodToCall,
            MsCoreReferenceFinder msCoreReferenceFinder)
        {
            const MethodAttributes attributes = MethodAttributes.Private
                                                | MethodAttributes.HideBySig
                                                | MethodAttributes.Static
                                                | MethodAttributes.SpecialName
                                                | MethodAttributes.RTSpecialName;

            var moduleClass = moduleDefinition.Types.FirstOrDefault(x => x.Name == "<Module>");
            if (moduleClass == null)
            {
                throw new WeavingException("Found no module class!");
            }

            var cctor = moduleClass.Methods.FirstOrDefault(x => x.Name == ".cctor");
            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor", attributes, moduleDefinition.Import(msCoreReferenceFinder.GetCoreTypeReference("Void")));
                cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                moduleClass.Methods.Add(cctor);
            }

            var importedMethodToCall = moduleDefinition.Import(methodToCall);

            var insertLocation = Math.Max(cctor.Body.Instructions.Count - 2, 0);
            cctor.Body.Instructions.Insert(insertLocation, Instruction.Create(OpCodes.Call, importedMethodToCall));
        }
        #endregion
    }
}