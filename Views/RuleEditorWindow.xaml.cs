using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Helpers;
using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
/// <summary>
/// Логика взаимодействия для RuleEditorWindow.xaml
/// </summary>

namespace DirectoryMonitor.Views
{
    public partial class RuleEditorWindow : Window
    {
        private Rule? _editingRule;
        private readonly IActionRepository _actionRepository;
        private readonly IRuleActionRepository _ruleActionRepository;
        private TreeNodeViewModel? _selectedNode;
        private List<ActionSelectionItem> _availableActions = new();
        private List<RuleActionItem> _assignedActions = new();

        public List<RuleActionItem> AssignedActions => _assignedActions;

        public Rule? ResultRule { get; private set; }

        public RuleEditorWindow(Rule? rule = null)
        {
            InitializeComponent();
            _editingRule = rule;

            _actionRepository = App.ServiceProvider.GetRequiredService<IActionRepository>();
            _ruleActionRepository = App.ServiceProvider.GetRequiredService<IRuleActionRepository>();

            Loaded += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadActionsAsync();

            if (_editingRule != null)
            {
                NameBox.Text = _editingRule.Name;
                PriorityBox.Text = _editingRule.Priority.ToString();
                EventTypeCombo.SelectedIndex = (int)_editingRule.EventType;

                if (!string.IsNullOrEmpty(_editingRule.ConditionsJson))
                {
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Formatting = Formatting.Indented
                    };

                    var condition = JsonConvert.DeserializeObject<ConditionNode>(_editingRule.ConditionsJson, settings);

                    if (condition != null)
                    {
                        var rootNode = new TreeNodeViewModel(condition);
                        ConditionsTree.ItemsSource = new List<TreeNodeViewModel> { rootNode };
                    }
                }
                else
                {
                    var rootGroup = new ConditionGroup { Operator = LogicalOperator.And };
                    var rootNode = new TreeNodeViewModel(rootGroup);
                    ConditionsTree.ItemsSource = new List<TreeNodeViewModel> { rootNode };
                }

                var ruleWithActions = await _actionRepository.GetRuleWithActionsAsync(_editingRule.Id);
                if (ruleWithActions?.RuleActions != null)
                {
                    _assignedActions = ruleWithActions.RuleActions
                        .OrderBy(ra => ra.Order)
                        .Select(ra => new RuleActionItem
                        {
                            Id = ra.Action.Id,
                            Name = ra.Action.Name,
                            ActionType = ra.Action.ActionType.ToString(),
                            Order = ra.Order
                        }).ToList();
                    RefreshActionsGrid();
                }
            }
            else
            {
                var rootGroup = new ConditionGroup { Operator = LogicalOperator.And };
                var rootNode = new TreeNodeViewModel(rootGroup);
                ConditionsTree.ItemsSource = new List<TreeNodeViewModel> { rootNode };
            }
        }

        private async Task LoadActionsAsync()
        {
            var actions = await _actionRepository.GetAllAsync();
            _availableActions = actions.Select(a => new ActionSelectionItem
            {
                Id = a.Id,
                Name = a.Name,
                ActionType = a.ActionType.ToString(),
                IsSelected = false
            }).ToList();
        }

        private void RefreshActionsGrid()
        {
            ActionsGrid.ItemsSource = null;
            ActionsGrid.ItemsSource = _assignedActions;
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNode?.Model is ConditionGroup group)
            {
                var newGroup = new ConditionGroup { Operator = LogicalOperator.And };
                var newVm = new TreeNodeViewModel(newGroup);
                group.Children.Add(newGroup);
                _selectedNode.Children.Add(newVm);
                _selectedNode.IsExpanded = true;
            }
            else if (_selectedNode == null && ConditionsTree.ItemsSource is List<TreeNodeViewModel> rootList && rootList.Any())
            {
                var rootVm = rootList.First();
                if (rootVm.Model is ConditionGroup rootGroup)
                {
                    var newGroup = new ConditionGroup { Operator = LogicalOperator.And };
                    var newVm = new TreeNodeViewModel(newGroup);
                    rootGroup.Children.Add(newGroup);
                    rootVm.Children.Add(newVm);
                    rootVm.IsExpanded = true;
                }
            }
        }

        private async void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNode?.Model is ConditionGroup targetGroup)
            {
                var dialog = new ConditionDialog();
                dialog.Owner = this;

                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var newVm = new TreeNodeViewModel(dialog.Result);
                    targetGroup.Children.Add(dialog.Result);
                    _selectedNode.Children.Add(newVm);
                    _selectedNode.IsExpanded = true;
                }
            }
            else if (_selectedNode == null && ConditionsTree.ItemsSource is List<TreeNodeViewModel> rootList && rootList.Any())
            {
                var rootVm = rootList.First();
                if (rootVm.Model is ConditionGroup rootGroup)
                {
                    var dialog = new ConditionDialog();
                    dialog.Owner = this;

                    if (dialog.ShowDialog() == true && dialog.Result != null)
                    {
                        var newVm = new TreeNodeViewModel(dialog.Result);
                        rootGroup.Children.Add(dialog.Result);
                        rootVm.Children.Add(newVm);
                        rootVm.IsExpanded = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a group node first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveNodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNode != null && _selectedNode.Model != null)
            {
                // Find parent and remove
                var parent = FindParentViewModel(_selectedNode);
                if (parent != null)
                {
                    parent.Children.Remove(_selectedNode);
                    if (parent.Model is ConditionGroup parentGroup)
                    {
                        parentGroup.Children.Remove(_selectedNode.Model);
                    }
                }
                else if (ConditionsTree.ItemsSource is List<TreeNodeViewModel> rootList && rootList.Contains(_selectedNode))
                {
                    // Can't remove root
                    MessageBox.Show("Cannot remove root group.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private TreeNodeViewModel? FindParentViewModel(TreeNodeViewModel child)
        {
            if (ConditionsTree.ItemsSource is List<TreeNodeViewModel> rootList)
            {
                foreach (var root in rootList)
                {
                    var result = FindParentInChildren(root, child);
                    if (result != null) return result;
                }
            }
            return null;
        }

        private TreeNodeViewModel? FindParentInChildren(TreeNodeViewModel parent, TreeNodeViewModel child)
        {
            if (parent.Children.Contains(child))
                return parent;

            foreach (var grandChild in parent.Children)
            {
                var result = FindParentInChildren(grandChild, child);
                if (result != null) return result;
            }
            return null;
        }

        private void ConditionsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedNode = e.NewValue as TreeNodeViewModel;
        }

        private void GroupOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.DataContext is TreeNodeViewModel node && node.Model is ConditionGroup group)
            {
                var selected = combo.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    var newOperator = selected.Content.ToString() switch
                    {
                        "AND" => LogicalOperator.And,
                        "OR" => LogicalOperator.Or,
                        "NOT" => LogicalOperator.Not,
                        _ => group.Operator
                    };

                    if (group.Operator != newOperator)
                    {
                        group.Operator = newOperator;
                        node.RefreshOperatorString();
                    }
                }
            }
        }

        private async void AssignActionButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ActionSelectionDialog(_availableActions);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && dialog.SelectedActionIds.Any())
            {
                foreach (var actionId in dialog.SelectedActionIds)
                {
                    var action = _availableActions.FirstOrDefault(a => a.Id == actionId);
                    if (action != null && !_assignedActions.Any(a => a.Id == actionId))
                    {
                        _assignedActions.Add(new RuleActionItem
                        {
                            Id = action.Id,
                            Name = action.Name,
                            ActionType = action.ActionType,
                            Order = _assignedActions.Count + 1
                        });
                    }
                }
                RefreshActionsGrid();
            }
        }

        private void RemoveActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionsGrid.SelectedItem is RuleActionItem selected)
            {
                _assignedActions.Remove(selected);
                for (int i = 0; i < _assignedActions.Count; i++)
                {
                    _assignedActions[i].Order = i + 1;
                }
                RefreshActionsGrid();
            }
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Rule name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string conditionsJson = "{}";

            if (ConditionsTree.ItemsSource is List<TreeNodeViewModel> rootList && rootList.Any())
            {
                var rootVm = rootList.First();
                rootVm.UpdateModel();

                conditionsJson = JsonConvert.SerializeObject(rootVm.Model, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    Formatting = Formatting.Indented
                });
            }

            var rule = new Rule
            {
                Id = _editingRule?.Id ?? 0,
                Name = NameBox.Text,
                IsActive = true,
                Priority = int.TryParse(PriorityBox.Text, out var priority) ? priority : 100,
                EventType = (EventType)EventTypeCombo.SelectedIndex,
                ConditionsJson = conditionsJson,
                CreatedAt = _editingRule?.CreatedAt ?? DateTime.UtcNow
            };

            ResultRule = rule;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
