using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;

namespace make2vc.Test
{
    [TestFixture]
    public class ParserTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void ParseSimplest()
        {
            var content = MakefileParser.Parse("../../../testdata/simplest.mk")
                .ToArray();

            Assert.That(content.Length, Is.EqualTo(2));
            var first = content[0] as Rule;
            Assert.That(first.FilePath, Does.EndWith("simplest.mk"));
            Assert.That(first.LineNumber, Is.EqualTo(2));
            Assert.That(first.Targets, Is.EqualTo("all"));
            Assert.That(first.Prerequisites, Is.EqualTo("myprog.c"));
            Assert.That(first.Recipe.Length, Is.EqualTo(1));
            Assert.That(first.Recipe[0], Is.EqualTo("gcc -g -Wall -o myprog myprog.c"));

            var second = content[1] as Rule;
            Assert.That(second.LineNumber, Is.EqualTo(5));
            Assert.That(second.Targets, Is.EqualTo("clean"));
        }
    }
}
