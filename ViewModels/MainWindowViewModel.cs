using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DirectoryMonitor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IJournalService _journalService;
        private readonly IWatchedPathRepository _watchedPathRepository;

        [ObservableProperty]
        private ObservableCollection<Models.Entities.EventLogEntry> _events = new();

        [ObservableProperty]
        private ObservableCollection<WatchedPath> _watchedPaths = new();

        [ObservableProperty]
        private string _eventTypeFilter = "All";

        [ObservableProperty]
        private string _searchText = string.Empty;

        public MainWindowViewModel(IJournalService journalService, IWatchedPathRepository watchedPathRepository)
        {
            _journalService = journalService;
            _watchedPathRepository = watchedPathRepository;

            LoadEventsCommand = new AsyncRelayCommand(LoadEventsAsync);
            LoadPathsCommand = new AsyncRelayCommand(LoadPathsAsync);
            AddPathCommand = new AsyncRelayCommand(AddPathAsync);
            RemovePathCommand = new AsyncRelayCommand(RemovePathAsync);

            LoadEventsCommand.Execute(null);
            LoadPathsCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadEventsCommand { get; }
        public IAsyncRelayCommand LoadPathsCommand { get; }
        public IAsyncRelayCommand AddPathCommand { get; }
        public IAsyncRelayCommand RemovePathCommand { get; }

        private async Task LoadEventsAsync()
        {
            EventType? filter = EventTypeFilter switch
            {
                "Created" => EventType.Created,
                "Changed" => EventType.Changed,
                "Deleted" => EventType.Deleted,
                "Renamed" => EventType.Renamed,
                _ => null
            };

            var events = await _journalService.GetFilteredEventsAsync(filter, SearchText, null, null);
            Events.Clear();
            foreach (var ev in events)
            {
                Events.Add(ev);
            }
        }

        private async Task LoadPathsAsync()
        {
            var paths = await _watchedPathRepository.GetAllAsync();
            WatchedPaths.Clear();
            foreach (var path in paths)
            {
                WatchedPaths.Add(path);
            }
        }

        private async Task AddPathAsync()
        {
            // TODO: Implement folder browser dialog
            var newPath = new WatchedPath
            {
                Path = @"C:\Temp", // Temporary hardcoded
                IsActive = true,
                IncludeSubdirectories = true
            };

            await _watchedPathRepository.AddAsync(newPath);
            await LoadPathsAsync();
        }

        private async Task RemovePathAsync()
        {
            // TODO: Get selected item from UI
            // Temporary placeholder
        }
    }
}
