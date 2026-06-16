using DirectoryMonitor.Helpers;
using DirectoryMonitor.Services.Interfaces;
using DirectoryMonitor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;

namespace DirectoryMonitor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly RulesViewModel _rulesViewModel;
        private readonly ActionsViewModel _actionsViewModel;
        private readonly LogsViewModel _logsViewModel;
        private readonly ISettingsService _settingsService;

        public MainWindow(
            MainWindowViewModel mainViewModel,
            RulesViewModel rulesViewModel,
            ActionsViewModel actionsViewModel,
            LogsViewModel logsViewModel,
            ISettingsService settingsService)
        {
            InitializeComponent();

            TaskbarIconHelper.Instance = TrayIcon;

            _mainViewModel = mainViewModel;
            _rulesViewModel = rulesViewModel;
            _actionsViewModel = actionsViewModel;
            _settingsService = settingsService;
            _logsViewModel = logsViewModel;


            // Set DataContext for main content (Journal and Watched Paths tabs)
            DataContext = _mainViewModel;

            // Get TrayIconViewModel from service provider
            var trayIconViewModel = App.ServiceProvider.GetRequiredService<TrayIconViewModel>();
            TrayIcon.DataContext = trayIconViewModel;


            // Set DataContext for Rules tab
            if (RulesTab.Content is FrameworkElement rulesContent)
            {
                rulesContent.DataContext = _rulesViewModel;
            }

            // Set DataContext for Actions Tab
            if (ActionsTab.Content is FrameworkElement actionsContent)
            {
                actionsContent.DataContext = _actionsViewModel;
            }

            // Set DataContext for System Logs tab
            if (SystemLogsTab.Content is FrameworkElement logsContent)
            {
                logsContent.DataContext = _logsViewModel;
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OpenSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(_settingsService);
            settings.Owner = this;
            settings.ShowDialog();
        }
    }
}