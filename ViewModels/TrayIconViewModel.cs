using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace DirectoryMonitor.ViewModels
{
    public partial class TrayIconViewModel
    {
        private readonly MainWindow _mainWindow;

        public TrayIconViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            ShowWindowCommand = new RelayCommand(ShowWindow);
            ExitCommand = new RelayCommand(Exit);
        }

        public IRelayCommand ShowWindowCommand { get; }
        public IRelayCommand ExitCommand { get; }

        private void ShowWindow()
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
