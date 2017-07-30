using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make2vc
{
    public enum BuildFileType
    {
        Unknown,
        Executable,
        StaticLibrary,
        DynamicLibrary,
        Intermediate,
        Source,
        Other
    }

    public struct BuildArtifact : IEquatable<BuildArtifact>
    {
        public BuildFileType Type { get; set; }
        public string Name { get; set; }
        public string[] Dependencies { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} [{2} dependencies]", Type, Name, Dependencies.Length);
        }

        public override bool Equals(object obj)
        {
            return obj is BuildArtifact &&
                Equals((BuildArtifact)obj);
        }

        public bool Equals(BuildArtifact other)
        {
            return Type == other.Type &&
                Name.Equals(other.Name) &&
                Dependencies.OrderBy(x => x).SequenceEqual(other.Dependencies.OrderBy(x => x));
        }
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
            var builtTargets = new HashSet<string>();
            var remainingTargets = new Stack<string>(new string[] { defaultGoal });
            while (remainingTargets.Count > 0)
            {
                var currentTarget = remainingTargets.Pop();
                if (!builtTargets.Add(currentTarget))
                {
                    continue;
                }

                var fileType = IdentifyFileType(currentTarget);
                if (!FileTypeIsBuildArtifact(fileType))
                {
                    continue;
                }

                Rule rule;
                string[] dependencies;
                if (targets.TryGetValue(currentTarget, out rule))
                {
                    dependencies = ExpandVariables(rule.Prerequisites, variables)
                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else if (File.Exists(MakePath(currentTarget, makefilePath)))
                {
                    dependencies = new string[] { };
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Don't know how to make {0}", currentTarget));
                }

                artifacts.Add(new BuildArtifact()
                {
                    Type = fileType,
                    Name = currentTarget,
                    Dependencies = dependencies
                });

                foreach (var dependency in dependencies)
                {
                    remainingTargets.Push(dependency);
                }
            }

            return artifacts;
        }

        private static string MakePath(string target, string makefilePath)
        {
            return Path.Combine(Path.GetDirectoryName(makefilePath), target);
        }

        private static bool FileTypeIsBuildArtifact(BuildFileType fileType)
        {
            return fileType != BuildFileType.Unknown &&
                fileType != BuildFileType.Other;
        }

        private static BuildFileType IdentifyFileType(string currentTarget)
        {
            var fileExtension = Path.GetExtension(currentTarget).ToLowerInvariant();
            switch (fileExtension)
            {
                case "":
                    return BuildFileType.Executable;

                case ".c":
                case ".cc":
                case ".cpp":
                case ".h":
                case ".hpp":
                    return BuildFileType.Source;

                default:
                    return BuildFileType.Unknown;
            }
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
