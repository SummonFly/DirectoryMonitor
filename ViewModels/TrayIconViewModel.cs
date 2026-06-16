using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace DirectoryMonitor.ViewModels
{
    public partial class TrayIconViewModel
    {
        public TrayIconViewModel()
        {
            ShowWindowCommand = new RelayCommand(ShowWindow);
            ExitCommand = new RelayCommand(Exit);
        }

        public IRelayCommand ShowWindowCommand { get; }
        public IRelayCommand ExitCommand { get; }

        private void ShowWindow()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
