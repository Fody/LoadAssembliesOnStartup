// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationFacts.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Test
{
    using System.Xml.Linq;
    using Catel.Test;
    using Fody;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigurationFacts
    {
        [TestMethod]
        public void ExcludeAssembliesNode()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup>
    <ExcludeAssemblies>
Foo
Bar
    </ExcludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.ExcludeAssemblies[0]);
            Assert.AreEqual("Bar", config.ExcludeAssemblies[1]);
        }

        [TestMethod]
        public void ExcludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeAssemblies='Foo|Bar'/>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.ExcludeAssemblies[0]);
            Assert.AreEqual("Bar", config.ExcludeAssemblies[1]);
        }

        [TestMethod]
        public void ExcludeAssembliesCombined()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup  ExcludeAssemblies='Foo'>
    <ExcludeAssemblies>
Bar
    </ExcludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.ExcludeAssemblies[0]);
            Assert.AreEqual("Bar", config.ExcludeAssemblies[1]);
        }

        [TestMethod]
        public void IncludeAssembliesNode()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup>
    <IncludeAssemblies>
Foo
Bar
    </IncludeAssemblies>
</LoadAssembliesOnStartup>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.IncludeAssemblies[0]);
            Assert.AreEqual("Bar", config.IncludeAssemblies[1]);
        }

        [TestMethod]
        public void IncludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup IncludeAssemblies='Foo|Bar'/>");
            var config = new Configuration(xElement);

            Assert.AreEqual("Foo", config.IncludeAssemblies[0]);
            Assert.AreEqual("Bar", config.IncludeAssemblies[1]);
        }

        [TestMethod]
        public void IncludeAndExcludeAssembliesAttribute()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup IncludeAssemblies='Bar' ExcludeAssemblies='Foo'/>");

            ExceptionTester.CallMethodAndExpectException<WeavingException>(() => new Configuration(xElement));
        }

        [TestMethod]
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

        [TestMethod]
        public void ExcludeOptimizedAssemblies()
        {
            var xElement = XElement.Parse(@"
<LoadAssembliesOnStartup ExcludeOptimizedAssemblies='true' />");

            var config = new Configuration(xElement);

            Assert.AreEqual(true, config.ExcludeOptimizedAssemblies);
        }
    }
}