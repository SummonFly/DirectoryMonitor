using System.IO;

namespace DirectoryMonitor.Models.Conditions
{
    public enum ItemType
    {
        File,
        Directory
    }
    public class ItemTypeCondition : ConditionNode
    {
        public ItemType Type { get; set; } = ItemType.File;

        public override bool IsMet(FileSystemEventArgs args, FileInfo? fileInfo = null)
        {
            if (args.ChangeType == WatcherChangeTypes.Deleted)
            {
                return Type == ItemType.File;
            }

            var isDirectory = Directory.Exists(args.FullPath);
            return Type == ItemType.File ? !isDirectory : isDirectory;
        }
    }
}
