using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.Services.Interfaces;

namespace DirectoryMonitor.Services
{
    public class JournalService : IJournalService
    {
        private readonly IEventLogRepository _eventLogRepository;

        public JournalService(IEventLogRepository eventLogRepository)
        {
            _eventLogRepository = eventLogRepository;
        }

        public async Task LogEventAsync(string path, EventType eventType, string? oldPath = null, int? watchedPathId = null)
        {
            var entry = new EventLogEntry
            {
                Path = path,
                EventType = eventType,
                OldPath = oldPath,
                WatchedPathId = watchedPathId,
                Timestamp = DateTime.UtcNow
            };

            await _eventLogRepository.AddAsync(entry);
        }

        public async Task<List<EventLogEntry>> GetRecentEventsAsync(int count)
        {
            return await _eventLogRepository.GetRecentAsync(count);
        }

        public async Task<List<EventLogEntry>> GetFilteredEventsAsync(EventType? eventType, string? searchPath, DateTime? from, DateTime? to)
        {
            return await _eventLogRepository.GetByFilterAsync(eventType, searchPath, from, to);
        }

        public async Task<int> CleanupOldEventsAsync(DateTime before)
        {
            return await _eventLogRepository.DeleteOldAsync(before);
        }
    }
}
