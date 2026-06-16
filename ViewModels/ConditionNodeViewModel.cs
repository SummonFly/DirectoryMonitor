using CommunityToolkit.Mvvm.ComponentModel;
using DirectoryMonitor.Models.Conditions;
using System.Collections.ObjectModel;

namespace DirectoryMonitor.ViewModels
{
    public partial class ConditionNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isExpanded = true;

        public ConditionNode Model { get; set; }
        public ObservableCollection<ConditionNodeViewModel> Children { get; set; } = new();

        public bool IsItemType => Model is ItemTypeCondition;

        public ConditionNodeViewModel(ConditionNode model)
        {
            Model = model;

            if (model is ConditionGroup group)
            {
                foreach (var child in group.Children)
                {
                    Children.Add(new ConditionNodeViewModel(child));
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
                    group.Children.Add(childVm.Model);
                }
            }
        }
    }
}
