using DirectoryMonitor.Models.Enums;
using System.IO;

namespace DirectoryMonitor.Models.Conditions
{
    public class ExtensionCondition : ConditionNode
    {
        public List<string> Extensions { get; set; } = new();
        public ExtensionMatchType MatchType { get; set; } = ExtensionMatchType.Equals;

        public override bool IsMet(FileSystemEventArgs args, FileInfo? fileInfo = null)
        {
            var extension = Path.GetExtension(args.FullPath).ToLowerInvariant();

            if (!extension.StartsWith("."))
                extension = "." + extension;

            return MatchType switch
            {
                ExtensionMatchType.Equals => Extensions.Any(e => e.ToLowerInvariant() == extension),
                ExtensionMatchType.Contains => Extensions.Any(e => extension.Contains(e.ToLowerInvariant())),
                _ => false
            };
        }
    }
}
