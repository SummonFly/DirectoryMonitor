using System.IO;

namespace DirectoryMonitor.Models.Conditions
{
    public abstract class ConditionNode
    {
        public abstract bool IsMet(FileSystemEventArgs args, FileInfo? fileInfo = null);
    }
}
