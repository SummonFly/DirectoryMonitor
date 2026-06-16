using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DirectoryMonitor.Views
{
    /// <summary>
    /// Логика взаимодействия для AddWatchedPathWindow.xaml
    /// </summary>
    public partial class AddWatchedPathWindow : Window
    {
        private List<Rule> _allRules;
        private readonly IRuleRepository _ruleRepository;
        public WatchedPath? NewWatchedPath { get; private set; }
        public List<int> SelectedRuleIds { get; private set; } = new();

        public AddWatchedPathWindow()
        {
            InitializeComponent();
            _ruleRepository = App.ServiceProvider.GetRequiredService<IRuleRepository>();
            Loaded += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            _allRules = await _ruleRepository.GetAllAsync();
            RulesListBox.ItemsSource = _allRules;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select folder to monitor";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PathBox.Text = dialog.SelectedPath;
            }
        }

        private void WaitForCreationBox_Checked(object sender, RoutedEventArgs e)
        {
            BrowseButton.IsEnabled = false;
            PathBox.IsEnabled = true;
            PathBox.Text = "";
        }

        private void WaitForCreationBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BrowseButton.IsEnabled = true;
            PathBox.IsEnabled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PathBox.Text))
            {
                MessageBox.Show("Path is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedRuleIds = RulesListBox.SelectedItems
                .Cast<Rule>()
                .Select(r => r.Id)
                .ToList();

            NewWatchedPath = new WatchedPath
            {
                Path = PathBox.Text,
                IsActive = IsActiveBox.IsChecked == true,
                IncludeSubdirectories = IncludeSubdirectoriesBox.IsChecked == true,
                WaitForCreation = WaitForCreationBox.IsChecked == true,
                CreatedAt = DateTime.UtcNow
            };

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
