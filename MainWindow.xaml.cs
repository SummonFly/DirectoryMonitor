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
        public MainWindow(IWatcherManager watcherManager)
        {
            InitializeComponent();
            _watcherManager = watcherManager;

            // Temporary test
            Loaded += async (s, e) =>
            {
                var testPath = new Models.Entities.WatchedPath
                {
                    Path = @"C:\Temp", // Change to an existing folder!
                    IsActive = true,
                    IncludeSubdirectories = false
                };

                await _watcherManager.StartWatchingAsync(testPath);
                _watcherManager.FileEvent += (sender, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Event: {args.ChangeType} - {args.FullPath}");
                    });
                };
            };
        }
    }
}