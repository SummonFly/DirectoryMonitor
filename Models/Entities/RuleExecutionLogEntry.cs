using DirectoryMonitor.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DirectoryMonitor.Models.Entities
{
    public class RuleExecutionLogEntry
    {
        [Key]
        public int Id { get; set; }

        public int RuleId { get; set; }

        [Required]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        public string EventPath { get; set; } = string.Empty;

        [Required]
        public EventType EventType { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

        public int ExecutionTimeMs { get; set; }
    }
}
