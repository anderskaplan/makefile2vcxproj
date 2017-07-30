using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make2vc.Test
{
    [TestFixture]
    class ProcessorTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void ProcessSimplest()
        {
            var artifacts = MakefileProcessor.Process("../../../testdata/simplest.mk")
                .ToArray();
            Assert.That(artifacts.Length, Is.EqualTo(1));
            var artifact = artifacts[1];
            Assert.That(artifact.Type, Is.EqualTo(BuildArtifactType.Executable));
            Assert.That(artifact.Name, Is.EqualTo("all"));
            Assert.That(artifact.Inputs, Is.EquivalentTo("myprog.c"));
        }
    }
}
