using System.ComponentModel.DataAnnotations;

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
        public string EventType { get; set; } = string.Empty;

        public string? OldPath { get; set; }

        public int? WatchedPathId { get; set; }

        public WatchedPath? WatchedPath { get; set; }
    }
}
