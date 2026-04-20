using System.IO;
using System.Timers;

namespace DirectoryMonitor.Services
{
    public class FileSystemWatcherWrapper : IDisposable
    {
        private FileSystemWatcher? _watcher;
        private readonly string _path;
        private readonly bool _includeSubdirectories;
        private readonly string? _filter;
        private readonly System.Timers.Timer _debounceTimer;
        private readonly Dictionary<string, DateTime> _lastEventTime = new();
        private const int DebounceMilliseconds = 300;

        public event EventHandler<FileSystemEventArgs>? FileEvent;

        public FileSystemWatcherWrapper(string path, bool includeSubdirectories, string? filter = null)
        {
            _path = path;
            _includeSubdirectories = includeSubdirectories;
            _filter = filter;

            _debounceTimer = new System.Timers.Timer(DebounceMilliseconds);
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += OnDebounceTimerElapsed;

            InitializeWatcher();
        }

        private void InitializeWatcher()
        {
            _watcher = new FileSystemWatcher
            {
                Path = _path,
                IncludeSubdirectories = _includeSubdirectories,
                EnableRaisingEvents = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            if (!string.IsNullOrWhiteSpace(_filter))
            {
                _watcher.Filter = _filter;
            }

            _watcher.Created += OnEvent;
            _watcher.Changed += OnEvent;
            _watcher.Deleted += OnEvent;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
        }

        public void Start()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = true;
            }
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
            }
        }

        private void OnEvent(object sender, FileSystemEventArgs e)
        {
            var eventKey = $"{e.FullPath}_{e.ChangeType}";

            lock (_lastEventTime)
            {
                if (_lastEventTime.TryGetValue(eventKey, out var lastTime))
                {
                    if ((DateTime.UtcNow - lastTime).TotalMilliseconds < DebounceMilliseconds)
                    {
                        return; // Duplicate event, ignore
                    }
                }
                _lastEventTime[eventKey] = DateTime.UtcNow;
            }

            _debounceTimer.Stop();
            _debounceTimer.Start();

            lock (_pendingEvents)
            {
                _pendingEvents.Add(e);
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            OnEvent(sender, e);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Watcher error: {e.GetException().Message}");
        }

        private readonly List<FileSystemEventArgs> _pendingEvents = new();

        private void OnDebounceTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            List<FileSystemEventArgs> eventsToProcess;

            lock (_pendingEvents)
            {
                eventsToProcess = new List<FileSystemEventArgs>(_pendingEvents);
                _pendingEvents.Clear();
            }

            foreach (var args in eventsToProcess)
            {
                FileEvent?.Invoke(this, args);
            }
        }

        public void Dispose()
        {
            _debounceTimer?.Dispose();
            _watcher?.Dispose();
        }
    }
}
