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
    public partial class RulesViewModel : ObservableObject
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly IActionRepository _actionRepository;
        private readonly IRuleEngine _ruleEngine;

        [ObservableProperty]
        private ObservableCollection<RuleListItemViewModel> _rules = new();

        [ObservableProperty]
        private RuleListItemViewModel? _selectedRule;

        public RulesViewModel(IRuleRepository ruleRepository, IRuleEngine ruleEngine, IActionRepository actionRepository)
        {
            _ruleRepository = ruleRepository;
            _actionRepository = actionRepository;
            _ruleEngine = ruleEngine;

            LoadRulesCommand = new AsyncRelayCommand(LoadRulesAsync);
            AddRuleCommand = new AsyncRelayCommand(AddRuleAsync);
            EditRuleCommand = new AsyncRelayCommand(EditRuleAsync, () => SelectedRule != null);
            DeleteRuleCommand = new AsyncRelayCommand(DeleteRuleAsync, () => SelectedRule != null);
            ToggleRuleCommand = new AsyncRelayCommand(ToggleRuleAsync, () => SelectedRule != null);


            LoadRulesCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadRulesCommand { get; }
        public IAsyncRelayCommand AddRuleCommand { get; }
        public IAsyncRelayCommand EditRuleCommand { get; }
        public IAsyncRelayCommand DeleteRuleCommand { get; }
        public IAsyncRelayCommand ToggleRuleCommand { get; }

        partial void OnSelectedRuleChanged(RuleListItemViewModel? value)
        {
            EditRuleCommand.NotifyCanExecuteChanged();
            DeleteRuleCommand.NotifyCanExecuteChanged();
            ToggleRuleCommand.NotifyCanExecuteChanged();
        }



        private async Task LoadRulesAsync()
        {
            var rules = await _ruleRepository.GetAllAsync();
            Rules.Clear();
            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                Rules.Add(new RuleListItemViewModel(rule));
            }
        }
        private async Task AddRuleAsync()
        {
            var dialog = new RuleEditorWindow();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultRule != null)
            {
                await _actionRepository.AddRuleWithActionsAsync(dialog.ResultRule, dialog.AssignedActions);
                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }

        private async Task EditRuleAsync()
        {
            if (SelectedRule == null) return;

            var rule = await _actionRepository.GetRuleWithActionsAsync(SelectedRule.Id);
            if (rule == null) return;

            var dialog = new RuleEditorWindow(rule);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultRule != null)
            {
                dialog.ResultRule.Id = rule.Id;
                dialog.ResultRule.CreatedAt = rule.CreatedAt;
                await _actionRepository.UpdateRuleWithActionsAsync(dialog.ResultRule, dialog.AssignedActions);
                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }

        private async Task DeleteRuleAsync()
        {
            if (SelectedRule == null) return;

            var result = MessageBox.Show($"Delete rule '{SelectedRule.Name}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _ruleRepository.DeleteAsync(SelectedRule.Id);
                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }

        private async Task ToggleRuleAsync()
        {
            if (SelectedRule == null) return;

            var rule = SelectedRule.Rule;
            rule.IsActive = SelectedRule.IsActive;
            await _ruleRepository.UpdateAsync(rule);
            await _ruleEngine.ReloadRulesAsync();
        }
    }
}
