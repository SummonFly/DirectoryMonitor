using CommunityToolkit.Mvvm.ComponentModel;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;

namespace DirectoryMonitor.ViewModels
{
    public partial class RuleListItemViewModel : ObservableObject
    {
        private readonly Rule _rule;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _eventType = string.Empty;

        [ObservableProperty]
        private int _priority;

        public int Id => _rule.Id;
        public Rule Rule => _rule;

        public RuleListItemViewModel(Rule rule)
        {
            _rule = rule;
            _isActive = rule.IsActive;
            _name = rule.Name;
            _eventType = rule.EventType.ToString();
            _priority = rule.Priority;
        }

        public void UpdateFromRule()
        {
            IsActive = _rule.IsActive;
            Name = _rule.Name;
            EventType = _rule.EventType.ToString();
            Priority = _rule.Priority;
        }
    }
}
