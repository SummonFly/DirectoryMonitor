using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Data.Repositories
{
    public interface ISystemLogRepository
    {
        Task AddAsync(SystemLogEntry entry);
        Task<List<SystemLogEntry>> GetRecentAsync(int limit);
        Task<List<SystemLogEntry>> GetByLevelAsync(LogLevel level, int limit);
        Task DeleteAllAsync();
        Task DeleteOldAsync(DateTime before);
    }
}
