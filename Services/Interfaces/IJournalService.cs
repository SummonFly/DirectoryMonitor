using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Services.Interfaces
{
    public interface IJournalService
    {
        Task LogEventAsync(string path, string eventType, string? oldPath = null, int? watchedPathId = null);
        Task<List<EventLogEntry>> GetRecentEventsAsync(int count);
        Task<List<EventLogEntry>> GetFilteredEventsAsync(string? eventType, string? searchPath, DateTime? from, DateTime? to);
        Task<int> CleanupOldEventsAsync(DateTime before);
    }
}
