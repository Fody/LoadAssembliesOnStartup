// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class Configuration
    {
        public Configuration(XElement config)
        {
            OptOut = true;
            ExcludePrivateAssemblies = true;
            ExcludeSystemAssemblies = true;
            IncludeAssemblies = new List<string>();
            ExcludeAssemblies = new List<string>();

            if (config is null)
            {
                return;
            }

            if (config.Attribute(nameof(IncludeAssemblies)) != null || config.Element(nameof(IncludeAssemblies)) != null)
            {
                OptOut = false;
            }

            ReadList(config, nameof(ExcludeAssemblies), ExcludeAssemblies);
            ReadList(config, nameof(IncludeAssemblies), IncludeAssemblies);
            ReadBool(config, nameof(ExcludePrivateAssemblies), x => ExcludePrivateAssemblies = x);
            ReadBool(config, nameof(ExcludeSystemAssemblies), x => ExcludeSystemAssemblies = x);
            ReadBool(config, nameof(ExcludeOptimizedAssemblies), x => ExcludeOptimizedAssemblies = x);
            ReadBool(config, nameof(WrapInTryCatch), x => WrapInTryCatch = x);

            if (IncludeAssemblies.Any() && ExcludeAssemblies.Any())
            {
                throw new WeavingException("Either configure IncludeAssemblies OR ExcludeAssemblies, not both.");
            }
        }

        public bool OptOut { get; private set; }
        public List<string> IncludeAssemblies { get; private set; }
        public List<string> ExcludeAssemblies { get; private set; }
        public bool ExcludePrivateAssemblies { get; private set; }
        public bool ExcludeSystemAssemblies { get; private set; }
        public bool ExcludeOptimizedAssemblies { get; private set; }
        public bool WrapInTryCatch { get; private set; }

        public static void ReadList(XElement config, string nodeName, List<string> list)
        {
            var attribute = config.Attribute(nodeName);
            if (attribute != null)
            {
                foreach (var item in attribute.Value.Split('|').NonEmpty())
                {
                    list.Add(item);
                }
            }

            var element = config.Element(nodeName);
            if (element != null)
            {
                foreach (var item in element.Value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                            .NonEmpty())
                {
                    list.Add(item);
                }
            }
        }

        public static void ReadBool(XElement config, string nodeName, Action<bool> setter)
        {
            var attribute = config.Attribute(nodeName);
            if (attribute != null)
            {
                if (bool.TryParse(attribute.Value, out var value))
                {
                    setter(value);
                }
                else
                {
                    throw new WeavingException($"Could not parse '{nodeName}' from '{attribute.Value}'.");
                }
            }
        }
    }
}
