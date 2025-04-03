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

            if (config.Attribute(nameof(IncludeAssemblies)) is not null || config.Element(nameof(IncludeAssemblies)) is not null)
            {
                OptOut = false;
            }

            ReadList(config, nameof(ExcludeAssemblies), ExcludeAssemblies);
            ReadList(config, nameof(IncludeAssemblies), IncludeAssemblies);
            ReadBool(config, nameof(ExcludePrivateAssemblies), _ => ExcludePrivateAssemblies = _);
            ReadBool(config, nameof(ExcludeSystemAssemblies), _ => ExcludeSystemAssemblies = _);
            ReadBool(config, nameof(ExcludeOptimizedAssemblies), _ => ExcludeOptimizedAssemblies = _);
            ReadBool(config, nameof(WrapInTryCatch), _ => WrapInTryCatch = _);

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
            if (attribute is not null)
            {
                foreach (var item in attribute.Value.Split('|').NonEmpty())
                {
                    list.Add(item);
                }
            }

            var element = config.Element(nodeName);
            if (element is not null)
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
            if (attribute is not null)
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
