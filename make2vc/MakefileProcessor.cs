using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make2vc
{
    class MakefileProcessor
    {
        public static void Process(string makefilePath)
        {
            foreach (var item in MakefileParser.Parse(makefilePath))
            {
                switch (item.Type)
                {
                    case ItemType.Rule:
                        var rule = (Rule)item;
                        Debug.WriteLine(string.Format("rule: '{0}'", rule.Targets));
                        break;

                    case ItemType.VariableDefinition:
                        var definition = (VariableDefinition)item;
                        //Debug.WriteLine("variable definition");
                        break;

                    case ItemType.Error:
                        var error = (Error)item;
                        Debug.WriteLine(string.Format("Malformed input on line {0} ({1}): '{2}'", error.LineNumber, error.FilePath, error.Line));
                        //throw new Exception(string.Format("Malformed input on line {0} ({1}): '{2}'", lineNumber, makefilePath, line));
                        break;
                }
            }
        }
    }
}
