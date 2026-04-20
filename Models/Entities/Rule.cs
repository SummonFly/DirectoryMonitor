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
        public string DefinitionJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
