using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace make2vc
{
    public enum ItemType
    {
        Error,
        Rule,
        VariableDefinition
    }

    public abstract class Item
    {
        public abstract ItemType Type { get; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
    }

    public class Error : Item
    {
        public override ItemType Type
        {
            get { return ItemType.Error; }
        }

        public string Line { get; set; }
    }

    public class Rule : Item
    {
        public override ItemType Type
        {
            get { return ItemType.Rule; }
        }

        public string Targets { get; set; }
        public string Prerequisites { get; set; }
        public string[] Recipe { get; set; }
    }

    public class VariableDefinition : Item
    {
        public override ItemType Type
        {
            get { return ItemType.VariableDefinition; }
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class MakefileParser
    {
        private static Regex ruleRegex = new Regex(@"^([^\s:#][^:]+)\s*:\s*(.*)$");
        private static Regex variableDefinitionRegex = new Regex(@"^([^\s:#=]+)\s*=\s*(.*)$");

        public static IEnumerable<Item> Parse(string makefilePath)
        {
            using (var stream = new FileStream(makefilePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                string line = null;
                int lineNumber = 0;

                Func<string> nextLine = () =>
                {
                    line = reader.ReadLine();
                    lineNumber++;
                    while (line != null && line.EndsWith("\\"))
                    {
                        line = line.Substring(0, line.Length - 1);
                        line += reader.ReadLine() ?? string.Empty;
                        lineNumber++;
                    }
                    return line;
                };

                nextLine();

                while (line != null)
                {
                    var match = ruleRegex.Match(line);
                    if (match.Success)
                    {
                        yield return new Rule
                        {
                            FilePath = makefilePath,
                            LineNumber = lineNumber,
                            Targets = match.Groups[1].Value,
                            Prerequisites = match.Groups[2].Value,
                            Recipe = ParseRecipe(nextLine).ToArray() // calls nextLine at least once
                        };
                        continue;
                    }

                    match = variableDefinitionRegex.Match(line);
                    if (match.Success)
                    {
                        yield return new VariableDefinition
                        {
                            FilePath = makefilePath,
                            LineNumber = lineNumber,
                            Name = match.Groups[1].Value,
                            Value = match.Groups[2].Value,
                        };
                        nextLine();
                        continue;
                    }

                    if (line.Length == 0 || line.StartsWith("#"))
                    {
                        nextLine();
                        continue;
                    }

                    yield return new Error()
                    {
                        FilePath = makefilePath,
                        LineNumber = lineNumber,
                        Line = line
                    };
                    nextLine();

                    //throw new Exception(string.Format("Malformed input on line {0} ({1}): '{2}'", lineNumber, makefilePath, line));
                }
            }
        }

        private static IEnumerable<string> ParseRecipe(Func<string> nextLine)
        {
            var commands = new List<string>();
            for (var line = nextLine();
                line != null && line.StartsWith("\t");
                line = nextLine())
            {
                commands.Add(line.Substring(1));
            }

            return commands;
        }
    }
}
