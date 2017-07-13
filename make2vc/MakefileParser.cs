using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace make2vc
{
    public class Item
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
    }

    public class Target : Item
    {
        public string Name { get; set; }
        public string[] Dependencies { get; set; }
        public string[] Commands { get; set; }
    }

    public class MakefileParser
    {
        private static Regex targetRegex = new Regex(@"^([a-zA-Z0-9_\-]+)\s*:\s*(.*)$");

        public static IEnumerable<Item> Parse(string makefilePath)
        {
            using (var stream = new FileStream(makefilePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                var line = reader.ReadLine();
                var lineNumber = 1;
                while (line != null)
                {
                    var match = targetRegex.Match(line);
                    if (match.Success)
                    {
                        var target = new Target
                        {
                            FilePath = makefilePath,
                            LineNumber = lineNumber,
                            Name = match.Groups[1].Value,
                            Dependencies = match.Groups[2].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        };
                        var commands = new List<string>();

                        while (true)
                        {
                            line = reader.ReadLine();
                            lineNumber++;

                            if (line != null && line.StartsWith("\t"))
                            {
                                commands.Add(line.Substring(1));
                            }
                            else
                            {
                                break;
                            }
                        }

                        target.Commands = commands.ToArray();
                        yield return target;
                        continue;
                    }

                    if (line.Length == 0 || line.StartsWith("#"))
                    {
                        line = reader.ReadLine();
                        lineNumber++;
                        continue;
                    }

                    throw new Exception(string.Format("Malformed input on line {0} ({1}): '{2}'", lineNumber, makefilePath, line));
                }
            }
        }
    }
}
