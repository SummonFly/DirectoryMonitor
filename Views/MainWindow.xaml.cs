using DirectoryMonitor.ViewModels;
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

        public MainWindow(
            MainWindowViewModel mainViewModel,
            RulesViewModel rulesViewModel,
            RuleLogViewModel ruleLogViewModel,
            ActionsViewModel actionsViewModel)
        {
            InitializeComponent();

            _mainViewModel = mainViewModel;
            _rulesViewModel = rulesViewModel;
            _ruleLogViewModel = ruleLogViewModel;
            _actionsViewModel = actionsViewModel;

            // Set DataContext for main content (Journal and Watched Paths tabs)
            DataContext = _mainViewModel;

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
    }
}