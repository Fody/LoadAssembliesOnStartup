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
            IncludeAssemblies = new List<string>();
            ExcludeAssemblies = new List<string>();

            if (config == null)
            {
                return;
            }

            if (config.Attribute("IncludeAssemblies") != null || config.Element("IncludeAssemblies") != null)
            {
                OptOut = false;
            }

            ReadList(config, "ExcludeAssemblies", ExcludeAssemblies);
            ReadList(config, "IncludeAssemblies", IncludeAssemblies);

            if (IncludeAssemblies.Any() && ExcludeAssemblies.Any())
            {
                throw new WeavingException("Either configure IncludeAssemblies OR ExcludeAssemblies, not both.");
            }
        }

        public bool OptOut { get; private set; }
        public List<string> IncludeAssemblies { get; private set; }
        public List<string> ExcludeAssemblies { get; private set; }

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
    }
}