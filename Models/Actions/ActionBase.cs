using System.IO;

namespace DirectoryMonitor.Models.Actions
{
    public abstract class ActionBase
    {
        public abstract Task ExecuteAsync(FileSystemEventArgs args, CancellationToken cancellationToken = default);
    }
}
