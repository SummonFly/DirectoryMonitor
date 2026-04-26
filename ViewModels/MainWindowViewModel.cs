using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace DirectoryMonitor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IJournalService _journalService;
        private readonly IWatchedPathRepository _watchedPathRepository;
        private readonly IWatcherManager _watcherManager;

        [ObservableProperty]
        private ObservableCollection<Models.Entities.EventLogEntry> _events = new();

        [ObservableProperty]
        private ObservableCollection<WatchedPath> _watchedPaths = new();

        [ObservableProperty]
        private WatchedPath? _selectedWatchedPath;

        [ObservableProperty]
        private string _eventTypeFilter = "All";

        [ObservableProperty]
        private string _searchText = string.Empty;

        public MainWindowViewModel(
            IJournalService journalService,
            IWatchedPathRepository watchedPathRepository,
            IWatcherManager watcherManager)
        {
            _journalService = journalService;
            _watchedPathRepository = watchedPathRepository;
            _watcherManager = watcherManager;

            // Commands
            LoadEventsCommand = new AsyncRelayCommand(LoadEventsAsync);
            LoadPathsCommand = new AsyncRelayCommand(LoadPathsAsync);
            AddPathCommand = new AsyncRelayCommand(AddPathAsync);
            RemovePathCommand = new AsyncRelayCommand(RemovePathAsync, () => SelectedWatchedPath != null);
            RefreshPathsCommand = new AsyncRelayCommand(LoadPathsAsync);

            // Auto-refresh journal every 3 seconds
            var timer = new System.Timers.Timer(3000);
            timer.Elapsed += async (s, e) => await LoadEventsAsync();
            timer.Start();

            // Load initial data
            LoadEventsCommand.Execute(null);
            LoadPathsCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadEventsCommand { get; }
        public IAsyncRelayCommand LoadPathsCommand { get; }
        public IAsyncRelayCommand AddPathCommand { get; }
        public IAsyncRelayCommand RemovePathCommand { get; }
        public IAsyncRelayCommand RefreshPathsCommand { get; }

        private async Task LoadEventsAsync()
        {
            var events = await _journalService.GetRecentEventsAsync(200);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Events.Clear();
                foreach (var ev in events)
                {
                    Events.Add(ev);
                }
            });
        }

        private async Task LoadPathsAsync()
        {
            var paths = await _watchedPathRepository.GetAllAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                WatchedPaths.Clear();
                foreach (var path in paths)
                {
                    WatchedPaths.Add(path);
                }
            });
        }

        partial void OnSelectedWatchedPathChanged(WatchedPath? value)
        {
            RemovePathCommand.NotifyCanExecuteChanged();
        }

        private async Task AddPathAsync()
        {
            // Use Windows Forms folder browser dialog
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select folder to monitor";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var newPath = new WatchedPath
                {
                    Path = dialog.SelectedPath,
                    IsActive = true,
                    IncludeSubdirectories = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _watchedPathRepository.AddAsync(newPath);
                await LoadPathsAsync();

                // Start watcher for this path
                await _watcherManager.StartWatchingAsync(newPath);
            }
        }

        private async Task RemovePathAsync()
        {
            if (SelectedWatchedPath == null) return;

            var result = MessageBox.Show($"Stop monitoring and remove '{SelectedWatchedPath.Path}'?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Stop watcher first
                await _watcherManager.StopWatchingAsync(SelectedWatchedPath.Id);

                // Remove from database
                await _watchedPathRepository.DeleteAsync(SelectedWatchedPath.Id);

                // Refresh UI
                await LoadPathsAsync();
            }
        }
    }
}
