using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Services.Interfaces
{
    public interface ISystemLogService
    {
        Task InfoAsync(string source, string message, string? details = null);
        Task WarningAsync(string source, string message, string? details = null);
        Task ErrorAsync(string source, string message, string? details = null);
        Task DebugAsync(string source, string message, string? details = null);
    }
}
