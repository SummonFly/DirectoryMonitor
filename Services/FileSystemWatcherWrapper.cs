using DirectoryMonitor.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Timers;

namespace DirectoryMonitor.Services
{
    public class FileSystemWatcherWrapper : IDisposable
    {
        private FileSystemWatcher? _watcher;
        private readonly string _path;
        private readonly bool _includeSubdirectories;
        private readonly System.Timers.Timer _debounceTimer;
        private readonly Dictionary<string, DateTime> _lastEventTime = new();
        private const int DebounceMilliseconds = 300;
        private readonly bool _waitForCreation;

        private readonly ISettingsService _settingsService;

        public event EventHandler<FileSystemEventArgs>? FileEvent;

        public FileSystemWatcherWrapper(string path, bool includeSubdirectories, bool waitForCreation = false)
        {
            _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();
            var debounceMs = _settingsService.Settings.DebounceMilliseconds;
            _debounceTimer = new System.Timers.Timer(debounceMs);

            _path = path;
            _includeSubdirectories = includeSubdirectories;
            _waitForCreation = waitForCreation;

            _debounceTimer = new System.Timers.Timer(DebounceMilliseconds);
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += OnDebounceTimerElapsed;

            if (Directory.Exists(path))
            {
                InitializeWatcher();
            }
            else if (waitForCreation)
            {
                WatchParentForCreation();
            }
            else
            {
                // Directory doesn't exist and we're not waiting for it
                // Log and keep watcher null
                System.Diagnostics.Debug.WriteLine($"Directory does not exist: {path}. Watcher not started.");
            }
        }

        private void WatchParentForCreation()
        {
            var parent = FindExistingParent(_path);
            if (string.IsNullOrEmpty(parent))
                return;

            _watcher = new FileSystemWatcher(parent);
            _watcher.IncludeSubdirectories = true;
            _watcher.Created += (s, e) =>
            {
                if (e.FullPath == _path || Directory.Exists(_path))
                {
                    _watcher.Dispose();
                    InitializeWatcher();
                    Start();
                }
            };
            _watcher.EnableRaisingEvents = true;
        }

        private string FindExistingParent(string path)
        {
            if (Directory.Exists(path))
                return path;

            var parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent))
                return string.Empty;

            return FindExistingParent(parent);
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
            var exception = e.GetException();
            var nativeErrorCode = exception.HResult & 0xFFFF;

            // Try to get more specific error if it's a Win32Exception
            if (exception is System.ComponentModel.Win32Exception win32Ex)
            {
                nativeErrorCode = win32Ex.NativeErrorCode;
            }

            System.Diagnostics.Debug.WriteLine($"Watcher error: {exception.Message}, Native error: {nativeErrorCode}");

            switch (nativeErrorCode)
            {
                case 2:     // ERROR_FILE_NOT_FOUND
                case 3:     // ERROR_PATH_NOT_FOUND
                case 5:     // ERROR_ACCESS_DENIED
                case 32:    // ERROR_SHARING_VIOLATION
                case 161:   // ERROR_BAD_PATHNAME
                            // Directory missing or inaccessible — attempt recovery
                    Stop();
                    _watcher?.Dispose();

                    if (_waitForCreation || !Directory.Exists(_path))
                    {
                        WatchParentForCreation();
                    }
                    break;

                default:
                    // Log, but don't attempt recovery for unknown errors
                    System.Diagnostics.Debug.WriteLine($"Unhandled watcher error (Native code {nativeErrorCode}): {exception.Message}");
                    break;
            }
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
