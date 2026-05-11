using CommunityToolkit.Mvvm.ComponentModel;
using DirectoryMonitor.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.ViewModels
{
    public partial class WatchedPathItemViewModel : ObservableObject
    {
        private readonly WatchedPath _model;

        [ObservableProperty]
        private string _path;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _includeSubdirectories;

        [ObservableProperty]
        private string _status;

        [ObservableProperty]
        private string _statusTooltip;

        public WatchedPath Model => _model;

        public WatchedPathItemViewModel(WatchedPath model)
        {
            _model = model;
            _path = model.Path;
            _isActive = model.IsActive;
            _includeSubdirectories = model.IncludeSubdirectories;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (!IsActive)
            {
                Status = "Disabled";
                StatusTooltip = "Monitoring is disabled";
            }
            else if (Directory.Exists(Path))
            {
                Status = "Active";
                StatusTooltip = "Folder exists and being monitored";
            }
            else if (_model.WaitForCreation)
            {
                Status = "Waiting";
                StatusTooltip = "Folder does not exist, waiting for creation";
            }
            else
            {
                Status = "Missing";
                StatusTooltip = "Folder does not exist. Enable WaitForCreation or fix path.";
            }
        }

        public void UpdateFromModel()
        {
            Path = _model.Path;
            IsActive = _model.IsActive;
            IncludeSubdirectories = _model.IncludeSubdirectories;
            UpdateStatus();
        }

        public string StatusBackground => Status switch
        {
            "Active" => "#4CAF50",
            "Waiting" => "#FFC107",
            "Missing" => "#F44336",
            "Disabled" => "#9E9E9E",
            _ => "#9E9E9E"
        };

        public string StatusIcon => Status switch
        {
            "Active" => "●",
            "Waiting" => "◐",
            "Missing" => "○",
            "Disabled" => "⊘",
            _ => "?"
        };
    }
}
