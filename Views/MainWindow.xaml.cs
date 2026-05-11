using DirectoryMonitor.Helpers;
using DirectoryMonitor.Services;
using DirectoryMonitor.Services.Interfaces;
using DirectoryMonitor.ViewModels;
using DirectoryMonitor.Views;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;

namespace DirectoryMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly RulesViewModel _rulesViewModel;
        private readonly RuleLogViewModel _ruleLogViewModel;
        private readonly ActionsViewModel _actionsViewModel;
        private readonly ISettingsService _settingsService;

        public MainWindow(
            MainWindowViewModel mainViewModel,
            RulesViewModel rulesViewModel,
            RuleLogViewModel ruleLogViewModel,
            ActionsViewModel actionsViewModel,
            ISettingsService settingsService)
        {
            InitializeComponent();

            TaskbarIconHelper.Instance = TrayIcon;

            _mainViewModel = mainViewModel;
            _rulesViewModel = rulesViewModel;
            _ruleLogViewModel = ruleLogViewModel;
            _actionsViewModel = actionsViewModel;
            _settingsService = settingsService;


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

            // Set DataContext for Rule Log tab
            if (RuleLogTab.Content is FrameworkElement logContent)
            {
                logContent.DataContext = _ruleLogViewModel;
            }

            // Set DataContext for Actions Tab
            if (ActionsTab.Content is FrameworkElement actionsContent)
            {
                actionsContent.DataContext = _actionsViewModel;
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