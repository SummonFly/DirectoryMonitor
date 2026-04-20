using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class EventLogRepository : IEventLogRepository
    {
        private readonly AppDbContext _context;

        public EventLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EventLogEntry entry)
        {
            entry.Timestamp = DateTime.UtcNow;
            await _context.EventLogEntries.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EventLogEntry>> GetRecentAsync(int count)
        {
            return await _context.EventLogEntries
                .Include(e => e.WatchedPath)
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<EventLogEntry>> GetByFilterAsync(string? eventType, string? searchPath, DateTime? from, DateTime? to)
        {
            var query = _context.EventLogEntries
                .Include(e => e.WatchedPath)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(e => e.EventType == eventType);
            }

            if (!string.IsNullOrWhiteSpace(searchPath))
            {
                query = query.Where(e => e.Path.Contains(searchPath) ||
                                         (e.OldPath != null && e.OldPath.Contains(searchPath)));
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.Timestamp <= to.Value);
            }

            return await query.OrderByDescending(e => e.Timestamp).ToListAsync();
        }

        public async Task<int> DeleteOldAsync(DateTime before)
        {
            var oldEntries = _context.EventLogEntries.Where(e => e.Timestamp < before);
            var count = await oldEntries.CountAsync();
            _context.EventLogEntries.RemoveRange(oldEntries);
            await _context.SaveChangesAsync();
            return count;
        }
    }
}
