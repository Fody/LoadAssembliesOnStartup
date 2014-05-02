// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FodyEnvironment.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup
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

        public static Action<string> LogInfo { get; set; }

        public static Action<string> LogWarning { get; set; }

        public static Action<string, SequencePoint> LogWarningPoint { get; set; }

        public static Action<string> LogError { get; set; }

        public static Action<string, SequencePoint> LogErrorPoint { get; set; }
    }
}