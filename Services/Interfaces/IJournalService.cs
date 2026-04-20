using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;

namespace DirectoryMonitor.Services.Interfaces
{
    public interface IJournalService
    {
        Task LogEventAsync(string path, EventType eventType, string? oldPath = null, int? watchedPathId = null);
        Task<List<EventLogEntry>> GetRecentEventsAsync(int count);
        Task<List<EventLogEntry>> GetFilteredEventsAsync(EventType? eventType, string? searchPath, DateTime? from, DateTime? to);
        Task<int> CleanupOldEventsAsync(DateTime before);
    }
}
