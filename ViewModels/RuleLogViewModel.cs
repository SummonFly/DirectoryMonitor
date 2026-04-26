using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using System.Collections.ObjectModel;
using System.Windows;

namespace DirectoryMonitor.ViewModels
{
    public partial class RuleLogViewModel : ObservableObject
    {
        private readonly IRuleExecutionLogRepository _logRepository;

        [ObservableProperty]
        private ObservableCollection<RuleExecutionLogEntry> _logs = new();

        public RuleLogViewModel(IRuleExecutionLogRepository logRepository)
        {
            _logRepository = logRepository;
            LoadLogsCommand = new AsyncRelayCommand(LoadLogsAsync);
            ClearLogsCommand = new AsyncRelayCommand(ClearLogsAsync);

            LoadLogsCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadLogsCommand { get; }
        public IAsyncRelayCommand ClearLogsCommand { get; }

        private async Task LoadLogsAsync()
        {
            var logs = await _logRepository.GetRecentAsync(200);
            Logs.Clear();
            foreach (var log in logs)
            {
                Logs.Add(log);
            }
        }

        private async Task ClearLogsAsync()
        {
            var result = MessageBox.Show("Clear all rule execution logs?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _logRepository.DeleteOldAsync(DateTime.UtcNow);
                await LoadLogsAsync();
            }
        }
    }
}
