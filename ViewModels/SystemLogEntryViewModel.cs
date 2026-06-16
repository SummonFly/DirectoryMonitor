using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DirectoryMonitor.ViewModels
{
    public partial class SystemLogEntryViewModel : ObservableObject
    {
        private readonly SystemLogEntry _model;

        [ObservableProperty]
        private bool _isExpanded;

        public SystemLogEntry Model => _model;

        public SystemLogEntryViewModel(SystemLogEntry model)
        {
            _model = model;
        }

        public IRelayCommand ToggleExpandCommand => new RelayCommand(() => IsExpanded = !IsExpanded);

        public DateTime Timestamp => _model.Timestamp;
        public LogLevel Level => _model.Level;
        public string Source => _model.Source;
        public string Message => _model.Message;
        public string? Details => _model.Details;
        public bool HasDetails => !string.IsNullOrEmpty(Details);

        public string LevelIcon => Level switch
        {
            LogLevel.Info => "i",
            LogLevel.Warning => "!",
            LogLevel.Error => "X",
            LogLevel.Debug => "D",
            _ => "?"
        };

        public SolidColorBrush LevelColor => Level switch
        {
            LogLevel.Info => new SolidColorBrush(Colors.Green),
            LogLevel.Warning => new SolidColorBrush(Colors.Orange),
            LogLevel.Error => new SolidColorBrush(Colors.Red),
            LogLevel.Debug => new SolidColorBrush(Colors.Gray),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }
}
