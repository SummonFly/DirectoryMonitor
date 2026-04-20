using System.IO;

namespace DirectoryMonitor.Models.Actions
{
    public class ShowNotificationAction : ActionBase
    {
        public string Title { get; set; } = "Directory Monitor";
        public string Message { get; set; } = string.Empty;

        public override async Task ExecuteAsync(FileSystemEventArgs args, CancellationToken cancellationToken = default)
        {
            // Will be implemented with INotificationService
            // For now, just log to debug
            await Task.Run(() => System.Diagnostics.Debug.WriteLine($"Notification: {Title} - {Message}"), cancellationToken);
        }
    }
}
