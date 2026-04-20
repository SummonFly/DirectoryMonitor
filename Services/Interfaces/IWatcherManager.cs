using DirectoryMonitor.Models.Entities;
using System.IO;

namespace DirectoryMonitor.Services.Interfaces
{
    public interface IWatcherManager
    {
        Task StartWatchingAsync(WatchedPath watchedPath);
        Task StopWatchingAsync(int watchedPathId);
        Task StartAllAsync();
        Task StopAllAsync();
        bool IsWatching(int watchedPathId);
        event EventHandler<FileSystemEventArgs> FileEvent;
    }
}
