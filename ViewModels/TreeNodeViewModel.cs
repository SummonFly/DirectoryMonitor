using CommunityToolkit.Mvvm.ComponentModel;
using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Enums;
using System.Collections.ObjectModel;

namespace DirectoryMonitor.ViewModels
{
    public partial class TreeNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isExpanded = true;

        public ConditionNode? Model { get; set; }
        public ObservableCollection<TreeNodeViewModel> Children { get; set; } = new();

        // For display
        public bool IsGroup => Model is ConditionGroup;
        public bool IsExtension => Model is ExtensionCondition;
        public bool IsFileName => Model is FileNameCondition;
        public bool IsSize => Model is SizeCondition;

        public string? OperatorString
        {
            get
            {
                if (Model is ConditionGroup group)
                    return group.Operator.ToString().ToUpperInvariant(); ;
                return null;
            }
            set
            {
                if (Model is ConditionGroup group && Enum.TryParse<LogicalOperator>(value, true, out var op))
                {
                    if (group.Operator != op)
                    {
                        group.Operator = op;
                        OnPropertyChanged(nameof(OperatorString));
                    }
                }
            }
        }

        public string? ExtensionsText
        {
            get
            {
                if (Model is ExtensionCondition ext)
                    return string.Join(", ", ext.Extensions);
                return null;
            }
        }

        public string? Pattern
        {
            get
            {
                if (Model is FileNameCondition fn)
                    return $"{fn.MatchType}: \"{fn.Pattern}\"";
                return null;
            }
        }

        public string? DisplayText
        {
            get
            {
                if (Model is SizeCondition size)
                    return $"{size.Operator} {size.Value} {size.Unit}";
                return null;
            }
        }

        public TreeNodeViewModel(ConditionNode model)
        {
            Model = model;

            if (model is ConditionGroup group)
            {
                foreach (var child in group.Children)
                {
                    Children.Add(new TreeNodeViewModel(child));
                }
            }
        }

        public void UpdateModel()
        {
            if (Model is ConditionGroup group)
            {
                group.Children.Clear();
                foreach (var childVm in Children)
                {
                    childVm.UpdateModel();
                    group.Children.Add(childVm.Model!);
                }
            }
        }

        public void RefreshOperatorString()
        {
            OnPropertyChanged(nameof(OperatorString));
        }
    }
}
