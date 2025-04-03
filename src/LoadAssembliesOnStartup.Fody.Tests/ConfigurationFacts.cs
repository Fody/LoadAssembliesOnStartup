namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.Xml.Linq;
    using Catel.Tests;
    using Fody;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationFacts
    {
        [TestCase]
        public void ExcludeAssembliesNode()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup>
    <ExcludeAssemblies>
Foo
Bar
Company.Tools.*
    </ExcludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.ExcludeAssemblies[0]);
            Assert.AreEqual("Bar", config.ExcludeAssemblies[1]);
            Assert.AreEqual("Company.Tools.*", config.ExcludeAssemblies[2]);
        }

        [TestCase]
        public void ExcludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeAssemblies='Foo|Bar|Company.Tools.*'/>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.ExcludeAssemblies[0]);
            Assert.AreEqual("Bar", config.ExcludeAssemblies[1]);
            Assert.AreEqual("Company.Tools.*", config.ExcludeAssemblies[2]);
        }

        [TestCase]
        public void ExcludeAssembliesCombined()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeAssemblies='Foo'>
    <ExcludeAssemblies>
Bar
    </ExcludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.ExcludeAssemblies[0]);
            Assert.AreEqual("Bar", config.ExcludeAssemblies[1]);
        }

        [TestCase]
        public void IncludeAssembliesNode()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup>
    <IncludeAssemblies>
Foo
Bar
Company.Tools.*
    </IncludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.IncludeAssemblies[0]);
            Assert.AreEqual("Bar", config.IncludeAssemblies[1]);
            Assert.AreEqual("Company.Tools.*", config.IncludeAssemblies[2]);
        }

        [TestCase]
        public void IncludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup IncludeAssemblies='Foo|Bar|Company.Tools.*'/>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.IncludeAssemblies[0]);
            Assert.AreEqual("Bar", config.IncludeAssemblies[1]);
            Assert.AreEqual("Company.Tools.*", config.IncludeAssemblies[2]);
        }

        [TestCase]
        public void IncludeAndExcludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup IncludeAssemblies='Bar' ExcludeAssemblies='Foo'/>");

            ExceptionTester.CallMethodAndExpectException<WeavingException>(() => new Configuration(xElement));
        }

        [TestCase]
        public void IncludeAssembliesCombined()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup  IncludeAssemblies='Foo'>
    <IncludeAssemblies>
Bar
    </IncludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.IncludeAssemblies[0]);
            Assert.AreEqual("Bar", config.IncludeAssemblies[1]);
        }

        [TestCase]
        public void ExcludeSystemAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />");

            var config = new Configuration(xElement);

            Assert.AreEqual(false, config.ExcludeSystemAssemblies);
        }

        [TestCase]
        public void ExcludePrivateAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />");

            var config = new Configuration(xElement);

            Assert.AreEqual(false, config.ExcludePrivateAssemblies);
        }

        [TestCase]
        public void ExcludeOptimizedAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeOptimizedAssemblies='true' />");

            var config = new Configuration(xElement);

            Assert.AreEqual(true, config.ExcludeOptimizedAssemblies);
        }

        [TestCase]
        public void WrapInTryCatch()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup WrapInTryCatch='true' />");

            var config = new Configuration(xElement);

            Assert.AreEqual(true, config.WrapInTryCatch);
        }
    }
}
