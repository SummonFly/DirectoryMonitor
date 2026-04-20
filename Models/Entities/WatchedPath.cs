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

        public string? FileExtensionsFilter { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
