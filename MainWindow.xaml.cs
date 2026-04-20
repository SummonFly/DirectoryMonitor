using DirectoryMonitor.Services;
using DirectoryMonitor.Services.Interfaces;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DirectoryMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly IWatcherManager _watcherManager;
        private readonly IJournalService _journalService;

        public MainWindow(IWatcherManager watcherManager, IJournalService journalService)
        {
            InitializeComponent();
            _watcherManager = watcherManager;
            _journalService = journalService;

            Loaded += async (s, e) =>
            {
                // Test: show recent events count
                var recentEvents = await _journalService.GetRecentEventsAsync(5);
                MessageBox.Show($"Recent events in DB: {recentEvents.Count}");

                // Start all watchers
                await _watcherManager.StartAllAsync();

                // Subscribe to events
                _watcherManager.FileEvent += async (sender, args) =>
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show($"Event: {args.ChangeType} - {args.FullPath}");
                    });
                };
            };
        }
    }
}