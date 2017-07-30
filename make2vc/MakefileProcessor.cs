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
            var variables = new Variables();
            var targets = new Dictionary<string, Rule>();
            string defaultGoal;
            ParseMakefile(makefilePath, variables, targets, out defaultGoal);

            var artifacts = new List<BuildArtifact>();
            var builtTargets = new HashSet<string>();
            var remainingTargets = new Stack<string>(new string[] { defaultGoal });
            while (remainingTargets.Count > 0)
            {
                var target = remainingTargets.Pop();
                if (!builtTargets.Add(target))
                {
                    continue;
                }

                var fileType = IdentifyFileType(target);
                if (!FileTypeIsBuildArtifact(fileType))
                {
                    continue;
                }

                string[] dependencies = GetDependencies(
                    target, makefilePath, variables, targets);

                artifacts.Add(new BuildArtifact()
                {
                    Type = fileType,
                    Name = target,
                    Dependencies = dependencies
                });

                foreach (var dependency in dependencies)
                {
                    remainingTargets.Push(dependency);
                }
            }

            return artifacts;
        }

        private static string[] GetDependencies(string target, string makefilePath, Variables variables, Dictionary<string, Rule> targets)
        {
            Rule rule;
            if (targets.TryGetValue(target, out rule))
            {
                return variables.Expand(rule.Prerequisites)
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (File.Exists(MakePath(target, makefilePath)))
            {
                return new string[] { };
            }
            else
            {
                throw new InvalidOperationException(string.Format("Don't know how to make {0}", target));
            }
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

        private static void ParseMakefile(string makefilePath, Variables variables, Dictionary<string, Rule> targets, out string defaultGoal)
        {
            defaultGoal = null;

            foreach (var item in MakefileParser.Parse(makefilePath))
            {
                switch (item.Type)
                {
                    case ItemType.Rule:
                        var rule = (Rule)item;
                        var targetNames = variables.Expand(rule.Targets);
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
                        variables.Set(variables.Expand(definition.Name), definition.Value);
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
    }
}
