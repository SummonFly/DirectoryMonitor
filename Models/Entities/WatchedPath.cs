using System.ComponentModel.DataAnnotations;

namespace DirectoryMonitor.Models.Entities
{
    public class WatchedPath
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Path { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IncludeSubdirectories { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<WatchedPathRule> WatchedPathRules { get; set; } = new List<WatchedPathRule>();
    }
}
