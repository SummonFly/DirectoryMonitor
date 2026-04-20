using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IEventLogRepository
    {
        Task AddAsync(EventLogEntry entry);
        Task<List<EventLogEntry>> GetRecentAsync(int count);
        Task<List<EventLogEntry>> GetByFilterAsync(EventType? eventType, string? searchPath, DateTime? from, DateTime? to);
        Task<int> DeleteOldAsync(DateTime before);
    }
}
