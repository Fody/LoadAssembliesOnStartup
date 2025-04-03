namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    public static class StringExtensions
    {
        public static IEnumerable<string> NonEmpty(this IEnumerable<string> list)
        {
            return list.Select(_ => _.Trim()).Where(_ => _ != string.Empty);
        }
    }
}