namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System.Xml.Linq;
    using Fody;
    using System.Threading.Tasks;
    using TUnit.Assertions;
    using TUnit.Core;

    public class ConfigurationFacts
    {
        [Test]
        public async Task ExcludeAssembliesNode()
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

            await Assert.That(config.ExcludeAssemblies[0]).IsEqualTo("Foo");
            await Assert.That(config.ExcludeAssemblies[1]).IsEqualTo("Bar");
            await Assert.That(config.ExcludeAssemblies[2]).IsEqualTo("Company.Tools.*");
        }

        [Test]
        public async Task ExcludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeAssemblies='Foo|Bar|Company.Tools.*'/>");
            var config = new Configuration(xElement);

            await Assert.That(config.ExcludeAssemblies[0]).IsEqualTo("Foo");
            await Assert.That(config.ExcludeAssemblies[1]).IsEqualTo("Bar");
            await Assert.That(config.ExcludeAssemblies[2]).IsEqualTo("Company.Tools.*");
        }

        [Test]
        public async Task ExcludeAssembliesCombined()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeAssemblies='Foo'>
    <ExcludeAssemblies>
Bar
    </ExcludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            await Assert.That(config.ExcludeAssemblies[0]).IsEqualTo("Foo");
            await Assert.That(config.ExcludeAssemblies[1]).IsEqualTo("Bar");
        }

        [Test]
        public async Task IncludeAssembliesNode()
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

            await Assert.That(config.IncludeAssemblies[0]).IsEqualTo("Foo");
            await Assert.That(config.IncludeAssemblies[1]).IsEqualTo("Bar");
            await Assert.That(config.IncludeAssemblies[2]).IsEqualTo("Company.Tools.*");
        }

        [Test]
        public async Task IncludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup IncludeAssemblies='Foo|Bar|Company.Tools.*'/>");
            var config = new Configuration(xElement);

            await Assert.That(config.IncludeAssemblies[0]).IsEqualTo("Foo");
            await Assert.That(config.IncludeAssemblies[1]).IsEqualTo("Bar");
            await Assert.That(config.IncludeAssemblies[2]).IsEqualTo("Company.Tools.*");
        }

        [Test]
        public async Task IncludeAndExcludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup IncludeAssemblies='Bar' ExcludeAssemblies='Foo'/>");

            await Assert.That(() => new Configuration(xElement)).Throws<WeavingException>();
        }

        [Test]
        public async Task IncludeAssembliesCombined()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup  IncludeAssemblies='Foo'>
    <IncludeAssemblies>
Bar
    </IncludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            await Assert.That(config.IncludeAssemblies[0]).IsEqualTo("Foo");
            await Assert.That(config.IncludeAssemblies[1]).IsEqualTo("Bar");
        }

        [Test]
        public async Task ExcludeSystemAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />");

            var config = new Configuration(xElement);

            await Assert.That(config.ExcludeSystemAssemblies).IsFalse();
        }

        [Test]
        public async Task ExcludePrivateAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />");

            var config = new Configuration(xElement);

            await Assert.That(config.ExcludePrivateAssemblies).IsFalse();
        }

        [Test]
        public async Task ExcludeOptimizedAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeOptimizedAssemblies='true' />");

            var config = new Configuration(xElement);

            await Assert.That(config.ExcludeOptimizedAssemblies).IsTrue();
        }

        [Test]
        public async Task WrapInTryCatch()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup WrapInTryCatch='true' />");

            var config = new Configuration(xElement);

            await Assert.That(config.WrapInTryCatch).IsTrue();
        }
    }
}
