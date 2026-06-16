using DirectoryMonitor.Models.Enums;
using System.IO;

namespace DirectoryMonitor.Models.Conditions
{
    public class SizeCondition : ConditionNode
    {
        public SizeOperator Operator { get; set; } = SizeOperator.Greater;
        public long Value { get; set; }
        public SizeUnit Unit { get; set; } = SizeUnit.Bytes;

        public override bool IsMet(FileSystemEventArgs args, FileInfo? fileInfo = null)
        {
            if (fileInfo == null && args.ChangeType == WatcherChangeTypes.Deleted)
                return false;

            var file = fileInfo ?? new FileInfo(args.FullPath);
            if (!file.Exists)
                return false;

            long bytes = file.Length;
            long targetBytes = Unit switch
            {
                SizeUnit.KB => Value * 1024,
                SizeUnit.MB => Value * 1024 * 1024,
                SizeUnit.GB => Value * 1024 * 1024 * 1024,
                _ => Value
            };

            return Operator switch
            {
                SizeOperator.Greater => bytes > targetBytes,
                SizeOperator.Less => bytes < targetBytes,
                SizeOperator.Equal => bytes == targetBytes,
                _ => false
            };
        }
    }
}
