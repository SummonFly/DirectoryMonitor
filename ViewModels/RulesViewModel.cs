using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
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
        private readonly IRuleActionRepository _ruleActionRepository;


        [ObservableProperty]
        private RuleItemViewModel? _selectedRule;

        [ObservableProperty]
        private ObservableCollection<RuleItemViewModel> _rules = new();

        public RulesViewModel(IRuleRepository ruleRepository, IRuleEngine ruleEngine, IActionRepository actionRepository, IRuleActionRepository ruleActionRepository)
        {
            _ruleRepository = ruleRepository;
            _actionRepository = actionRepository;
            _ruleEngine = ruleEngine;
            _ruleActionRepository = ruleActionRepository;

            LoadRulesCommand = new AsyncRelayCommand(LoadRulesAsync);
            AddRuleCommand = new AsyncRelayCommand(AddRuleAsync);
            EditRuleCommand = new AsyncRelayCommand(EditRuleAsync, () => SelectedRule != null);
            DeleteRuleCommand = new AsyncRelayCommand(DeleteRuleAsync, () => SelectedRule != null);
            ToggleRuleCommand = new AsyncRelayCommand(ToggleRuleAsync, () => SelectedRule != null);
            EditRuleWithParameterCommand = new AsyncRelayCommand<RuleItemViewModel>(EditRuleWithParameterAsync);
            DeleteRuleWithParameterCommand = new AsyncRelayCommand<RuleItemViewModel>(DeleteRuleWithParameterAsync);


            LoadRulesCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadRulesCommand { get; }
        public IAsyncRelayCommand AddRuleCommand { get; }
        public IAsyncRelayCommand EditRuleCommand { get; }
        public IAsyncRelayCommand DeleteRuleCommand { get; }
        public IAsyncRelayCommand ToggleRuleCommand { get; }
        public IAsyncRelayCommand<RuleItemViewModel> EditRuleWithParameterCommand { get; }
        public IAsyncRelayCommand<RuleItemViewModel> DeleteRuleWithParameterCommand { get; }

        partial void OnSelectedRuleChanged(RuleItemViewModel? value)
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
                Rules.Add(new RuleItemViewModel(rule));
            }
        }
        private async Task AddRuleAsync()
        {
            var dialog = new RuleEditorWindow();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultRule != null)
            {
                // 1. Save Rule first, get generated Id
                var rule = dialog.ResultRule;
                await _ruleRepository.AddAsync(rule);  // EF assigns Id after SaveChanges

                // 2. Save RuleAction associations
                if (dialog.AssignedActions.Any())
                {
                    foreach (var actionItem in dialog.AssignedActions)
                    {
                        var ruleAction = new RuleAction
                        {
                            RuleId = rule.Id,  // Now rule has valid Id
                            ActionId = actionItem.Id,
                            Order = actionItem.Order
                        };
                        await _ruleActionRepository.AddAsync(ruleAction);
                    }
                }

                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }

        private async Task EditRuleAsync()
        {
            if (SelectedRule == null) return;

            var rule = await _ruleRepository.GetByIdAsync(SelectedRule.Model.Id);
            if (rule == null) return;

            var dialog = new RuleEditorWindow(rule);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.ResultRule != null)
            {
                var updatedRule = dialog.ResultRule;
                updatedRule.Id = rule.Id;
                updatedRule.CreatedAt = rule.CreatedAt;

                // Update Rule
                await _ruleRepository.UpdateAsync(updatedRule);

                // Update RuleAction associations (delete old, add new)
                await _ruleActionRepository.DeleteByRuleIdAsync(rule.Id);

                if (dialog.AssignedActions.Any())
                {
                    foreach (var actionItem in dialog.AssignedActions)
                    {
                        var ruleAction = new RuleAction
                        {
                            RuleId = rule.Id,
                            ActionId = actionItem.Id,
                            Order = actionItem.Order
                        };
                        await _ruleActionRepository.AddAsync(ruleAction);
                    }
                }

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
                await _ruleRepository.DeleteAsync(SelectedRule.Model.Id);
                await LoadRulesAsync();
                await _ruleEngine.ReloadRulesAsync();
            }
        }

        private async Task ToggleRuleAsync()
        {
            if (SelectedRule == null) return;
            var rule = SelectedRule.Model;
            rule.IsActive = SelectedRule.IsActive;
            await _ruleRepository.UpdateAsync(rule);
            await _ruleEngine.ReloadRulesAsync();
        }

        private async Task EditRuleWithParameterAsync(RuleItemViewModel? item)
        {
            if (item == null) return;
            SelectedRule = item;
            await EditRuleAsync();
        }

        private async Task DeleteRuleWithParameterAsync(RuleItemViewModel? item)
        {
            if (item == null) return;
            SelectedRule = item;
            await DeleteRuleAsync();
        }
    }
}
