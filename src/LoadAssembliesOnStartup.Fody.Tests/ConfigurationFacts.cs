namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.Xml.Linq;
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

            Assert.That(config.ExcludeAssemblies[0], Is.EqualTo("Foo"));
            Assert.That(config.ExcludeAssemblies[1], Is.EqualTo("Bar"));
            Assert.That(config.ExcludeAssemblies[2], Is.EqualTo("Company.Tools.*"));
        }

        [TestCase]
        public void ExcludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeAssemblies='Foo|Bar|Company.Tools.*'/>");
            var config = new Configuration(xElement);

            Assert.That(config.ExcludeAssemblies[0], Is.EqualTo("Foo"));
            Assert.That(config.ExcludeAssemblies[1], Is.EqualTo("Bar"));
            Assert.That(config.ExcludeAssemblies[2], Is.EqualTo("Company.Tools.*"));
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

            Assert.That(config.ExcludeAssemblies[0], Is.EqualTo("Foo"));
            Assert.That(config.ExcludeAssemblies[1], Is.EqualTo("Bar"));
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

            Assert.That(config.IncludeAssemblies[0], Is.EqualTo("Foo"));
            Assert.That(config.IncludeAssemblies[1], Is.EqualTo("Bar"));
            Assert.That(config.IncludeAssemblies[2], Is.EqualTo("Company.Tools.*"));
        }

        [TestCase]
        public void IncludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup IncludeAssemblies='Foo|Bar|Company.Tools.*'/>");
            var config = new Configuration(xElement);

            Assert.That(config.IncludeAssemblies[0], Is.EqualTo("Foo"));
            Assert.That(config.IncludeAssemblies[1], Is.EqualTo("Bar"));
            Assert.That(config.IncludeAssemblies[2], Is.EqualTo("Company.Tools.*"));
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

            Assert.That(config.IncludeAssemblies[0], Is.EqualTo("Foo"));
            Assert.That(config.IncludeAssemblies[1], Is.EqualTo("Bar"));
        }

        [TestCase]
        public void ExcludeSystemAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />");

            var config = new Configuration(xElement);

            Assert.That(config.ExcludeSystemAssemblies, Is.False);
        }

        [TestCase]
        public void ExcludePrivateAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />");

            var config = new Configuration(xElement);

            Assert.That(config.ExcludePrivateAssemblies, Is.False);
        }

        [TestCase]
        public void ExcludeOptimizedAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeOptimizedAssemblies='true' />");

            var config = new Configuration(xElement);

            Assert.That(config.ExcludeOptimizedAssemblies, Is.False);
        }

        [TestCase]
        public void WrapInTryCatch()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup WrapInTryCatch='true' />");

            var config = new Configuration(xElement);

            Assert.That(config.WrapInTryCatch, Is.True);
        }
    }
}
