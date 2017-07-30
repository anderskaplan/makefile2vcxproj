using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make2vc
{
    public enum BuildArtifactType
    {
        Unknown,
        Executable,
        StaticLibrary,
        DynamicLibrary
    }

    public struct BuildArtifact
    {
        public BuildArtifactType Type { get; set; }
        public string Name { get; set; }
        public string[] Inputs { get; set; }
    }

    public class MakefileProcessor
    {
        public static IEnumerable<BuildArtifact> Process(string makefilePath)
        {
            var variables = new Dictionary<string, string>();
            var targets = new Dictionary<string, Rule>();
            string defaultGoal;
            ParseMakefile(makefilePath, variables, targets, out defaultGoal);

            var artifacts = new List<BuildArtifact>();

            // push the default goal on the stack of targets that need to be built
            // while targets-to-build is nonempty:
            //   pop top
            //   if already marked as built: skip
            //   mark as built
            //   what kind of target (i.e. file) is top? (executable, lib, obj, source, other) -- identify by .exe .lib .dll etc
            //   if "non-interesting" target: skip
            //   add all dependencies to targets-to-build
            //   add a build artifact node



            return artifacts;
        }

        private static void ParseMakefile(string makefilePath, Dictionary<string, string> variables, Dictionary<string, Rule> targets, out string defaultGoal)
        {
            defaultGoal = null;

            foreach (var item in MakefileParser.Parse(makefilePath))
            {
                switch (item.Type)
                {
                    case ItemType.Rule:
                        var rule = (Rule)item;
                        var targetNames = ExpandVariables(rule.Targets, variables);
                        foreach (var targetName in targetNames
                            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (defaultGoal == null && !targetName.StartsWith("."))
                            {
                                defaultGoal = targetName;
                            }

                            targets[targetName] = rule;
                        }
                        //Debug.WriteLine(string.Format("rule with targets: '{0}'", targetNames));
                        break;

                    case ItemType.VariableDefinition:
                        var definition = (VariableDefinition)item;
                        variables[ExpandVariables(definition.Name, variables)] = definition.Value;
                        //Debug.WriteLine("variable definition");
                        break;

                    case ItemType.Error:
                        var error = (Error)item;
                        Debug.WriteLine(string.Format("ERROR: malformed input at {0} line {1}: '{2}'", error.FilePath, error.LineNumber, error.Line));
                        //throw new Exception(string.Format("Malformed input on line {0} ({1}): '{2}'", lineNumber, makefilePath, line));
                        break;
                }
            }

            Debug.WriteLine(string.Format("{0} targets", targets.Count));
        }

        private static string ExpandVariables(string text, Dictionary<string, string> variables)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '$' && i + 1 < text.Length)
                {
                    switch (text[i + 1])
                    {
                        case '$':
                            sb.Append('$');
                            i++;
                            break;

                        case '(':
                            {
                                var name = Scan(text, i + 2, ')');
                                i = i + 1 + name.Length;
                                sb.Append(ValueOrEmptyString(name, variables));
                            }
                            break;

                        case '{':
                            {
                                var name = Scan(text, i + 2, '}');
                                i = i + 1 + name.Length;
                                sb.Append(ValueOrEmptyString(name, variables));
                            }
                            break;

                        default:
                            sb.Append(ValueOrEmptyString(text.Substring(i + 1, 1), variables));
                            i++;
                            break;
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
            }

            return sb.ToString();
        }

        private static string ValueOrEmptyString(string name, Dictionary<string, string> variables)
        {
            var value = string.Empty;
            variables.TryGetValue(name, out value);
            return value;
        }

        private static string Scan(string text, int startIndex, char terminator)
        {
            var endIndex = Math.Max(startIndex, text.IndexOf(terminator, startIndex));
            return text.Substring(startIndex, endIndex - startIndex);
        }
    }
}
