using DirectoryMonitor.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Models.Entities
{
    public class SystemLogEntry
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public LogLevel Level { get; set; } = LogLevel.Info;

        public string Source { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Details { get; set; }

        public int? RuleId { get; set; }

        public int? WatchedPathId { get; set; }
    }
}
