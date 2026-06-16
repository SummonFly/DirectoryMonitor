using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace DirectoryMonitor.Views
{
    /// <summary>
    /// Логика взаимодействия для WatchedPathEditorWindow.xaml
    /// </summary>
    public partial class WatchedPathEditorWindow : Window
    {
        private WatchedPath _watchedPath;
        private List<Rule> _allRules;
        private List<int> _selectedRuleIds;
        private readonly IWatchedPathRuleRepository _watchedPathRuleRepository;
        private readonly IRuleRepository _ruleRepository;

        public WatchedPath? UpdatedWatchedPath { get; private set; }

        public WatchedPathEditorWindow(WatchedPath watchedPath)
        {
            InitializeComponent();

            _watchedPath = watchedPath;
            _watchedPathRuleRepository = App.ServiceProvider.GetRequiredService<IWatchedPathRuleRepository>();
            _ruleRepository = App.ServiceProvider.GetRequiredService<IRuleRepository>();

            Loaded += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            PathBox.Text = _watchedPath.Path;
            IncludeSubdirectoriesBox.IsChecked = _watchedPath.IncludeSubdirectories;
            IsActiveBox.IsChecked = _watchedPath.IsActive;
            WaitForCreationBox.IsChecked = _watchedPath.WaitForCreation;

            // Load all rules
            _allRules = await _ruleRepository.GetAllAsync();

            // Load selected rule IDs for this path
            _selectedRuleIds = await _watchedPathRuleRepository.GetRuleIdsForWatchedPathAsync(_watchedPath.Id);

            // Populate listbox with checkboxes
            RulesListBox.ItemsSource = _allRules;
            RulesListBox.SelectionMode = SelectionMode.Multiple;

            // Select previously selected rules
            for (int i = 0; i < _allRules.Count; i++)
            {
                if (_selectedRuleIds.Contains(_allRules[i].Id))
                {
                    RulesListBox.SelectedItems.Add(_allRules[i]);
                }
            }
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Update WatchedPath properties
            _watchedPath.IncludeSubdirectories = IncludeSubdirectoriesBox.IsChecked == true;
            _watchedPath.IsActive = IsActiveBox.IsChecked == true;
            _watchedPath.UpdatedAt = DateTime.UtcNow;
            _watchedPath.WaitForCreation = WaitForCreationBox.IsChecked == true;

            // Get selected rule IDs
            var selectedRuleIds = RulesListBox.SelectedItems
                .Cast<Rule>()
                .Select(r => r.Id)
                .ToList();

            // Update associations in database
            await _watchedPathRuleRepository.UpdateRulesForWatchedPathAsync(_watchedPath.Id, selectedRuleIds);

            UpdatedWatchedPath = _watchedPath;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select folder to monitor";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PathBox.Text = dialog.SelectedPath;
                _watchedPath.Path = dialog.SelectedPath;
            }
        }
    }
}
