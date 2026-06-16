using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Data.Repositories;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DirectoryMonitor.ViewModels
{
    public partial class LogsViewModel : ObservableObject
    {
        private readonly ISystemLogRepository _logRepository;

        [ObservableProperty]
        private ObservableCollection<SystemLogEntryViewModel> _logs = new();

        public LogsViewModel(ISystemLogRepository logRepository)
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
            var logs = await _logRepository.GetRecentAsync(500);
            Logs.Clear();
            foreach (var log in logs.OrderByDescending(l => l.Timestamp))
            {
                Logs.Add(new SystemLogEntryViewModel(log));
            }
        }

        private async Task ClearLogsAsync()
        {
            var result = MessageBox.Show("Clear all system logs?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _logRepository.DeleteAllAsync();
                await LoadLogsAsync();
            }
        }
    }
}
