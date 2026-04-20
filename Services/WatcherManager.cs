using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Services.Interfaces;
using System.Collections.Concurrent;
using System.IO;

namespace DirectoryMonitor.Services
{
    public class WatcherManager : IWatcherManager
    {
        private readonly ConcurrentDictionary<int, FileSystemWatcherWrapper> _watchers = new();
        private readonly IJournalService _journalService;
        private readonly IWatchedPathRepository _watchedPathRepository;

        public event EventHandler<FileSystemEventArgs>? FileEvent;

        public WatcherManager(IJournalService journalService, IWatchedPathRepository watchedPathRepository)
        {
            _journalService = journalService;
            _watchedPathRepository = watchedPathRepository;
        }

        public async Task StartWatchingAsync(WatchedPath watchedPath)
        {
            await Task.Run(() =>
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

                wrapper.FileEvent += async (s, e) => await OnFileEvent(e, watchedPath.Id);
                wrapper.Start();

                _watchers[watchedPath.Id] = wrapper;
            });
        }

        public async Task StopWatchingAsync(int watchedPathId)
        {
            await Task.Run(() =>
            {
                if (_watchers.TryRemove(watchedPathId, out var wrapper))
                {
                    wrapper.FileEvent -= async (s, e) => await OnFileEvent(e, watchedPathId);
                    wrapper.Stop();
                    wrapper.Dispose();
                }
            });
        }

        public async Task StartAllAsync()
        {
            var activePaths = await _watchedPathRepository.GetActiveAsync();
            foreach (var path in activePaths)
            {
                await StartWatchingAsync(path);
            }
        }

        public async Task StopAllAsync()
        {
            var activePaths = await _watchedPathRepository.GetActiveAsync();
            foreach (var path in activePaths)
            {
                await StopWatchingAsync(path.Id);
            }
        }

        public bool IsWatching(int watchedPathId)
        {
            return _watchers.ContainsKey(watchedPathId);
        }

        private async Task OnFileEvent(FileSystemEventArgs e, int watchedPathId)
        {
            string eventType = e.ChangeType.ToString();
            string? oldPath = null;

            if (e is RenamedEventArgs renamed)
            {
                oldPath = renamed.OldFullPath;
            }

            await _journalService.LogEventAsync(e.FullPath, eventType, oldPath, watchedPathId);

            FileEvent?.Invoke(this, e);
        }
    }
}
