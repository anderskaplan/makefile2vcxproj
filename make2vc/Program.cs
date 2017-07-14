using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make2vc
{
    class Program
    {
        static void Main(string[] args)
        {
            var makefilePath = "Makefile";

            makefilePath = "../../../testdata/simplest.mk";
            //makefilePath = "../../../test1/Makefile";

            MakefileProcessor.Process(makefilePath);

            //VcxprojWriter.Write("test.vcxproj");
        }
    }
}
