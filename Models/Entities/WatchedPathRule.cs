using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Models.Entities
{
    public class WatchedPathRule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WatchedPathId { get; set; }

        [Required]
        public int RuleId { get; set; }

        // Navigation
        public WatchedPath WatchedPath { get; set; } = null!;
        public Rule Rule { get; set; } = null!;
    }
}
