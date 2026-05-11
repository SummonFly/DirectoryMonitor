using DirectoryMonitor.Helpers;
using DirectoryMonitor.Services.Interfaces;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace DirectoryMonitor.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowToast(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TaskbarIconHelper.Instance?.ShowBalloonTip(title, message, BalloonIcon.Info);
            });
        }
    }
}
