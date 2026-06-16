using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.Services.Interfaces;
using DirectoryMonitor.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace DirectoryMonitor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IJournalService _journalService;
        private readonly IWatchedPathRepository _watchedPathRepository;
        private readonly IWatcherManager _watcherManager;
        private readonly IWatchedPathRuleRepository _watchedPathRuleRepository;
        private readonly IRuleEngine _ruleEngine;
        private readonly IRuleRepository _ruleRepository;
        private readonly IRuleActionRepository _ruleActionRepository;


        [ObservableProperty]
        private ObservableCollection<RuleItemViewModel> _rules = new();

        [ObservableProperty]
        private ObservableCollection<EventLogEntry> _events = new();

        [ObservableProperty]
        private ObservableCollection<WatchedPathItemViewModel> _watchedPaths = new();

        [ObservableProperty]
        private WatchedPathItemViewModel? _selectedWatchedPath;

        [ObservableProperty]
        private string _eventTypeFilter = "All";

        [ObservableProperty]
        private string _searchText = string.Empty;



        public MainWindowViewModel(
            IJournalService journalService,
            IWatchedPathRepository watchedPathRepository,
            IWatcherManager watcherManager,
            IWatchedPathRuleRepository watchedPathRuleRepository,
            IRuleEngine ruleEngine,
            IRuleRepository ruleRepository,
            IRuleActionRepository ruleActionRepository)
        {
            _journalService = journalService;
            _watchedPathRepository = watchedPathRepository;
            _watcherManager = watcherManager;
            _watchedPathRuleRepository = watchedPathRuleRepository;
            _ruleEngine = ruleEngine;
            _ruleRepository = ruleRepository;
            _ruleActionRepository = ruleActionRepository;


            // Commands
            LoadEventsCommand = new AsyncRelayCommand(LoadEventsAsync);
            LoadPathsCommand = new AsyncRelayCommand(LoadPathsAsync);
            AddPathCommand = new AsyncRelayCommand(AddPathAsync);
            RemovePathCommand = new AsyncRelayCommand(RemovePathAsync, () => SelectedWatchedPath != null);
            RefreshPathsCommand = new AsyncRelayCommand(LoadPathsAsync);
            RefreshEventsCommand = new AsyncRelayCommand(LoadEventsAsync);
            EditPathCommand = new AsyncRelayCommand(EditPathAsync, () => SelectedWatchedPath != null);
            EditPathWithParameterCommand = new AsyncRelayCommand<WatchedPathItemViewModel>(EditPathWithParameterAsync);
            RemovePathWithParameterCommand = new AsyncRelayCommand<WatchedPathItemViewModel>(RemovePathWithParameterAsync);
            AddRuleCommand = new AsyncRelayCommand(AddRuleAsync);
            RefreshRulesCommand = new AsyncRelayCommand(LoadRulesAsync);
            EditRuleWithParameterCommand = new AsyncRelayCommand<RuleItemViewModel>(EditRuleWithParameterAsync);
            DeleteRuleWithParameterCommand = new AsyncRelayCommand<RuleItemViewModel>(DeleteRuleWithParameterAsync);

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EventTypeFilter) || e.PropertyName == nameof(SearchText))
                {
                    LoadEventsCommand.Execute(null);
                }
            };

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
        public IAsyncRelayCommand RefreshEventsCommand { get; }
        public IAsyncRelayCommand EditPathCommand { get; }
        public IAsyncRelayCommand<WatchedPathItemViewModel> EditPathWithParameterCommand { get; }
        public IAsyncRelayCommand<WatchedPathItemViewModel> RemovePathWithParameterCommand { get; }
        public IAsyncRelayCommand AddRuleCommand { get; }
        public IAsyncRelayCommand RefreshRulesCommand { get; }
        public IAsyncRelayCommand<RuleItemViewModel> EditRuleWithParameterCommand { get; }
        public IAsyncRelayCommand<RuleItemViewModel> DeleteRuleWithParameterCommand { get; }

        private async Task LoadEventsAsync()
        {
            EventType? eventType = EventTypeFilter switch
            {
                "Created" => EventType.Created,
                "Changed" => EventType.Changed,
                "Deleted" => EventType.Deleted,
                "Renamed" => EventType.Renamed,
                _ => null
            };

            var events = await _journalService.GetFilteredEventsAsync(eventType, SearchText, null, null);

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
                    WatchedPaths.Add(new WatchedPathItemViewModel(path));
                }
            });
        }

        partial void OnSelectedWatchedPathChanged(WatchedPathItemViewModel? value)
        {
            RemovePathCommand.NotifyCanExecuteChanged();
            EditPathCommand.NotifyCanExecuteChanged();
        }

        private async Task AddPathAsync()
        {
            var dialog = new AddWatchedPathWindow();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.NewWatchedPath != null)
            {
                var newPath = dialog.NewWatchedPath;

                await _watchedPathRepository.AddAsync(newPath);
                await LoadPathsAsync();

                // Save rule associations
                if (dialog.SelectedRuleIds != null && dialog.SelectedRuleIds.Any())
                {
                    await _watchedPathRuleRepository.UpdateRulesForWatchedPathAsync(newPath.Id, dialog.SelectedRuleIds);
                }

                await _ruleEngine.ReloadRulesAsync();

                // Start watcher if active
                if (newPath.IsActive)
                {
                    await _watcherManager.StartWatchingAsync(newPath);
                }
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
                await _watcherManager.StopWatchingAsync(SelectedWatchedPath.Model.Id);

                // Remove from database
                await _watchedPathRepository.DeleteAsync(SelectedWatchedPath.Model.Id);

                await _ruleEngine.ReloadRulesAsync();

                // Refresh UI
                await LoadPathsAsync();
            }
        }

        private async Task EditPathAsync()
        {
            if (SelectedWatchedPath == null) return;

            var dialog = new WatchedPathEditorWindow(SelectedWatchedPath.Model);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.UpdatedWatchedPath != null)
            {
                await _watchedPathRepository.UpdateAsync(dialog.UpdatedWatchedPath);
                await LoadPathsAsync();
                await _ruleEngine.ReloadRulesAsync();
                // Restart watcher if needed
                await _watcherManager.StopWatchingAsync(dialog.UpdatedWatchedPath.Id);
                if (dialog.UpdatedWatchedPath.IsActive)
                {
                    await _watcherManager.StartWatchingAsync(dialog.UpdatedWatchedPath);
                }
            }
        }

        private async Task EditPathWithParameterAsync(WatchedPathItemViewModel? item)
        {
            if (item == null) return;
            SelectedWatchedPath = item;
            await EditPathAsync();
        }

        private async Task RemovePathWithParameterAsync(WatchedPathItemViewModel? item)
        {
            if (item == null) return;
            SelectedWatchedPath = item;
            await RemovePathAsync();
        }

        private async Task LoadRulesAsync()
        {
            var rules = await _ruleRepository.GetAllAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Rules.Clear();
                foreach (var rule in rules.OrderBy(r => r.Priority))
                {
                    Rules.Add(new RuleItemViewModel(rule));
                }
            });
        }

        private async Task AddRuleAsync()
        {
            var dialog = new RuleEditorWindow();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultRule != null)
            {
                var rule = dialog.ResultRule;
                await _ruleRepository.AddAsync(rule);
                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }

        private async Task EditRuleWithParameterAsync(RuleItemViewModel? item)
        {
            if (item == null) return;

            var rule = await _ruleRepository.GetByIdAsync(item.Model.Id);
            if (rule == null) return;

            var dialog = new RuleEditorWindow(rule);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultRule != null)
            {
                dialog.ResultRule.Id = rule.Id;
                dialog.ResultRule.CreatedAt = rule.CreatedAt;
                await _ruleRepository.UpdateAsync(dialog.ResultRule);

                // Update RuleAction associations
                await _ruleActionRepository.DeleteByRuleIdAsync(rule.Id);
                foreach (var actionItem in dialog.AssignedActions)
                {
                    await _ruleActionRepository.AddAsync(new RuleAction
                    {
                        RuleId = rule.Id,
                        ActionId = actionItem.Id,
                        Order = actionItem.Order
                    });
                }

                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }

        private async Task DeleteRuleWithParameterAsync(RuleItemViewModel? item)
        {
            if (item == null) return;

            var result = MessageBox.Show($"Delete rule '{item.Name}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _ruleRepository.DeleteAsync(item.Model.Id);
                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }
    }
}
