using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Services.Interfaces;
using System.Collections.Concurrent;
using System.IO;

namespace DirectoryMonitor.Services
{
    public class WatcherManager : IWatcherManager
    {
        private readonly ConcurrentDictionary<int, FileSystemWatcherWrapper> _watchers = new();

        public event EventHandler<FileSystemEventArgs>? FileEvent;

        public Task StartWatchingAsync(WatchedPath watchedPath)
        {
            return Task.Run(() =>
            {
                if (_watchers.ContainsKey(watchedPath.Id))
                {
                    StopWatchingAsync(watchedPath.Id).Wait();
                }

                var wrapper = new FileSystemWatcherWrapper(
                    watchedPath.Path,
                    watchedPath.IncludeSubdirectories,
                    watchedPath.FileExtensionsFilter
                );

                wrapper.FileEvent += OnFileEvent;
                wrapper.Start();

                _watchers[watchedPath.Id] = wrapper;
            });
        }

        public Task StopWatchingAsync(int watchedPathId)
        {
            return Task.Run(() =>
            {
                if (_watchers.TryRemove(watchedPathId, out var wrapper))
                {
                    wrapper.FileEvent -= OnFileEvent;
                    wrapper.Stop();
                    wrapper.Dispose();
                }
            });
        }

        public Task StartAllAsync()
        {
            // Will be implemented after repository integration
            return Task.CompletedTask;
        }

        public Task StopAllAsync()
        {
            return Task.Run(() =>
            {
                foreach (var id in _watchers.Keys)
                {
                    StopWatchingAsync(id).Wait();
                }
            });
        }

        public bool IsWatching(int watchedPathId)
        {
            return _watchers.ContainsKey(watchedPathId);
        }

        private void OnFileEvent(object? sender, FileSystemEventArgs e)
        {
            FileEvent?.Invoke(this, e);
        }
    }
}
