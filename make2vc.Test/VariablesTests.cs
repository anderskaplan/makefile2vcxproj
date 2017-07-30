using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make2vc.Test
{
    [TestFixture]
    public class VariablesTests
    {
        [Test]
        public void BasicVariableExpansion()
        {
            var variables = new Variables();
            variables.Set("!", ".");

            Assert.That(variables.Expand("*!*"), Is.EqualTo("*!*"), "plain string");
            Assert.That(variables.Expand("*$$*"), Is.EqualTo("*$*"), "*$$* -> *$*");
            Assert.That(variables.Expand("*$!*"), Is.EqualTo("*.*"), "*$!* -> *.*");
            Assert.That(variables.Expand("*$(!)*"), Is.EqualTo("*.*"), "*$(!)* -> *.*");
            Assert.That(variables.Expand("*${!}*"), Is.EqualTo("*.*"), "*${!}* -> *.*");
            Assert.That(variables.Expand("$$"), Is.EqualTo("$"), "$$ -> $");
            Assert.That(variables.Expand("$!"), Is.EqualTo("."), "$! -> .");
            Assert.That(variables.Expand("$(!)"), Is.EqualTo("."), "$(!) -> .");
            Assert.That(variables.Expand("${!}"), Is.EqualTo("."), "${!} -> .");
        }

        [Test]
        public void NestedVariableExpansion()
        {
            var variables = new Variables();
            variables.Set("a", "every");
            variables.Set("b", "thing");
            variables.Set("everything", "%");

            Assert.That(variables.Expand("${${a}$(b)}"), Is.EqualTo("%"));
        }
    }
}
