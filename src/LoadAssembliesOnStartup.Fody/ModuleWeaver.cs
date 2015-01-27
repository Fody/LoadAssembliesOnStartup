// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LoadAssembliesOnStartup.Fody
{
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Weaving;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class ModuleWeaver
    {
        public ModuleWeaver()
        {
            // Init logging delegates to make testing easier
            LogInfo = s => { };
            LogWarning = s => { };
            LogError = s => { };
        }

        public XElement Config { get; set; }

        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }
        public Action<string, SequencePoint> LogWarningPoint { get; set; }
        public Action<string> LogError { get; set; }
        public Action<string, SequencePoint> LogErrorPoint { get; set; }

        public IAssemblyResolver AssemblyResolver { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public string References { get; set; }

        public void Execute()
        {
            try
            {
#if DEBUG
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
#endif

                InitializeEnvironment();

                // Read config
                var configuration = new Configuration(Config);

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
                LogError(ex.Message);

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
            FodyEnvironment.LogInfo = LogInfo;
            FodyEnvironment.LogWarning = LogWarning;
            FodyEnvironment.LogWarningPoint = LogWarningPoint;
            FodyEnvironment.LogError = LogError;
            FodyEnvironment.LogErrorPoint = LogErrorPoint;
        }
    }
}