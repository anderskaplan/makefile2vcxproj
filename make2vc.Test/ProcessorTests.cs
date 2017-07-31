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

            Assert.That(artifacts, Is.EquivalentTo(new BuildArtifact[]
            {
                new BuildArtifact()
                {
                    Type = BuildFileType.Executable,
                    Name = "all",
                    Dependencies = new string[] { "myprog.c" }
                },
                new BuildArtifact()
                {
                    Type = BuildFileType.Source,
                    Name = "myprog.c",
                    Dependencies = new string[] { }
                }
            }));
        }

        [Test]
        public void ProcessVars()
        {
            var artifacts = MakefileProcessor.Process("../../../testdata/vars.mk")
                .ToArray();

            Assert.That(artifacts, Is.EquivalentTo(new BuildArtifact[]
            {
                new BuildArtifact()
                {
                    Type = BuildFileType.Executable,
                    Name = "myprog",
                    Dependencies = new string[] { "myprog.c" }
                },
                new BuildArtifact()
                {
                    Type = BuildFileType.Source,
                    Name = "myprog.c",
                    Dependencies = new string[] { }
                }
            }));
        }
    }
}
