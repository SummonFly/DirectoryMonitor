using DirectoryMonitor.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

namespace DirectoryMonitor.Models.Entities
{
    public class EventLogEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string Path { get; set; } = string.Empty;

        [Required]
        public EventType EventType { get; set; }

        public string? OldPath { get; set; }

        public int? WatchedPathId { get; set; }

        public WatchedPath? WatchedPath { get; set; }

        public string EventTypeIcon => EventType switch
        {
            EventType.Created => "+",
            EventType.Changed => "~",
            EventType.Deleted => "x",
            EventType.Renamed => "→",
            _ => "?"
        };

        public SolidColorBrush EventTypeColor => EventType switch
        {
            EventType.Created => new SolidColorBrush(Colors.Green),
            EventType.Changed => new SolidColorBrush(Colors.Orange),
            EventType.Deleted => new SolidColorBrush(Colors.Red),
            EventType.Renamed => new SolidColorBrush(Colors.Blue),
            _ => new SolidColorBrush(Colors.Gray)
        };

        public bool HasOldPath => !string.IsNullOrEmpty(OldPath);
    }
}
