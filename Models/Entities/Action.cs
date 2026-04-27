using DirectoryMonitor.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Models.Entities
{
    public class Action
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ActionType ActionType { get; set; }

        [Required]
        public string ParametersJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<RuleAction> RuleActions { get; set; } = new List<RuleAction>();
    }
}
