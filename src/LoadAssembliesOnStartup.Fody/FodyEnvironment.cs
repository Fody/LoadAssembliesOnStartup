// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FodyEnvironment.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System;
    using System.Xml.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public static class FodyEnvironment
    {
        public static ModuleDefinition ModuleDefinition { get; set; }
        public static IAssemblyResolver AssemblyResolver { get; set; }

        public static XElement Config { get; set; }

        public static Action<string> WriteDebug { get; set; }

        public static Action<string> WriteInfo { get; set; }

        public static Action<string> WriteWarning { get; set; }

        public static Action<string, SequencePoint> WriteWarningPoint { get; set; }

        public static Action<string> WriteError { get; set; }

        public static Action<string, SequencePoint> WriteErrorPoint { get; set; }
    }
}
