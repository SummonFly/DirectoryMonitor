namespace DirectoryMonitor.Helpers
{
    public class ActionSelectionItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
