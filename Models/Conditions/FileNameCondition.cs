using DirectoryMonitor.Models.Enums;
using System.IO;
using System.Text.RegularExpressions;

namespace DirectoryMonitor.Models.Conditions
{
    public class FileNameCondition : ConditionNode
    {
        public string Pattern { get; set; } = string.Empty;
        public StringMatchType MatchType { get; set; } = StringMatchType.Contains;

        public override bool IsMet(FileSystemEventArgs args, FileInfo? fileInfo = null)
        {
            var fileName = Path.GetFileName(args.FullPath);

            return MatchType switch
            {
                StringMatchType.Equals => fileName.Equals(Pattern, StringComparison.OrdinalIgnoreCase),
                StringMatchType.Contains => fileName.Contains(Pattern, StringComparison.OrdinalIgnoreCase),
                StringMatchType.StartsWith => fileName.StartsWith(Pattern, StringComparison.OrdinalIgnoreCase),
                StringMatchType.EndsWith => fileName.EndsWith(Pattern, StringComparison.OrdinalIgnoreCase),
                StringMatchType.Regex => Regex.IsMatch(fileName, Pattern, RegexOptions.IgnoreCase),
                _ => false
            };
        }
    }
}
