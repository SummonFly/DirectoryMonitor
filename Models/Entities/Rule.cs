using DirectoryMonitor.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DirectoryMonitor.Models.Entities
{
    public class Rule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int Priority { get; set; } = 100;

        [Required]
        public EventType EventType { get; set; }

        [Required]
        public string ConditionsJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<WatchedPathRule> WatchedPathRules { get; set; } = new List<WatchedPathRule>();
        public ICollection<RuleAction> RuleActions { get; set; } = new List<RuleAction>();
    }
}
