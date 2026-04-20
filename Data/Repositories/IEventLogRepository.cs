using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IEventLogRepository
    {
        Task AddAsync(EventLogEntry entry);
        Task<List<EventLogEntry>> GetRecentAsync(int count);
        Task<List<EventLogEntry>> GetByFilterAsync(string? eventType, string? searchPath, DateTime? from, DateTime? to);
        Task<int> DeleteOldAsync(DateTime before);
    }
}
