// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LoadAssembliesOnStartup
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using LoadAssembliesOnStartup.Weaving;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Weaving;

    public class ModuleWeaver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleWeaver"/> class.
        /// </summary>
        /// <remarks>
        /// The class must contain an empty constructor.
        /// </remarks>
        public ModuleWeaver()
        {
            // Init logging delegates to make testing easier
            LogInfo = s => { };
            LogWarning = s => { };
            LogError = s => { };
        }

        /// <summary>
        /// Gets or sets the configuration element. Contains the full element from <c>FodyWeavers.xml</c>.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public XElement Config { get; set; }

        /// <summary>
        /// Gets or sets the log info delegate.
        /// </summary>
        public Action<string> LogInfo { get; set; }

        public Action<string> LogWarning { get; set; }

        public Action<string, SequencePoint> LogWarningPoint { get; set; }

        public Action<string> LogError { get; set; }

        public Action<string, SequencePoint> LogErrorPoint { get; set; }

        /// <summary>
        /// Gets or sets the assembly resolver. Contains a  <seealso cref="Mono.Cecil.IAssemblyResolver"/> for resolving dependencies.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public IAssemblyResolver AssemblyResolver { get; set; }

        /// <summary>
        /// Gets or sets the module definition. Contains the Cecil representation of the assembly being built.
        /// </summary>
        /// <value>
        /// The module definition.
        /// </value>
        public ModuleDefinition ModuleDefinition { get; set; }

        public void Execute()
        {
            try
            {
//#if DEBUG
//                Debugger.Launch();
//#endif

                InitializeEnvironment();

                // 1st step: set up the basics
                var msCoreReferenceFinder = new MsCoreReferenceFinder(this, ModuleDefinition.AssemblyResolver);
                msCoreReferenceFinder.Execute();

                // 2nd step: create method that imports the types
                var loadTypesWeaver = new LoadTypesWeaver(ModuleDefinition, msCoreReferenceFinder);
                var loadTypesMethod = loadTypesWeaver.Execute();

                // 3rd step: call method on assembly init
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