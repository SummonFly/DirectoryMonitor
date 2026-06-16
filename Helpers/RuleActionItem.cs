namespace DirectoryMonitor.Helpers
{
    public class RuleActionItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
