namespace LoadAssembliesOnStartup.Fody.Weaving
{
    using System;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class ModuleLoaderImporter
    {
        public void ImportModuleLoader(ModuleDefinition moduleDefinition, MethodReference methodToCall,
            MsCoreReferenceFinder msCoreReferenceFinder)
        {
            const MethodAttributes attributes = MethodAttributes.Private
                                                | MethodAttributes.HideBySig
                                                | MethodAttributes.Static
                                                | MethodAttributes.SpecialName
                                                | MethodAttributes.RTSpecialName;

            var moduleClass = moduleDefinition.Types.FirstOrDefault(_ => _.Name == "<Module>");
            if (moduleClass is null)
            {
                throw new WeavingException("Found no module class!");
            }

            var cctor = moduleClass.Methods.FirstOrDefault(_ => _.Name == ".cctor");
            if (cctor is null)
            {
                cctor = new MethodDefinition(".cctor", attributes, moduleDefinition.ImportReference(msCoreReferenceFinder.GetCoreTypeReference("Void")));
                cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                moduleClass.Methods.Add(cctor);
            }

            var importedMethodToCall = moduleDefinition.ImportReference(methodToCall);

            var insertLocation = Math.Max(cctor.Body.Instructions.Count - 2, 0);
            cctor.Body.Instructions.Insert(insertLocation, Instruction.Create(OpCodes.Call, importedMethodToCall));
        }
    }
}
