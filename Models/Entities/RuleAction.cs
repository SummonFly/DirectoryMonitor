using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Models.Entities
{
    public class RuleAction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RuleId { get; set; }

        [Required]
        public int ActionId { get; set; }

        public int Order { get; set; } = 0;

        // Navigation
        public Rule Rule { get; set; } = null!;
        public Action Action { get; set; } = null!;
    }
}
