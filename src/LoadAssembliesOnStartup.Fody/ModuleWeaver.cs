// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LoadAssembliesOnStartup.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;
    using global::Fody;
    using Weaving;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class ModuleWeaver : BaseModuleWeaver
    {
        public IAssemblyResolver AssemblyResolver { get; set; }

        public override bool ShouldCleanReference => true;

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            var assemblies = new List<string>();

            // For now just return all references
            assemblies.Add("netstandard");
            assemblies.AddRange(ModuleDefinition.AssemblyReferences.Select(_ => _.Name));

            return assemblies;
        }

        public override void Execute()
        {
            try
            {
#if DEBUG
                if (!Debugger.IsAttached)
                {
                    //Debugger.Launch();
                }
#endif

                // First of all, set the assembly resolver
                if (AssemblyResolver is null)
                {
                    AssemblyResolver = ModuleDefinition.AssemblyResolver;
                }

                // Clear cache because static members will be re-used over multiple builds over multiple systems
                CacheHelper.ClearAllCaches();

                InitializeEnvironment();

                // Read config
                var configuration = new Configuration(Config);

                WriteInfo($"LoadAssembliesOnStartup.Fody v{GetType().Assembly.GetVersion()}");

                // Set up the basics
                var msCoreReferenceFinder = new MsCoreReferenceFinder(this, ModuleDefinition.AssemblyResolver);
                msCoreReferenceFinder.Execute();

                // Create method that imports the types
                var loadTypesWeaver = new LoadTypesWeaver(ModuleDefinition, msCoreReferenceFinder, configuration, this);
                var loadTypesMethod = loadTypesWeaver.Execute();

                // Call method on assembly init
                var moduleLoaderImporter = new ModuleLoaderImporter();
                moduleLoaderImporter.ImportModuleLoader(ModuleDefinition, loadTypesMethod, msCoreReferenceFinder);
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);

#if DEBUG
                Debugger.Launch();
#endif
            }
        }

        private void InitializeEnvironment()
        {
            FodyEnvironment.ModuleDefinition = ModuleDefinition;
            FodyEnvironment.AssemblyResolver = AssemblyResolver;

            FodyEnvironment.Config = Config;
            FodyEnvironment.WriteDebug = WriteDebug;
            FodyEnvironment.WriteInfo = WriteInfo;
            FodyEnvironment.WriteWarning = WriteWarning;
            FodyEnvironment.WriteWarningPoint = WriteWarning;
            FodyEnvironment.WriteError = WriteError;
            FodyEnvironment.WriteErrorPoint = WriteError;

            if (string.IsNullOrEmpty(References))
            {
                References = string.Empty;
            }
        }
    }
}
