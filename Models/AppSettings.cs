namespace DirectoryMonitor.Models
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public int DebounceMilliseconds { get; set; } = 300;
        public int MaxLogEntries { get; set; } = 200;
        public int LogRetentionDays { get; set; } = 30;
        public bool ShowNotifications { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;
    }
}
